using UnityEngine;

/// Enemy type identifiers used by the spawner, fallback art, and ability controller.
///
/// Authorship note:
/// - Student-owned design: enemy lineup, unlock order, and the decision to include boss types that
///   later return as elite enemies.
/// - AI-assisted support: naming/organization review so the enum can be shared cleanly by
///   GameBootstrap, EnemyAbilityController, and Art2D fallback art.
public enum EnemyKind
{
    SlimeScout,
    TinyBat,
    ShieldGuard,
    SparkSpitter,
    BombSprite,
    FrostWisp,
    DashImp,
    HealerFairy,
    SummonerShade,
    CrystalBrute,
    SlimeKing,
    FrostQueen,
    ShadeOverlord,
    CrystalTitan
}

/// Base enemy behaviour: patrol between two points, chase with line of sight, take damage,
/// and apply shared status effects. EnemyAbilityController adds type-specific attacks.
///
/// Authorship note:
/// - Student-completed code: enemy movement, chase rules, health/XP behaviour, and final
///   balance in the playable run, including the decision to make bosses reappear later as weaker
///   elite enemies for presentation pacing.
/// - AI-assisted support: code review suggestions, organization guidance for boss ability variants,
///   reuse of boss attacks in elite mode, and explanatory comments for how the ability system works.
///   The student completed the gameplay decisions and reviewed the final submitted code.
public class Enemy : MonoBehaviour
{
    public EnemyKind kind = EnemyKind.SlimeScout;
    public string displayName = "Slime Scout";
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
    public AudioClip defeatClip;

    // Set only for real floor boss encounters. Boss-kind elite enemies keep this false so they use
    // reduced attack counts/timers and do not trigger boss UI or exit-lock behaviour.
    public bool isBoss;

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

    public bool IsDead { get { return dead; } }
    public float HealthFraction { get { return maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth; } }

    // What: Cache this enemy's renderer and base color before gameplay starts.
    // Human: Chose enemy tint/flash feedback.
    // AI: Helped identify renderer state needed for visual reset.
    void Awake()
    {
        startPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    // What: Subscribe to run start and initialize health/status values.
    // Human: Chose enemies reset cleanly when a run restarts.
    // AI: Helped connect enemies to GameManager.OnGameStarted.
    // What: Initialize ability timers with offsets so enemies do not all attack at once.
    // Human: Tuned ability opening delays and boss timer defaults.
    // AI: Helped separate ability timing from base enemy movement.
    void Start()
    {
        ResetForNewRun();
    }

    // What: Reset health, status effects, death state, and renderer color for a new run.
    // Human: Tuned enemy baseline values.
    // AI: Helped ensure burn/slow/flash state does not leak across restarts.
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

    // What: Update status effects, choose patrol/chase destination, move, and repaint visuals.
    // Human: Designed patrol plus chase enemy behaviour.
    // AI: Helped keep special attacks in EnemyAbilityController so base movement stays readable.
    // What: Dispatch this enemy's special behaviour based on its EnemyKind.
    // Human: Designed each enemy type's combat identity.
    // AI: Helped organize the switch so each ability stays in its own method.
    void Update()
    {
        // The base enemy loop is deliberately small: update status effects, decide whether to chase,
        // move toward the chosen destination, then update visuals. Special attacks are handled below
        // in EnemyAbilityController so the patrol/chase logic stays readable.
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

    // What: Decide whether the enemy should chase the player right now.
    // Human: Chose line-of-sight and alert-after-hit rules.
    // AI: Helped make walls block sight for route-planning gameplay.
    bool ShouldChase()
    {
        if (player == null) return false;
        if (alertTimer > 0f) return true;

        // Walls block sight, so enemies can be avoided through route planning instead of only speed.
        Vector2 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > chaseRange) return false;
        return !Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, wallMask);
    }

    // What: Damage the player when this enemy overlaps the player trigger.
    // Human: Chose contact damage as the base enemy threat.
    // AI: Helped keep actual life loss inside Player.TakeHit/GameManager.
    void OnTriggerEnter2D(Collider2D other)
    {
        Player p = other.GetComponent<Player>();
        if (p != null) p.TakeHit(transform.position);
    }

