using UnityEngine;

/// Mouse-aimed combat controller for the player.
/// Weapon upgrades change the values stored here; each spawned Bullet receives a snapshot of the
/// current build so existing bullets do not change mid-flight.
///
/// Authorship note:
/// - Student-owned implementation: shooting behaviour, upgrade effects, and final integration with
///   the level-up flow.
/// - AI-assisted support: review suggestions and comment wording that explain how upgrade values
///   are transferred into projectile instances.
public class PlayerCombat : MonoBehaviour
{
    [Header("Shooting")]
    public float fireCooldown = 0.22f;
    public float bulletSpeed = 12f;
    public int damage = 1;
    public float bulletLifetime = 1.2f;
    public int extraProjectiles;
    public float spreadAngle = 24f;
    public float minFireCooldown = 0.11f;
    public float rapidFireMultiplier = 0.84f;
    public int damagePerUpgrade = 1;
    public int maxDamage = 4;
    public int pierceCount;
    public bool hasBurnShot;
    public bool hasSlowShot;
    public bool hasExplosiveShot;
    public int burnDamage = 1;
    public float burnDuration = 2.4f;
    public float slowMultiplier = 0.55f;
    public float slowDuration = 1.8f;
    public float explosionRadius = 1.25f;
    public int explosionDamage = 1;

    [Header("Setup")]
    public GameObject bulletPrefab;
    public float muzzleOffset = 0.48f;
    public AudioClip shootClip;

    Camera mainCamera;
    AudioSource audioSrc;
    float cooldownTimer;
    float baseFireCooldown;
    int baseDamage;
    int baseBurnDamage;
    float baseBurnDuration;
    float baseSlowMultiplier;
    float baseSlowDuration;
    float baseExplosionRadius;
    int baseExplosionDamage;
    Vector2 aimDirection = Vector2.up;

