using UnityEngine;

/// Top-down player controller for movement, pickups, exit interaction, and hit feedback.
/// Combat lives in PlayerCombat so movement and shooting can be explained separately.
///
/// Authorship note:
/// - Student-owned implementation: player movement, collision feel, pickup/exit interaction,
///   lives feedback, knockback, and final tuning.
/// - AI-assisted support: review suggestions and explanatory comments for readability.
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

    // What: Cache base movement/audio/sprite state before gameplay starts.
    // Human: Tuned the player's movement speed, radius, and hit feedback.
    // AI: Helped identify which baseline values must be restored on a new run.
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

    // What: Subscribe to run resets and initialize the player to a clean state.
    // Human: Chose that upgrades reset between runs.
    // AI: Helped route reset through GameManager.OnGameStarted.
    void Start()
    {
        if (GameManager.I != null) GameManager.I.OnGameStarted += ResetForNewRun;
        ResetForNewRun();
    }

    // What: Read movement input, move with wall checks, update effects, and handle timers.
    // Human: Chose keyboard movement and moment-to-moment control feel.
    // AI: Helped gate updates by GameManager phase and organize per-frame responsibilities.
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

    // What: Move the player while preventing clipping through runtime wall colliders.
    // Human: Tuned player collision radius and wall sliding feel.
    // AI: Helped split diagonal movement into axis retries for smoother sliding.
    float MoveWithWallCheck(Vector2 input)
    {
        if (input.sqrMagnitude <= 0f) return 0f;

        Vector3 before = transform.position;

        Vector2 current = transform.position;
        Vector2 delta = input * (moveSpeed * moveSlowMultiplier * Time.deltaTime);
        // Circle casts keep the player from clipping through thin runtime-built wall colliders.
        if (!Physics2D.CircleCast(current, radius, delta.normalized, delta.magnitude, wallMask))
        {
            transform.position = current + delta;
            return Vector3.Distance(before, transform.position);
        }

        // If diagonal movement hits a corner, try each axis separately so wall sliding feels smooth.
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

    // What: Flash the player while invulnerable after taking a hit.
    // Human: Chose the hit feedback color and temporary immunity behaviour.
    // AI: Helped make the tint return safely to the base sprite color.
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

    // What: Restore player movement and status values at the beginning of a new run.
    // Human: Chose which player upgrades/statuses are run-scoped.
    // AI: Helped make reset clear slow and invulnerability state too.
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

    // What: Permanently increase movement speed for the current run.
    // Human: Designed move speed as a passive upgrade.
    // AI: Helped cap the bonus so movement remains controllable.
    public void ApplyMoveSpeedBonus(float amount)
    {
        moveSpeed = Mathf.Min(baseMoveSpeed + 2.0f, moveSpeed + Mathf.Max(0f, amount));
    }

    // What: Place the player at the next floor's spawn point and clear floor-local statuses.
    // Human: Chose that floor transitions reset slows and hit immunity.
    // AI: Helped centralize per-floor player reset logic.
    public void ResetForNewFloor(Vector3 position)
    {
        transform.position = position;
        invulnerabilityTimer = 0f;
        footstepTimer = 0f;
        moveSlowTimer = 0f;
        moveSlowMultiplier = 1f;
        if (sr != null) sr.color = baseColor;
    }

    // What: Apply a temporary movement slow from enemy projectiles.
    // Human: Designed Frost-style slows as enemy pressure.
    // AI: Helped clamp the multiplier so slows remain readable and fair.
    public void ApplyTemporarySlow(float multiplier, float duration)
    {
        if (duration <= 0f) return;

        moveSlowMultiplier = Mathf.Clamp(multiplier, 0.35f, 1f);
        moveSlowTimer = Mathf.Max(moveSlowTimer, duration);
    }

    // What: Handle trigger collisions with keys, exits, and enemies.
    // Human: Defined the player's interaction rules.
    // AI: Helped keep trigger handling component-based instead of tag-based.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger handling is component-based instead of tag-based. That keeps setup simpler because
        // GameBootstrap creates the objects at runtime and adds the correct components directly.
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

    // What: Spend one life, start invulnerability, play feedback, and knock the player back.
    // Human: Tuned lives, hit immunity, and knockback feel.
    // AI: Helped route life loss through GameManager.PlayerHit.
    public void TakeHit(Vector3 source)
    {
        if (invulnerabilityTimer > 0f || GameManager.I.phase != GameManager.Phase.Playing) return;

        // Short immunity prevents one enemy overlap from removing all lives at once.
        invulnerabilityTimer = invulnerabilitySeconds;
        Play(hitClip, 0.7f);
        CameraShake.Pulse(0.25f, 0.2f);
        GameManager.I.PlayerHit();

        Vector3 away = (transform.position - source).normalized;
        TryApplyKnockback(away * 0.35f);
    }

    // What: Play a one-shot audio clip if one is assigned.
    // Human: Selected pickup/hit/win sound assets and volumes.
    // AI: Helped keep audio null-safe for fallback builds.
    void Play(AudioClip clip, float volume)
    {
        if (clip != null) audioSrc.PlayOneShot(clip, volume);
    }

    // What: Count down temporary slow status and restore normal speed afterward.
    // Human: Chose slow duration as a status effect.
    // AI: Helped keep slow state local to the player controller.
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

    // What: Spawn tiny visual puffs while the player is moving.
    // Human: Added movement readability feedback.
    // AI: Helped make the puffs visual-only and self-cleaning.
    void EmitFootsteps(float movedDistance)
    {
        // Footstep puffs are lightweight visual feedback only. They do not affect collision or
        // gameplay, but they make movement easier to see during the presentation.
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

    // What: Push the player away from a hit source without moving through walls.
    // Human: Tuned knockback distance.
    // AI: Helped try full and half pushes to keep knockback from clipping into walls.
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

    // What: Check if the player's collision circle overlaps a wall at a world position.
    // Human: Chose circle-based player collision.
    // AI: Helped wrap the physics check in a small helper.
    bool IsBlocked(Vector3 worldPos)
    {
        return Physics2D.OverlapCircle(worldPos, radius * 0.95f, wallMask) != null;
    }

    // What: Nudge the player out of a wall if a generated room places them inside one.
    // Human: Wanted runtime room variants to be robust during iteration.
    // AI: Helped implement the ring search for the nearest free spot.
    void TryUnstuckFromWalls()
    {
        if (!IsBlocked(transform.position)) return;

        // Runtime room variants can move the player between floors; this fallback nudges the player
        // to the nearest free spot if a layout change ever places them inside a wall.
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

/// Small self-cleaning movement puff spawned while the player walks.
///
/// Authorship note:
/// - Student-owned implementation: movement readability goal and final footstep timing/appearance.
/// - AI-assisted support: review suggestions and comments clarifying that the puff is visual-only
///   and does not participate in gameplay collision.
public class FootstepPuff : MonoBehaviour
{
    public Vector2 drift;
    public float life = 0.25f;
    float t;
    SpriteRenderer sr;

    // What: Cache the SpriteRenderer used by the footstep fade.
    // Human: Chose the footstep puff effect.
    // AI: Helped make the tiny effect component self-contained.
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // What: Drift, shrink, fade, and destroy one footstep puff.
    // Human: Tuned the puff's lifetime and motion.
    // AI: Helped keep cleanup local so Player does not track spawned puffs.
    void Update()
    {
        // Fade and shrink until the puff removes itself. The player never needs to keep references
        // to individual puffs.
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
