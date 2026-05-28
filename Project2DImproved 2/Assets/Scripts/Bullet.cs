using UnityEngine;

/// Combat projectile that moves forward, damages enemies, and stops at walls.
public class Bullet : MonoBehaviour
{
    public float speed = 14f;
    public int damage = 1;
    public float lifetime = 1.5f;
    public float radius = 0.08f;
    public LayerMask wallMask;

    float born;

    void Start() { born = Time.time; }

    void Update()
    {
        if (Time.time - born >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 current = transform.position;
        Vector2 direction = transform.up;
        float distance = speed * Time.deltaTime;

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
            enemy.TakeDamage(damage, transform.position);
            HitAndDestroy(enemyHit.point);
            return;
        }

        transform.position = current + direction * distance;
    }

    RaycastHit2D FindEnemyHit(Vector2 current, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(current, radius, direction, distance);
        RaycastHit2D bestHit = default;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].collider != null ? hits[i].collider.GetComponent<Enemy>() : null;
            if (enemy == null || hits[i].distance >= bestDistance) continue;

            bestHit = hits[i];
            bestDistance = hits[i].distance;
        }

        return bestHit;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform.position);
            HitAndDestroy(transform.position);
            return;
        }

        if (wallMask.value != 0 && ((1 << other.gameObject.layer) & wallMask.value) != 0)
            HitAndDestroy(transform.position);
    }

    void HitAndDestroy(Vector3 position)
    {
        SpawnImpact(position);
        Destroy(gameObject);
    }

    void SpawnImpact(Vector3 position)
    {
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

public class BulletImpact : MonoBehaviour
{
    public Vector2 drift;
    public float life = 0.16f;

    float t;
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
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
