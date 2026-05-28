using UnityEngine;

/// Dungeon guard that patrols two points and chases the player when nearby.
public class Enemy : MonoBehaviour
{
    public Transform player;
    public Vector3 pointA;
    public Vector3 pointB;
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 3.0f;
    public float chaseRange = 3.0f;
    public float collisionRadius = 0.32f;
    public int maxHealth = 3;
    public int currentHealth;
    public LayerMask wallMask;

    Vector3 target;
    SpriteRenderer sr;
    Color baseColor;
    float hitFlashTimer;

    void Start()
    {
        target = pointB;
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        bool chasing = ShouldChase();
        Vector3 destination = chasing ? player.position : target;
        float speed = chasing ? chaseSpeed : patrolSpeed;
        MoveWithWallCheck(destination, speed);

        if (!chasing && Vector3.Distance(transform.position, target) < 0.05f)
            target = target == pointA ? pointB : pointA;

        UpdateHitFlash();
    }

    bool ShouldChase()
    {
        if (player == null) return false;

        Vector2 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > chaseRange) return false;
        return !Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, wallMask);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Player p = other.GetComponent<Player>();
        if (p != null) p.TakeHit(transform.position);
    }

    public void TakeDamage(int damage, Vector3 hitSource)
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, damage));
        hitFlashTimer = 0.12f;

        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void UpdateHitFlash()
    {
        if (sr == null || hitFlashTimer <= 0f) return;

        hitFlashTimer -= Time.deltaTime;
        sr.color = hitFlashTimer > 0f ? Color.white : baseColor;
    }

    void MoveWithWallCheck(Vector3 destination, float speed)
    {
        Vector2 current = transform.position;
        Vector2 toDestination = (Vector2)destination - current;
        float stepDistance = speed * Time.deltaTime;
        if (toDestination.sqrMagnitude <= 0.0001f) return;

        Vector2 delta = Vector2.ClampMagnitude(toDestination, stepDistance);
        if (!Physics2D.CircleCast(current, collisionRadius, delta.normalized, delta.magnitude, wallMask))
        {
            transform.position = current + delta;
            return;
        }

        // Slide on one axis when diagonal movement clips a wall.
        Vector2 xDelta = new Vector2(delta.x, 0f);
        if (Mathf.Abs(xDelta.x) > 0.001f &&
            !Physics2D.CircleCast(current, collisionRadius, xDelta.normalized, Mathf.Abs(xDelta.x), wallMask))
            transform.position += (Vector3)xDelta;

        Vector2 yDelta = new Vector2(0f, delta.y);
        if (Mathf.Abs(yDelta.y) > 0.001f &&
            !Physics2D.CircleCast(transform.position, collisionRadius, yDelta.normalized, Mathf.Abs(yDelta.y), wallMask))
            transform.position += (Vector3)yDelta;
    }
}