    // What: Apply incoming damage, flash, knock back, and alert this enemy.
    // Human: Tuned hit reaction and damage values.
    // AI: Helped route all damage through DealDamage for consistent death handling.
    public void TakeDamage(int damage, Vector3 hitSource)
    {
        if (dead || GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        DealDamage(Mathf.Max(1, damage));
        alertTimer = alertSecondsAfterHit;
        hitFlashTimer = 0.12f;
        TryApplyKnockback((transform.position - hitSource).normalized * knockbackDistance);
    }

    // What: Start or refresh burn damage-over-time on this enemy.
    // Human: Designed Burn Shot as a weapon upgrade.
    // AI: Helped store burn on the target so bullets can be simple stat snapshots.
    public void ApplyBurn(int damagePerTick, float duration)
    {
        if (dead || duration <= 0f || damagePerTick <= 0) return;

        burnDamagePerTick = Mathf.Max(burnDamagePerTick, damagePerTick);
        burnTimer = Mathf.Max(burnTimer, duration);
        burnTickTimer = 0.2f;
        alertTimer = Mathf.Max(alertTimer, alertSecondsAfterHit);
        UpdateVisualState();
    }

    // What: Start or refresh a temporary movement slow on this enemy.
    // Human: Designed Slow Shot as a crowd-control upgrade.
    // AI: Helped clamp slow values so enemies remain readable and fair.
    public void ApplySlow(float multiplier, float duration)
    {
        if (dead || duration <= 0f) return;

        slowMultiplier = Mathf.Clamp(multiplier, 0.25f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);
        alertTimer = Mathf.Max(alertTimer, alertSecondsAfterHit);
        UpdateVisualState();
    }

    // What: Restore health without exceeding maxHealth.
    // Human: Designed healer enemies as support threats.
    // AI: Helped make healing no-op for dead or nonpositive cases.
    public void Heal(int amount)
    {
        if (dead || amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        hitFlashTimer = 0.08f;
        UpdateVisualState();
    }

    // What: Force this enemy to stay alert/chasing for a short time.
    // Human: Chose that damaged enemies should pressure the player.
    // AI: Helped reuse this hook from attacks and hit reactions.
    public void ForceAlert(float duration)
    {
        if (dead) return;
        alertTimer = Mathf.Max(alertTimer, duration);
    }

    // What: Check distance and wall line-of-sight from this enemy to the player.
    // Human: Chose walls as stealth/route-planning blockers.
    // AI: Helped centralize the sight check for movement and abilities.
    public bool HasLineOfSightToPlayer(float range)
    {
        if (player == null) return false;

        Vector2 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > range) return false;
        return !Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, wallMask);
    }

    // What: Move this enemy by an ability-driven delta if the path is not blocked.
    // Human: Designed dash/reposition abilities for advanced enemies.
    // AI: Helped keep ability movement wall-safe.
    public void TryMoveAbilityDelta(Vector3 delta)
    {
        TryApplyKnockback(delta);
    }

    // What: Subtract health and trigger death when health reaches zero.
    // Human: Tuned enemy health and death pacing.
    // AI: Helped keep all damage sources using one health path.
    void DealDamage(int amount)
    {
        if (dead) return;

        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, amount));

        if (currentHealth <= 0) Die();
    }

    // What: Mark the enemy dead, play rewards/audio, and deactivate the object.
    // Human: Chose XP reward and boss-defeat progression behaviour.
    // AI: Helped order boss defeat and enemy defeat notifications safely.
    public void Die()
    {
        if (dead) return;

        // Defeated enemies are deactivated rather than destroyed immediately. That avoids accidental
        // duplicate XP events and keeps the death path simple for this vertical slice.
        dead = true;
        if (defeatClip != null) AudioSource.PlayClipAtPoint(defeatClip, transform.position, 0.55f);
        if (isBoss && GameManager.I != null) GameManager.I.RegisterBossDefeated();
        if (GameManager.I != null) GameManager.I.RegisterEnemyDefeated(xpReward);
        gameObject.SetActive(false);
    }

    // What: Tick burn/slow timers and apply periodic burn damage.
    // Human: Designed status effect durations and tick behaviour.
    // AI: Helped keep status logic on the enemy target instead of each bullet.
    void UpdateStatusEffects()
    {
        // Burn and slow are owned by the target so multiple bullet types can apply them consistently.
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

    // What: Update sprite color for hit flash, burn, slow, and normal state.
    // Human: Chose visual feedback colors for enemy status.
    // AI: Helped prioritize temporary flash over persistent status tint.
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

    // What: Push this enemy away from a hit without clipping through walls.
    // Human: Tuned enemy knockback distance.
    // AI: Helped use the same full/half movement idea as player knockback.
    void TryApplyKnockback(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0.0001f) return;

        Vector3 destination = transform.position + delta;
        if (!Physics2D.CircleCast(transform.position, collisionRadius, delta.normalized, delta.magnitude, wallMask))
            transform.position = destination;
    }

    // What: Move toward a destination with wall collision and axis-slide fallback.
    // Human: Chose patrol/chase movement feel.
    // AI: Helped implement wall-safe movement for generated rooms.
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

