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
    public float alertSecondsAfterHit = 4.0f;
    public float knockbackDistance = 0.22f;
    public int maxHealth = 3;
    public int currentHealth;
    public int xpReward = 10;
    public LayerMask wallMask;

    Vector3 startPosition;
    Vector3 target;
    SpriteRenderer sr;
    Color baseColor;
    float hitFlashTimer;
    float alertTimer;
    float burnTimer;
    float burnTickTimer;
    int burnDamagePerTick;
    float slowTimer;
    float slowMultiplier = 1f;
    bool dead;

    void Awake()
    {
        startPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Start()
    {
        if (GameManager.I != null) GameManager.I.OnGameStarted += ResetForNewRun;
        ResetForNewRun();
    }

    void OnDestroy()
    {
        if (GameManager.I != null) GameManager.I.OnGameStarted -= ResetForNewRun;
    }

    void ResetForNewRun()
    {
        dead = false;
        alertTimer = 0f;
        hitFlashTimer = 0f;
        burnTimer = 0f;
        burnTickTimer = 0f;
        burnDamagePerTick = 0;
        slowTimer = 0f;
        slowMultiplier = 1f;
        target = pointB;
        currentHealth = maxHealth;
        transform.position = startPosition;
        gameObject.SetActive(true);
        if (sr != null) sr.color = baseColor;
    }

    void Update()
    {
        if (dead || GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        if (alertTimer > 0f) alertTimer -= Time.deltaTime;
        UpdateStatusEffects();
        bool chasing = ShouldChase();
        Vector3 destination = chasing ? player.position : target;
        float speed = (chasing ? chaseSpeed : patrolSpeed) * slowMultiplier;
        MoveWithWallCheck(destination, speed);

        if (!chasing && Vector3.Distance(transform.position, target) < 0.05f)
            target = target == pointA ? pointB : pointA;

        UpdateVisualState();
    }

    bool ShouldChase()
    {
        if (player == null) return false;
        if (alertTimer > 0f) return true;

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
        if (dead || GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        DealDamage(Mathf.Max(1, damage));
        alertTimer = alertSecondsAfterHit;
        hitFlashTimer = 0.12f;
        TryApplyKnockback((transform.position - hitSource).normalized * knockbackDistance);
    }

    public void ApplyBurn(int damagePerTick, float duration)
    {
        if (dead || duration <= 0f || damagePerTick <= 0) return;

        burnDamagePerTick = Mathf.Max(burnDamagePerTick, damagePerTick);
        burnTimer = Mathf.Max(burnTimer, duration);
        burnTickTimer = 0.2f;
        alertTimer = Mathf.Max(alertTimer, alertSecondsAfterHit);
        UpdateVisualState();
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (dead || duration <= 0f) return;

        slowMultiplier = Mathf.Clamp(multiplier, 0.25f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);
        alertTimer = Mathf.Max(alertTimer, alertSecondsAfterHit);
        UpdateVisualState();
    }

    void DealDamage(int amount)
    {
        if (dead) return;

        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, amount));

        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        if (dead) return;

        dead = true;
        if (GameManager.I != null) GameManager.I.RegisterEnemyDefeated(xpReward);
        gameObject.SetActive(false);
    }

    void UpdateStatusEffects()
    {
        if (burnTimer > 0f)
        {
            burnTimer -= Time.deltaTime;
            burnTickTimer -= Time.deltaTime;

            if (burnTickTimer <= 0f)
            {
                burnTickTimer = 0.5f;
                DealDamage(burnDamagePerTick);
            }

            if (burnTimer <= 0f)
            {
                burnTimer = 0f;
                burnDamagePerTick = 0;
            }
        }

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowTimer = 0f;
                slowMultiplier = 1f;
            }
        }
    }

    void UpdateVisualState()
    {
        if (sr == null) return;

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            sr.color = Color.white;
            return;
        }

        if (burnTimer > 0f)
        {
            sr.color = Color.Lerp(baseColor, new Color(1f, 0.22f, 0.08f), 0.72f);
            return;
        }

        if (slowTimer > 0f)
        {
            sr.color = Color.Lerp(baseColor, new Color(0.32f, 0.72f, 1f), 0.65f);
            return;
        }

        sr.color = baseColor;
    }

    void TryApplyKnockback(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0.0001f) return;

        Vector3 destination = transform.position + delta;
        if (!Physics2D.CircleCast(transform.position, collisionRadius, delta.normalized, delta.magnitude, wallMask))
            transform.position = destination;
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
