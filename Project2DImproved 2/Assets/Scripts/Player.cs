using UnityEngine;

/// Top-down player movement with manual wall blocking and short hit immunity.
public class Player : MonoBehaviour
{
    public float moveSpeed = 4.8f;
    public float radius = 0.32f;
    public float invulnerabilitySeconds = 1.1f;
    public LayerMask wallMask;
    public AudioClip keyClip;
    public AudioClip hitClip;
    public AudioClip winClip;
    public float footstepSpawnInterval = 0.08f;
    public float footstepSpeedThreshold = 0.2f;

    Vector3 startPosition;
    float invulnerabilityTimer;
    float footstepTimer;
    float baseMoveSpeed;
    float moveSlowTimer;
    float moveSlowMultiplier = 1f;
    AudioSource audioSrc;
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        startPosition = transform.position;
        baseMoveSpeed = moveSpeed;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Start()
    {
        if (GameManager.I != null) GameManager.I.OnGameStarted += ResetForNewRun;
        ResetForNewRun();
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        TryUnstuckFromWalls();

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        float movedDistance = MoveWithWallCheck(input);
        EmitFootsteps(movedDistance);

        if (invulnerabilityTimer > 0f) invulnerabilityTimer -= Time.deltaTime;
        UpdateMoveSlow();
        UpdateTint();
    }

    float MoveWithWallCheck(Vector2 input)
    {
        if (input.sqrMagnitude <= 0f) return 0f;

        Vector3 before = transform.position;

        Vector2 current = transform.position;
        Vector2 delta = input * (moveSpeed * moveSlowMultiplier * Time.deltaTime);
        if (!Physics2D.CircleCast(current, radius, delta.normalized, delta.magnitude, wallMask))
        {
            transform.position = current + delta;
            return Vector3.Distance(before, transform.position);
        }

        Vector2 xDelta = new Vector2(delta.x, 0f);
        if (Mathf.Abs(xDelta.x) > 0.001f &&
            !Physics2D.CircleCast(current, radius, xDelta.normalized, Mathf.Abs(xDelta.x), wallMask))
            transform.position += (Vector3)xDelta;

        Vector2 yDelta = new Vector2(0f, delta.y);
        if (Mathf.Abs(yDelta.y) > 0.001f &&
            !Physics2D.CircleCast(transform.position, radius, yDelta.normalized, Mathf.Abs(yDelta.y), wallMask))
            transform.position += (Vector3)yDelta;

        return Vector3.Distance(before, transform.position);
    }

    void UpdateTint()
    {
        if (sr == null) return;

        if (invulnerabilityTimer > 0f)
        {
            sr.color = Mathf.PingPong(Time.unscaledTime * 10f, 1f) > 0.5f
                ? new Color(1f, 0.55f, 0.55f, 0.75f)
                : baseColor;
        }
        else
        {
            sr.color = baseColor;
        }
    }

    void ResetForNewRun()
    {
        transform.position = startPosition;
        moveSpeed = baseMoveSpeed;
        invulnerabilityTimer = 0f;
        footstepTimer = 0f;
        moveSlowTimer = 0f;
        moveSlowMultiplier = 1f;
        if (sr != null) sr.color = baseColor;
    }

    public void ApplyMoveSpeedBonus(float amount)
    {
        moveSpeed = Mathf.Min(baseMoveSpeed + 2.0f, moveSpeed + Mathf.Max(0f, amount));
    }

    public void ResetForNewFloor(Vector3 position)
    {
        transform.position = position;
        invulnerabilityTimer = 0f;
        footstepTimer = 0f;
        moveSlowTimer = 0f;
        moveSlowMultiplier = 1f;
        if (sr != null) sr.color = baseColor;
    }

    public void ApplyTemporarySlow(float multiplier, float duration)
    {
        if (duration <= 0f) return;

        moveSlowMultiplier = Mathf.Clamp(multiplier, 0.35f, 1f);
        moveSlowTimer = Mathf.Max(moveSlowTimer, duration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        KeyPickup key = other.GetComponent<KeyPickup>();
        if (key != null)
        {
            key.Collect();
            Play(keyClip, 0.8f);
            return;
        }

        ExitDoor exit = other.GetComponent<ExitDoor>();
        if (exit != null)
        {
            GameManager.I.TryExit();
            if (GameManager.I.phase == GameManager.Phase.Won) Play(winClip, 0.8f);
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null) TakeHit(enemy.transform.position);
    }

    public void TakeHit(Vector3 source)
    {
        if (invulnerabilityTimer > 0f || GameManager.I.phase != GameManager.Phase.Playing) return;

        invulnerabilityTimer = invulnerabilitySeconds;
        Play(hitClip, 0.7f);
        CameraShake.Pulse(0.25f, 0.2f);
        GameManager.I.PlayerHit();

        Vector3 away = (transform.position - source).normalized;
        TryApplyKnockback(away * 0.35f);
    }

    void Play(AudioClip clip, float volume)
    {
        if (clip != null) audioSrc.PlayOneShot(clip, volume);
    }

    void UpdateMoveSlow()
    {
        if (moveSlowTimer <= 0f) return;

        moveSlowTimer -= Time.deltaTime;
        if (moveSlowTimer <= 0f)
        {
            moveSlowTimer = 0f;
            moveSlowMultiplier = 1f;
        }
    }

    void EmitFootsteps(float movedDistance)
    {
        if (movedDistance <= footstepSpeedThreshold * Time.deltaTime) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f) return;

        footstepTimer = footstepSpawnInterval;
        GameObject puff = new GameObject("FootstepPuff");
        puff.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 0.12f);
        SpriteRenderer puffSr = puff.AddComponent<SpriteRenderer>();
        puffSr.sprite = Art2D.SolidCircle(new Color(1f, 1f, 1f, 0.8f), 16);
        puffSr.sortingOrder = 1;
        puff.transform.localScale = Vector3.one * Random.Range(0.12f, 0.2f);
        FootstepPuff effect = puff.AddComponent<FootstepPuff>();
        effect.drift = Random.insideUnitCircle * 0.25f;
    }

    void TryApplyKnockback(Vector3 delta)
    {
        Vector3 original = transform.position;
        Vector3 full = original + delta;
        if (!IsBlocked(full))
        {
            transform.position = full;
            return;
        }

        Vector3 half = original + delta * 0.5f;
        if (!IsBlocked(half))
        {
            transform.position = half;
            return;
        }

        // If both push attempts clip into a wall, keep current position.
    }

    bool IsBlocked(Vector3 worldPos)
    {
        return Physics2D.OverlapCircle(worldPos, radius * 0.95f, wallMask) != null;
    }

    void TryUnstuckFromWalls()
    {
        if (!IsBlocked(transform.position)) return;

        Vector3 origin = transform.position;
        for (int ring = 1; ring <= 6; ring++)
        {
            float distance = ring * 0.08f;
            for (int i = 0; i < 16; i++)
            {
                float angle = (Mathf.PI * 2f / 16f) * i;
                Vector3 candidate = origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
                if (!IsBlocked(candidate))
                {
                    transform.position = candidate;
                    return;
                }
            }
        }
    }
}

public class FootstepPuff : MonoBehaviour
{
    public Vector2 drift;
    public float life = 0.25f;
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
        transform.localScale *= 0.98f;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(1f - (t / life));
            sr.color = c;
        }

        if (t >= life) Destroy(gameObject);
    }
}