/// Optional enemy ability layer used by later-floor enemy types.
/// Keeping this separate from Enemy makes the simple patrol/chase behaviour easier to read.
///
/// Authorship note:
/// - Student-completed code: enemy type selection, ability goals, and final gameplay tuning.
/// - AI-assisted support: organization review and comments that describe each behaviour pattern.
public class EnemyAbilityController : MonoBehaviour
{
    public Enemy owner;
    public EnemyKind kind;
    public Transform player;
    public LayerMask wallMask;
    public Sprite projectileSprite;
    public Sprite slimeSprite;
    public Transform spawnRoot;
    // Filled by GameBootstrap from the current floor. Normal early enemies use 1.0; later floors
    // increase cooldown pressure, projectile speed, range, and select summon caps.
    public float difficultyScale = 1f;

    float abilityTimer;
    float bossProjectileTimer;
    float bossDashTimer;
    float bossSummonTimer;
    float bossPulseTimer;
    float dashWindup;
    float dashTimer;
    Vector2 dashDirection;
    int summonsMade;
    bool exploded;

    // What: Initialize ability timers with offsets so enemies do not all attack at once.
    // Human: Tuned ability opening delays and boss timer defaults.
    // AI: Helped separate ability timing from base enemy movement.
    void Start()
    {
        owner = owner != null ? owner : GetComponent<Enemy>();
        abilityTimer = Random.Range(0.25f, 1.2f);
        bossProjectileTimer = 0.85f;
        bossDashTimer = 2.4f;
        bossSummonTimer = 4.3f;
        bossPulseTimer = 3.1f;
    }