    // What: Cache camera/audio references needed by aiming and shooting.
    // Human: Chose mouse-aimed combat for the player.
    // AI: Helped keep component lookup simple for a runtime-built Player object.
    void Awake()
    {
        mainCamera = Camera.main;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    // What: Store baseline weapon stats and subscribe to run resets.
    // Human: Tuned the starting weapon values.
    // AI: Suggested saving base values so restarts reset all upgrades reliably.
    void Start()
    {
        baseFireCooldown = fireCooldown;
        baseDamage = damage;
        baseBurnDamage = burnDamage;
        baseBurnDuration = burnDuration;
        baseSlowMultiplier = slowMultiplier;
        baseSlowDuration = slowDuration;
        baseExplosionRadius = explosionRadius;
        baseExplosionDamage = explosionDamage;
        if (GameManager.I != null) GameManager.I.OnGameStarted += ResetForNewRun;
        ResetForNewRun();
    }

    // What: Unsubscribe from GameManager when this combat component is destroyed.
    // Human: Owned runtime player lifecycle.
    // AI: Suggested cleanup for event safety.
    void OnDestroy()
    {
        if (GameManager.I != null) GameManager.I.OnGameStarted -= ResetForNewRun;
    }

    // What: Aim at the mouse, tick cooldown, and fire while the mouse button is held.
    // Human: Chose hold-to-shoot gameplay.
    // AI: Helped gate combat by GameManager phase.
    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        AimAtMouse();

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0)) TryFire();
    }

    // What: Convert mouse position into a world-space aim direction.
    // Human: Chose direct mouse aiming and sprite rotation.
    // AI: Helped document orthographic world conversion.
    void AimAtMouse()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Mouse position is converted to world space because the game uses a top-down orthographic
        // camera. The player transform points up along the current aim direction.
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        Vector2 toMouse = mouseWorld - transform.position;
        if (toMouse.sqrMagnitude < 0.0001f) return;

        aimDirection = toMouse.normalized;
        transform.up = aimDirection;
    }

    // What: Spawn one volley if the fire cooldown has expired.
    // Human: Tuned the base fire cadence.
    // AI: Helped set cooldown before spawning to avoid same-frame double fire.
    void TryFire()
    {
        if (cooldownTimer > 0f) return;

        // The cooldown is assigned before spawning bullets so recursive or event-driven calls cannot
        // fire twice in the same frame.
        cooldownTimer = fireCooldown;
        SpawnBulletVolley();

        if (shootClip != null) audioSrc.PlayOneShot(shootClip, 0.45f);
    }

    // What: Apply one selected weapon upgrade to the current combat stats.
    // Human: Designed the upgrade effects and their balance caps.
    // AI: Helped organize the switch and document caps that prevent runaway output.
    public void ApplyUpgrade(WeaponUpgradeKind upgrade)
    {
        // Upgrade effects are capped to avoid runaway values during longer floor runs.
        switch (upgrade)
        {
            case WeaponUpgradeKind.ExtraProjectile:
                extraProjectiles = Mathf.Min(2, extraProjectiles + 1);
                break;
            case WeaponUpgradeKind.RapidFire:
                fireCooldown = Mathf.Max(minFireCooldown, fireCooldown * rapidFireMultiplier);
                cooldownTimer = Mathf.Min(cooldownTimer, fireCooldown);
                break;
            case WeaponUpgradeKind.DamageUp:
                damage = Mathf.Min(maxDamage, damage + Mathf.Max(1, damagePerUpgrade));
                break;
            case WeaponUpgradeKind.PiercingShot:
                pierceCount = Mathf.Min(1, pierceCount + 1);
                break;
            case WeaponUpgradeKind.BurnShot:
                hasBurnShot = true;
                burnDamage = Mathf.Min(2, burnDamage + 1);
                burnDuration = Mathf.Min(4f, burnDuration + 0.45f);
                break;
            case WeaponUpgradeKind.SlowShot:
                hasSlowShot = true;
                slowDuration = Mathf.Min(4f, slowDuration + 0.45f);
                slowMultiplier = Mathf.Max(0.32f, slowMultiplier - 0.06f);
                break;
            case WeaponUpgradeKind.ExplosiveShot:
                hasExplosiveShot = true;
                explosionRadius = Mathf.Min(1.55f, explosionRadius + 0.14f);
                explosionDamage = Mathf.Min(3, explosionDamage + 1);
                break;
        }
    }

    // What: Apply a passive fire-rate multiplier while respecting the minimum cooldown.
    // Human: Chose passive fire-rate as a floor-clear reward.
    // AI: Helped clamp the multiplier to keep late-game damage from scaling too hard.
    public void ApplyFireCooldownBonus(float multiplier)
    {
        // Passive fire-rate bonuses multiply the current cooldown, including previous weapon
        // upgrades, but still respect the global minimum.
        fireCooldown = Mathf.Max(minFireCooldown, fireCooldown * Mathf.Clamp(multiplier, 0.74f, 1f));
        cooldownTimer = Mathf.Min(cooldownTimer, fireCooldown);
    }

    // What: Restore all run-scoped weapon values to their baseline.
    // Human: Required restarts to begin from the same weapon state.
    // AI: Helped list every upgraded field so no state leaks into a new run.
    void ResetForNewRun()
    {
        // Weapon upgrades are run-scoped. Starting a new run restores the baseline so repeated
        // demonstrations always begin from the same readable state.
        fireCooldown = baseFireCooldown;
        damage = baseDamage;
        extraProjectiles = 0;
        pierceCount = 0;
        hasBurnShot = false;
        hasSlowShot = false;
        hasExplosiveShot = false;
        burnDamage = baseBurnDamage;
        burnDuration = baseBurnDuration;
        slowMultiplier = baseSlowMultiplier;
        slowDuration = baseSlowDuration;
        explosionRadius = baseExplosionRadius;
        explosionDamage = baseExplosionDamage;
        cooldownTimer = 0f;
    }

    // What: Spawn either a single bullet or a symmetrical multi-shot volley.
    // Human: Designed extra projectiles as a weapon upgrade.
    // AI: Helped compute spread angles around the current aim direction.
    void SpawnBulletVolley()
    {
        int projectileCount = Mathf.Clamp(1 + extraProjectiles, 1, 5);
        if (projectileCount == 1)
        {
            SpawnBullet(aimDirection);
            return;
        }

        // Extra projectiles are spread symmetrically around the current mouse aim direction.
        float totalSpread = spreadAngle * (projectileCount - 1);
        float startAngle = -totalSpread * 0.5f;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + spreadAngle * i;
            Vector2 shotDirection = (Vector2)(Quaternion.Euler(0f, 0f, angle) * (Vector3)aimDirection);
            SpawnBullet(shotDirection.normalized);
        }
    }

    // What: Create one bullet and copy the current weapon build into it.
    // Human: Chose bullet speed, lifetime, and upgrade effects.
    // AI: Helped snapshot stats so existing bullets do not change after later upgrades.
    void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPosition = transform.position + (Vector3)(direction * muzzleOffset);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        GameObject bullet = bulletPrefab != null
            ? Instantiate(bulletPrefab, spawnPosition, rotation)
            : BuildFallbackBullet(spawnPosition, rotation);

        bullet.SetActive(true);

        Bullet projectile = bullet.GetComponent<Bullet>();
        if (projectile != null)
        {
            // Copy the active weapon build into the projectile so Bullet only handles hit logic.
            projectile.speed = bulletSpeed;
            projectile.damage = damage;
            projectile.lifetime = bulletLifetime;
            projectile.pierceRemaining = pierceCount;
            projectile.burnDamage = hasBurnShot ? burnDamage : 0;
            projectile.burnDuration = hasBurnShot ? burnDuration : 0f;
            projectile.slowMultiplier = hasSlowShot ? slowMultiplier : 1f;
            projectile.slowDuration = hasSlowShot ? slowDuration : 0f;
            projectile.explosionRadius = hasExplosiveShot ? explosionRadius : 0f;
            projectile.explosionDamage = hasExplosiveShot ? explosionDamage : 0;
        }
    }

    // What: Build a minimal bullet if the configured bullet prefab is unavailable.
    // Human: Wanted the game to remain playable without manual prefab setup.
    // AI: Helped implement a procedural fallback projectile object.
    GameObject BuildFallbackBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = new GameObject("Player Bullet");
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.transform.localScale = Vector3.one * 0.18f;

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(1f, 0.82f, 0.25f), 32);
        sr.sortingOrder = 4;

        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        bullet.AddComponent<Bullet>();
        return bullet;
    }
}
