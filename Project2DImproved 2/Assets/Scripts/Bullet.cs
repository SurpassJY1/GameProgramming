using System.Collections.Generic;
using UnityEngine;

/// Player projectile. It combines movement, wall/enemy hit detection, and upgrade effects
/// such as pierce, burn, slow, and explosive damage.
///
/// Authorship note:
/// - Student-owned implementation: projectile rules, collision behaviour, upgrade effect handling,
///   and final integration with PlayerCombat.
/// - AI-assisted support: review suggestions and explanatory comments for the hit-detection flow.
public class Bullet : MonoBehaviour
{
    public float speed = 14f;
    public int damage = 1;
    public float lifetime = 1.5f;
    public float radius = 0.08f;
    public LayerMask wallMask;
    public int pierceRemaining;
    public int burnDamage;
    public float burnDuration;
    public float slowMultiplier = 1f;
    public float slowDuration;
    public float explosionRadius;
    public int explosionDamage;
    public AudioClip impactClip;

    float born;
    readonly List<Enemy> hitEnemies = new List<Enemy>();

    // What: Record the spawn time so lifetime expiry can be checked later.
    // Human: Tuned bullet lifetime for room-sized combat.
    // AI: Helped keep lifetime tracking simple and local to the projectile.
    void Start() { born = Time.time; }

    // What: Move the bullet, detect walls/enemies, and expire when lifetime runs out.
    // Human: Chose fast projectile combat with wall collision.
    // AI: Helped use manual casts to avoid tunnelling at high speeds.
    void Update()
    {
        if (Time.time - born >= lifetime)
        {
            HitAndDestroy(transform.position);
            return;
        }

        Vector2 current = transform.position;
        Vector2 direction = transform.up;
        float distance = speed * Time.deltaTime;

        // Manual casts prevent fast bullets from tunnelling through small enemies or walls.
        RaycastHit2D wallHit = wallMask.value != 0
            ? Physics2D.CircleCast(current, radius, direction, distance, wallMask)
            : default;
        RaycastHit2D enemyHit = FindEnemyHit(current, direction, distance);

        if (wallHit.collider != null &&
            (enemyHit.collider == null || wallHit.distance <= enemyHit.distance))
        {
            HitAndDestroy(wallHit.point);
            return;
        }

        if (enemyHit.collider != null)
        {
            Enemy enemy = enemyHit.collider.GetComponent<Enemy>();
            HitEnemy(enemy, enemyHit.point, direction);
            return;
        }

        transform.position = current + direction * distance;
    }

    // What: Find the nearest enemy hit along this frame's bullet movement.
    // Human: Designed piercing shots to pass through multiple enemies.
    // AI: Helped skip enemies already hit by the same piercing projectile.
    RaycastHit2D FindEnemyHit(Vector2 current, Vector2 direction, float distance)
    {
        // CircleCastAll can return several enemies. Pick the closest unhit enemy so piercing
        // projectiles move through targets in physical order.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(current, radius, direction, distance);
        RaycastHit2D bestHit = default;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].collider != null ? hits[i].collider.GetComponent<Enemy>() : null;
            if (enemy != null && hitEnemies.Contains(enemy)) continue;
            if (enemy == null || hits[i].distance >= bestDistance) continue;