    // What: Dispatch this enemy's special behaviour based on its EnemyKind.
    // Human: Designed each enemy type's combat identity.
    // AI: Helped organize the switch so each ability stays in its own method.
    void Update()
    {
        if (owner == null || owner.IsDead || GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;
        if (player == null) return;

        // Each kind has one small ability pattern so enemies feel different without a full AI system.
        switch (kind)
        {
            case EnemyKind.SparkSpitter:
                UpdateProjectileAttack(1.55f, 5.2f, 5.8f, 1, 0f, 1f, 0f, new Color(1f, 0.82f, 0.25f, 1f));
                break;
            case EnemyKind.BombSprite:
                UpdateBombSprite();
                break;
            case EnemyKind.FrostWisp:
                UpdateProjectileAttack(2.1f, 4.8f, 4.7f, 0, 2.4f, 0.45f, 1.6f, new Color(0.42f, 0.88f, 1f, 1f));
                break;
            case EnemyKind.DashImp:
                UpdateDashImp();
                break;
            case EnemyKind.HealerFairy:
                UpdateHealerFairy();
                break;
            case EnemyKind.SummonerShade:
                UpdateSummonerShade();
                break;
            case EnemyKind.CrystalBrute:
                UpdateCrystalBrute();
                break;
            case EnemyKind.SlimeKing:
                UpdateSlimeKing();
                break;
            case EnemyKind.FrostQueen:
                UpdateFrostQueen();
                break;
            case EnemyKind.ShadeOverlord:
                UpdateShadeOverlord();
                break;
            case EnemyKind.CrystalTitan:
                UpdateCrystalTitan();
                break;
        }
    }

    // What: Fire one projectile at the player after cooldown and line-of-sight checks.
    // Human: Designed ranged pressure for Spark/Frost/boss enemies.
    // AI: Helped parameterize speed, damage, slow, lifetime, and color for reuse.
    void UpdateProjectileAttack(float interval, float range, float speed, int damage, float slowDuration, float slowMultiplier, float lifetime, Color color)
    {
        // Spark Spitter and Frost Wisp share the same ranged attack path. The passed-in parameters
        // decide whether the projectile deals damage, applies slow, or only creates pressure.
        abilityTimer -= Time.deltaTime;
        if (abilityTimer > 0f || !owner.HasLineOfSightToPlayer(ScaledRange(range))) return;

        abilityTimer = ScaledInterval(interval, 0.85f) + Random.Range(-0.18f, 0.28f);
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        SpawnEnemyProjectile(direction, ScaledSpeed(speed), damage, slowDuration, slowMultiplier, lifetime > 0f ? lifetime : 1.4f, color);
        owner.ForceAlert(2.4f);
    }

    // What: Create and configure one enemy projectile object.
    // Human: Chose enemy projectile readability and collision behaviour.
    // AI: Helped build the projectile fully in code for the runtime-generated scene.
    void SpawnEnemyProjectile(Vector2 direction, float speed, int damage, float slowDuration, float slowMultiplier, float lifetime, Color color)
    {
        GameObject projectile = new GameObject(kind + " Projectile");
        projectile.transform.SetParent(spawnRoot != null ? spawnRoot : transform.parent);
        projectile.transform.position = transform.position + (Vector3)(direction * 0.35f);
        projectile.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        projectile.transform.localScale = new Vector3(0.44f, 0.32f, 1f);

        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = projectileSprite != null ? projectileSprite : Art2D.Projectile(64, 24);
        sr.color = color;
        sr.sortingOrder = 4;

        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.28f;

        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
        enemyProjectile.speed = speed;
        enemyProjectile.damage = damage;
        enemyProjectile.slowDuration = slowDuration;
        enemyProjectile.slowMultiplier = slowMultiplier;
        enemyProjectile.lifetime = lifetime;
        enemyProjectile.wallMask = wallMask;
    }

    // What: Make Bomb Sprite explode when the player gets close.
    // Human: Designed proximity explosion as a risk/reward enemy.
    // AI: Helped scale trigger radius and effect size with difficulty.
    void UpdateBombSprite()
    {
        if (exploded) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > Mathf.Min(1.05f, 0.8f * difficultyScale)) return;

        exploded = true;
        Player target = player.GetComponent<Player>();
        if (target != null && distance <= Mathf.Min(1.55f, 1.25f * difficultyScale)) target.TakeHit(transform.position);
        SpawnPulse(new Color(1f, 0.55f, 0.15f, 0.55f), Mathf.Min(2.8f, 2.3f * difficultyScale), 0.32f);
        owner.Die();
    }

    // What: Handle Dash Imp warning, dash movement, and cooldown.
    // Human: Designed a readable windup before fast movement.
    // AI: Helped split dash into windup and active dash timers.
    void UpdateDashImp()
    {
        // Dash has a short windup pulse first, giving the player a readable warning.
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            owner.TryMoveAbilityDelta((Vector3)(dashDirection * (ScaledSpeed(7.4f) * Time.deltaTime)));
            return;
        }

        if (dashWindup > 0f)
        {
            dashWindup -= Time.deltaTime;
            if (dashWindup <= 0f)
            {
                dashTimer = 0.28f;
                dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
            }
            return;
        }

        abilityTimer -= Time.deltaTime;
        if (abilityTimer > 0f || !owner.HasLineOfSightToPlayer(ScaledRange(4.6f))) return;

        abilityTimer = ScaledInterval(2.4f, 1.25f);
        dashWindup = ScaledInterval(0.42f, 0.28f);
        SpawnPulse(new Color(1f, 0.92f, 0.25f, 0.26f), 1.4f, 0.22f);
        owner.ForceAlert(2.2f);
    }

    // What: Heal nearby damaged allies on a cooldown.
    // Human: Designed Healer Fairy as a support enemy.
    // AI: Helped avoid self-healing so fights do not stall forever.
    void UpdateHealerFairy()
    {
        // The healer supports nearby enemies but does not heal itself. This keeps the support role
        // visible without making one enemy stall the run forever.
        abilityTimer -= Time.deltaTime;
        if (abilityTimer > 0f) return;

        abilityTimer = ScaledInterval(3.4f, 1.9f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ScaledRange(2.35f));
        bool healed = false;
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy ally = hits[i].GetComponent<Enemy>();
            if (ally == null || ally == owner || ally.IsDead || ally.HealthFraction >= 0.98f) continue;

            ally.Heal(1);
            healed = true;
        }

        if (healed) SpawnPulse(new Color(0.5f, 1f, 0.68f, 0.3f), 2.6f, 0.35f);
    }

    // What: Summon small slime allies when the player is in sight.
    // Human: Designed Summoner Shade as a pressure multiplier.
    // AI: Helped cap summons so enemy growth stays controlled.
    void UpdateSummonerShade()
    {
        // Summoning is capped per enemy so late floors add pressure without creating unlimited
        // enemy growth or performance problems.
        int summonCap = difficultyScale >= 1.25f ? 4 : 3;
        if (summonsMade >= summonCap) return;

        abilityTimer -= Time.deltaTime;
        if (abilityTimer > 0f || !owner.HasLineOfSightToPlayer(ScaledRange(5.0f))) return;

        abilityTimer = ScaledInterval(4.8f, 2.8f);
        if (TrySummonSlime()) summonsMade++;
    }

    // What: Try to place one summoned slime near the summoner without spawning inside walls.
    // Human: Chose summoned slime stats and XP reward.
    // AI: Helped implement repeated safe-position attempts.
    bool TrySummonSlime()
    {
        // Try several nearby positions so summoned enemies do not appear inside walls.
        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0.7f, 1.25f);
            Vector2 candidate = (Vector2)transform.position + offset;
            if (Physics2D.OverlapCircle(candidate, 0.34f, wallMask) != null) continue;

            GameObject slime = new GameObject("Summoned Slime Scout");
            slime.transform.SetParent(spawnRoot != null ? spawnRoot : transform.parent);
            slime.transform.position = candidate;
            slime.transform.localScale = Vector3.one * 0.78f;

            SpriteRenderer sr = slime.AddComponent<SpriteRenderer>();
            sr.sprite = slimeSprite != null ? slimeSprite : Art2D.EnemySprite(EnemyKind.SlimeScout);
            sr.color = new Color(0.72f, 1f, 0.62f, 1f);
            sr.sortingOrder = 2;

            CircleCollider2D col = slime.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.44f;

            Rigidbody2D rb = slime.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            Enemy enemy = slime.AddComponent<Enemy>();
            enemy.kind = EnemyKind.SlimeScout;
            enemy.displayName = "Summoned Slime Scout";
            enemy.player = player;
            enemy.pointA = candidate;
            enemy.pointB = candidate + Random.insideUnitCircle.normalized * 0.9f;
            enemy.wallMask = wallMask;
            enemy.maxHealth = 1;
            enemy.currentHealth = 1;
            enemy.patrolSpeed = ScaledSpeed(1.7f);
            enemy.chaseSpeed = ScaledSpeed(2.45f);
            enemy.chaseRange = ScaledRange(2.8f);
            enemy.xpReward = 2;
            enemy.collisionRadius = 0.3f;

            SpawnPulse(new Color(0.55f, 0.45f, 0.95f, 0.32f), 1.7f, 0.28f);
            return true;
        }

        return false;
    }

    // What: Give Crystal Brute a short forward pressure step.
    // Human: Designed Crystal Brute as a heavy elite threat.
    // AI: Helped scale the lunge with late-floor difficulty.
    void UpdateCrystalBrute()
    {
        abilityTimer -= Time.deltaTime;
        if (abilityTimer > 0f || !owner.HasLineOfSightToPlayer(ScaledRange(4.2f))) return;

        abilityTimer = ScaledInterval(3.2f, 1.8f);
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        owner.TryMoveAbilityDelta((Vector3)(direction * Mathf.Min(1.05f, 0.75f * difficultyScale)));
        SpawnPulse(new Color(0.7f, 0.92f, 1f, 0.26f), 2.0f, 0.24f);
        owner.ForceAlert(2.5f);
    }

    // What: Run Slime King's summon and close-range slam behaviour.
    // Human: Designed Slime King as the first boss and later elite.
    // AI: Helped branch boss mode versus elite mode using owner.isBoss.
    void UpdateSlimeKing()
    {
        // Slime King is the summoner boss. In full boss mode it can spawn several slimes and uses
        // a larger close-range slam; in elite mode it keeps the same identity but fewer summons and
        // a slower pulse timer make it manageable as a normal-room threat.
        bossSummonTimer -= Time.deltaTime;
        bossPulseTimer -= Time.deltaTime;

        if (bossSummonTimer <= 0f && summonsMade < (owner.isBoss ? 8 : 3) && owner.HasLineOfSightToPlayer(5.5f))
        {
            bossSummonTimer = owner.isBoss ? 4.8f : 6.4f;
            int count = owner.isBoss ? 3 : 1;
            for (int i = 0; i < count; i++)
                if (TrySummonSlime()) summonsMade++;
            owner.ForceAlert(3f);
        }

        if (bossPulseTimer <= 0f && owner.HealthFraction < 0.75f)
        {
            bossPulseTimer = owner.isBoss ? 2.8f : 4.2f;
            float range = owner.isBoss ? 1.75f : 1.35f;
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= range)
            {
                Player target = player.GetComponent<Player>();
                if (target != null) target.TakeHit(transform.position);
            }
            SpawnPulse(new Color(0.5f, 1f, 0.36f, 0.3f), owner.isBoss ? 3.2f : 2.2f, 0.35f);
            owner.ForceAlert(2.5f);
        }
    }

    // What: Run Frost Queen's slowing projectiles and reposition dash.
    // Human: Designed Frost Queen as a space-control boss.
    // AI: Helped tune boss volleys separately from later elite behaviour.
    void UpdateFrostQueen()
    {
        // Frost Queen controls space with slowing projectiles. The real boss fires a wider volley,
        // while the elite version keeps only the central projectile and a weaker reposition burst.
        bossProjectileTimer -= Time.deltaTime;
        bossDashTimer -= Time.deltaTime;

        if (bossProjectileTimer <= 0f && owner.HasLineOfSightToPlayer(6.4f))
        {
            bossProjectileTimer = owner.isBoss ? 1.25f : 1.9f;
            Vector2 aim = ((Vector2)player.position - (Vector2)transform.position).normalized;
            SpawnEnemyProjectile(aim, 4.9f, 0, 2.5f, 0.42f, 1.9f, new Color(0.45f, 0.9f, 1f, 1f));
            if (owner.isBoss)
            {
                SpawnEnemyProjectile(Rotate(aim, 22f), 4.6f, 0, 2.2f, 0.5f, 1.8f, new Color(0.72f, 0.96f, 1f, 1f));
                SpawnEnemyProjectile(Rotate(aim, -22f), 4.6f, 0, 2.2f, 0.5f, 1.8f, new Color(0.72f, 0.96f, 1f, 1f));
            }
            owner.ForceAlert(2.8f);
        }

        if (bossDashTimer <= 0f && owner.HasLineOfSightToPlayer(5.2f))
        {
            bossDashTimer = owner.isBoss ? 3.6f : 5.4f;
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            owner.TryMoveAbilityDelta((Vector3)(direction * (owner.isBoss ? 0.72f : 0.45f)));
            SpawnPulse(new Color(0.42f, 0.88f, 1f, 0.25f), owner.isBoss ? 2.8f : 1.8f, 0.28f);
            owner.ForceAlert(2.5f);
        }
    }

    // What: Run Shade Overlord's projectile spread and summoning pressure.
    // Human: Designed Shade Overlord as a mixed ranged/summon boss.
    // AI: Helped keep elite mode lower intensity than boss mode.
    void UpdateShadeOverlord()
    {
        // Shade Overlord mixes projectile pressure with summons. Both modes keep the same two-part
        // identity, but the non-boss elite version has lower summon count and longer cooldowns.
        bossProjectileTimer -= Time.deltaTime;
        bossSummonTimer -= Time.deltaTime;

        if (bossProjectileTimer <= 0f && owner.HasLineOfSightToPlayer(6.0f))
        {
            bossProjectileTimer = owner.isBoss ? 1.55f : 2.3f;
            Vector2 aim = ((Vector2)player.position - (Vector2)transform.position).normalized;
            SpawnEnemyProjectile(aim, 5.4f, 1, 1.6f, 0.62f, 1.8f, new Color(0.72f, 0.42f, 1f, 1f));
            SpawnEnemyProjectile(Rotate(aim, owner.isBoss ? 35f : 20f), 4.8f, 1, 0f, 1f, 1.7f, new Color(0.9f, 0.55f, 1f, 1f));
            SpawnEnemyProjectile(Rotate(aim, owner.isBoss ? -35f : -20f), 4.8f, 1, 0f, 1f, 1.7f, new Color(0.9f, 0.55f, 1f, 1f));
            owner.ForceAlert(3f);
        }

        if (bossSummonTimer <= 0f && summonsMade < (owner.isBoss ? 6 : 2) && owner.HasLineOfSightToPlayer(5.8f))
        {
            bossSummonTimer = owner.isBoss ? 5.2f : 7.4f;
            int count = owner.isBoss ? 2 : 1;
            for (int i = 0; i < count; i++)
                if (TrySummonSlime()) summonsMade++;
            owner.ForceAlert(3f);
        }
    }

    // What: Run Crystal Titan's dash, projectiles, and low-health shockwave.
    // Human: Designed Crystal Titan as the late heavy-pressure boss.
    // AI: Helped structure its windup, projectile, and pulse timers.
    void UpdateCrystalTitan()
    {
        // Crystal Titan is the heavy pressure boss: projectile shots, dash windup, and low-health
        // shockwaves. owner.isBoss gates the extra side projectiles and stronger shockwave so the
        // later elite version remains readable without feeling like a full boss fight.
        bossProjectileTimer -= Time.deltaTime;
        bossDashTimer -= Time.deltaTime;
        bossPulseTimer -= Time.deltaTime;

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            owner.TryMoveAbilityDelta((Vector3)(dashDirection * ((owner.isBoss ? 6.6f : 5.0f) * Time.deltaTime)));
            return;
        }

        if (dashWindup > 0f)
        {
            dashWindup -= Time.deltaTime;
            if (dashWindup <= 0f)
            {
                dashTimer = 0.34f;
                dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
            }
            return;
        }

        if (bossProjectileTimer <= 0f && owner.HasLineOfSightToPlayer(6.2f))
        {
            bossProjectileTimer = owner.HealthFraction < 0.45f ? 1.1f : (owner.isBoss ? 1.55f : 2.2f);
            Vector2 aim = ((Vector2)player.position - (Vector2)transform.position).normalized;
            SpawnEnemyProjectile(aim, 5.8f, 1, 0f, 1f, 1.6f, new Color(1f, 0.35f, 0.22f, 1f));
            if (owner.isBoss)
            {
                SpawnEnemyProjectile(Rotate(aim, 18f), 5.2f, 1, 0f, 1f, 1.6f, new Color(1f, 0.62f, 0.24f, 1f));
                SpawnEnemyProjectile(Rotate(aim, -18f), 5.2f, 1, 0f, 1f, 1.6f, new Color(1f, 0.62f, 0.24f, 1f));
            }
            owner.ForceAlert(2.8f);
        }

        if (bossDashTimer <= 0f && owner.HasLineOfSightToPlayer(5.4f))
        {
            bossDashTimer = owner.isBoss ? 4.2f : 5.6f;
            dashWindup = owner.isBoss ? 0.55f : 0.68f;
            SpawnPulse(new Color(1f, 0.24f, 0.18f, 0.24f), owner.isBoss ? 2.35f : 1.65f, 0.3f);
            owner.ForceAlert(3f);
        }

        if (bossPulseTimer <= 0f && owner.HealthFraction < 0.65f)
        {
            bossPulseTimer = owner.HealthFraction < 0.3f ? 2.2f : (owner.isBoss ? 3.4f : 4.8f);
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= (owner.isBoss ? 1.65f : 1.25f))
            {
                Player target = player.GetComponent<Player>();
                if (target != null) target.TakeHit(transform.position);
            }
            SpawnPulse(new Color(0.9f, 0.25f, 1f, 0.28f), owner.isBoss ? 3.1f : 2.1f, 0.35f);
            owner.ForceAlert(2.5f);
        }
    }

    // What: Scale ability cooldowns down as floor difficulty rises, but keep a minimum.
    // Human: Requested higher late-game challenge.
    // AI: Helped add caps so attacks remain readable.
    float ScaledInterval(float baseInterval, float minimum)
    {
        // Shorter intervals create more pressure, but each ability keeps a readable lower bound.
        return Mathf.Max(minimum, baseInterval / Mathf.Clamp(difficultyScale, 1f, 1.45f));
    }

    // What: Scale projectile and dash speeds from the floor difficulty value.
    // Human: Wanted later floors to pressure movement more.
    // AI: Helped reuse one difficulty scalar across ability types.
    float ScaledSpeed(float baseSpeed)
    {
        // Projectile and dash speeds scale together with the floor pressure value.
        return baseSpeed * Mathf.Clamp(difficultyScale, 1f, 1.45f);
    }

    // What: Increase ability range modestly on later floors.
    // Human: Wanted high floors to be harder to kite safely.
    // AI: Helped cap range growth so walls still matter.
    float ScaledRange(float baseRange)
    {
        // Range increases are modest so walls and route planning still matter.
        return baseRange + (Mathf.Clamp(difficultyScale, 1f, 1.45f) - 1f) * 1.25f;
    }

    // What: Rotate a direction vector by a number of degrees for projectile spreads.
    // Human: Designed spread shots for boss pressure.
    // AI: Helped keep the math in one helper.
    Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
    }

    // What: Spawn a fading circular pulse for ability warning or impact feedback.
    // Human: Chose pulse colors and ability readability cues.
    // AI: Helped reuse BulletImpact for cleanup.
    void SpawnPulse(Color color, float scale, float life)
    {
        GameObject pulse = new GameObject(kind + " Pulse");
        pulse.transform.SetParent(spawnRoot != null ? spawnRoot : transform.parent);
        pulse.transform.position = transform.position;
        pulse.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = pulse.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(color, 64);
        sr.sortingOrder = 3;

        BulletImpact effect = pulse.AddComponent<BulletImpact>();
        effect.life = life;
        effect.drift = Vector2.zero;
    }
}