            bestHit = hits[i];
            bestDistance = hits[i].distance;
        }

        return bestHit;
    }

    // What: Handle backup trigger collisions with enemies or walls.
    // Human: Required reliable bullet contact even around spawn overlaps.
    // AI: Helped keep trigger logic consistent with manual cast logic.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger callbacks are a backup for overlap cases at spawn or after piercing. The Update
        // cast handles most high-speed travel.
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            HitEnemy(enemy, transform.position, transform.up);
            return;
        }

        if (wallMask.value != 0 && ((1 << other.gameObject.layer) & wallMask.value) != 0)
            HitAndDestroy(transform.position);
    }

    // What: Play impact feedback, apply explosion damage if enabled, and remove the bullet.
    // Human: Chose impact/explosion feedback for weapon upgrades.
    // AI: Helped centralize the destroy path so wall hits and timeout hits share behaviour.
    void HitAndDestroy(Vector3 position)
    {
        PlayImpact(position);
        Explode(position);
        SpawnImpact(position);
        Destroy(gameObject);
    }

    // What: Damage one enemy, apply upgrade statuses, then either pierce or destroy the bullet.
    // Human: Designed direct damage, burn, slow, explosion, and piercing upgrades.
    // AI: Helped order effects so hit feedback and status application stay predictable.
    void HitEnemy(Enemy enemy, Vector3 hitPoint, Vector2 direction)
    {
        if (enemy == null || hitEnemies.Contains(enemy)) return;

        // Track hit enemies so piercing shots cannot damage the same target repeatedly.
        hitEnemies.Add(enemy);
        enemy.TakeDamage(damage, transform.position);
        PlayImpact(hitPoint);

        if (burnDamage > 0 && burnDuration > 0f) enemy.ApplyBurn(burnDamage, burnDuration);
        if (slowDuration > 0f) enemy.ApplySlow(slowMultiplier, slowDuration);

        Explode(hitPoint);
        SpawnImpact(hitPoint);

        if (pierceRemaining > 0)
        {
            pierceRemaining--;
            transform.position = hitPoint + (Vector3)(direction.normalized * (radius + 0.04f));
            return;
        }

        Destroy(gameObject);
    }

    // What: Play the bullet impact sound at the hit position.
    // Human: Selected the impact audio asset.
    // AI: Helped use PlayClipAtPoint to avoid managing audio children on bullets.
    void PlayImpact(Vector3 position)
    {
        // PlayClipAtPoint creates a temporary AudioSource at the impact position, which keeps the
        // bullet prefab free of extra child audio objects.
        if (impactClip != null) AudioSource.PlayClipAtPoint(impactClip, position, 0.45f);
    }

    // What: Apply area damage around an impact when Explosive Shot is active.
    // Human: Designed explosive shot as an optional weapon upgrade.
    // AI: Helped guard the method so normal bullets have no area damage cost.
    void Explode(Vector3 position)
    {
        // Explosive Shot is optional. When inactive, radius/damage stay at zero and this method
        // exits immediately, so normal bullets do not need a separate class.
        if (explosionRadius <= 0f || explosionDamage <= 0) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy == null) continue;

            enemy.TakeDamage(explosionDamage, position);
        }

        SpawnExplosion(position);
    }

    // What: Create the visible blast circle for an explosive hit.
    // Human: Chose the explosion color/scale feedback.
    // AI: Helped reuse BulletImpact for fade-out and cleanup.
    void SpawnExplosion(Vector3 position)
    {
        GameObject explosion = new GameObject("BulletExplosion");
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * Mathf.Max(0.2f, explosionRadius * 2f);

        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(1f, 0.42f, 0.12f, 0.42f), 32);
        sr.sortingOrder = 4;

        BulletImpact effect = explosion.AddComponent<BulletImpact>();
        effect.life = 0.22f;
        effect.drift = Vector2.zero;
    }

    // What: Create the small spark shown for every bullet impact.
    // Human: Chose the hit spark visual feedback.
    // AI: Helped keep impact creation lightweight and self-cleaning.
    void SpawnImpact(Vector3 position)
    {
        // Tiny hit spark for normal bullet contacts. BulletImpact owns its own fade and cleanup.
        GameObject impact = new GameObject("BulletImpact");
        impact.transform.position = position;
        impact.transform.localScale = Vector3.one * 0.12f;

        SpriteRenderer sr = impact.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(1f, 0.86f, 0.25f, 0.85f), 16);
        sr.sortingOrder = 5;

        BulletImpact effect = impact.AddComponent<BulletImpact>();
        effect.drift = -transform.up * 0.35f;
    }
}

/// Short-lived visual effect used by bullet hits, explosions, and enemy ability pulses.
///
/// Authorship note:
/// - Student-owned implementation: impact feedback requirement and final fade/drift tuning.
/// - AI-assisted support: review suggestions and comments explaining why one tiny component is
///   reused for several temporary VFX objects.
public class BulletImpact : MonoBehaviour
{
    public Vector2 drift;
    public float life = 0.16f;

    float t;
    SpriteRenderer sr;

    // What: Cache the renderer used by this temporary visual effect.
    // Human: Tuned the impact effect appearance.
    // AI: Helped keep effect setup minimal.
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // What: Move, shrink, fade, and destroy this temporary visual effect.
    // Human: Chose the hit/explosion fade timing.
    // AI: Helped make each effect responsible for its own cleanup.
    void Update()
    {
        // The effect is intentionally self-cleaning. Runtime-created VFX objects do not need to be
        // tracked by GameBootstrap or pooled for this small project.
        t += Time.deltaTime;
        transform.position += (Vector3)(drift * Time.deltaTime);
        transform.localScale *= 0.94f;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(1f - (t / life));
            sr.color = c;
        }

        if (t >= life) Destroy(gameObject);
    }
}