/// Simple projectile used by ranged enemies. Damage and slow are optional so the same class can
/// support Spark Spitter and Frost Wisp behaviour.
///
/// Authorship note:
/// - Student-completed code: final projectile behaviour and interaction with Player.
/// - AI-assisted support: comment wording and readability review.
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    public float lifetime = 1.4f;
    public float radius = 0.14f;
    public float slowDuration;
    public float slowMultiplier = 0.5f;
    public LayerMask wallMask;

    float born;

    // What: Record spawn time for projectile lifetime checks.
    // Human: Tuned enemy projectile lifetime.
    // AI: Helped keep projectile expiry local.
    void Start()
    {
        born = Time.time;
    }

    // What: Move the enemy projectile forward and destroy it on timeout or wall hit.
    // Human: Chose projectile speed/range pressure.
    // AI: Helped use CircleCast so shots do not pass through walls.
    void Update()
    {
        if (Time.time - born >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Enemy projectiles use the same manual wall cast idea as player bullets so fast shots do
        // not pass through thin runtime-built wall colliders.
        Vector2 current = transform.position;
        Vector2 direction = transform.up;
        float distance = speed * Time.deltaTime;
        if (wallMask.value != 0 && Physics2D.CircleCast(current, radius, direction, distance, wallMask).collider != null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = current + direction * distance;
    }

    // What: Apply projectile damage and/or slow when hitting the player.
    // Human: Designed enemy projectile effects.
    // AI: Helped support damage-only, slow-only, and combined projectile variants.
    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        // Damage and slow are optional. Frost projectiles can slow without costing a life, while
        // Spark and boss shots can deal direct contact damage.
        if (damage > 0) player.TakeHit(transform.position);
        if (slowDuration > 0f) player.ApplyTemporarySlow(slowMultiplier, slowDuration);
        Destroy(gameObject);
    }
}
