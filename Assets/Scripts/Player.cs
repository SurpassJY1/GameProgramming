using UnityEngine;

/// Player ship: WASD/arrows movement, Space or LMB shoots upward, takes contact damage.
/// Holds power-up state (rapid fire, shield) consumed by power-up pickups.
public class Player : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float fireCooldown = 0.25f;
    public float fireCooldownRapid = 0.08f;
    public GameObject bulletPrefab;
    public AudioClip shootClip;
    public AudioClip hitClip;
    public float invulnAfterHit = 1.0f;
    public Vector2 arenaHalf = new Vector2(8.5f, 4.8f);

    // Power-up state — public so HUD can read remaining timers.
    public float rapidTimer;
    public float shieldTimer;
    public bool HasShield => shieldTimer > 0f;
    public bool HasRapid => rapidTimer > 0f;

    float lastShot;
    float invulnTimer;
    AudioSource audioSrc;
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Start()
    {
        // Player GameObject stays alive across restarts, so OnEnable wouldn't
        // re-fire — subscribe to the StartGame event instead.
        if (GameManager.I != null) GameManager.I.OnGameStarted += ResetForNewRun;
        ResetForNewRun();
    }

    void ResetForNewRun()
    {
        transform.position = new Vector3(0f, -3f, 0f);
        rapidTimer = 0f;
        shieldTimer = 0f;
        invulnTimer = 0f;
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        // Movement (legacy Input — works without InputSystem package).
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v).normalized;
        Vector3 pos = transform.position + (Vector3)(dir * moveSpeed * Time.deltaTime);
        pos.x = Mathf.Clamp(pos.x, -arenaHalf.x, arenaHalf.x);
        pos.y = Mathf.Clamp(pos.y, -arenaHalf.y, arenaHalf.y);
        transform.position = pos;

        // Fire
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            float cd = HasRapid ? fireCooldownRapid : fireCooldown;
            if (Time.time - lastShot >= cd) Shoot();
        }

        // Tick power-ups
        if (rapidTimer > 0f) rapidTimer -= Time.deltaTime;
        if (shieldTimer > 0f) shieldTimer -= Time.deltaTime;
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;

        // Shield glow tint
        if (sr != null)
        {
            if (HasShield) sr.color = Color.Lerp(baseColor, new Color(0.4f, 0.8f, 1f, 1f), 0.5f);
            else if (invulnTimer > 0f) sr.color = (Mathf.PingPong(Time.unscaledTime * 12f, 1f) > 0.5f)
                                                  ? new Color(1f, 0.5f, 0.5f, 0.6f) : baseColor;
            else sr.color = baseColor;
        }
    }

    void Shoot()
    {
        lastShot = Time.time;
        if (bulletPrefab == null) return;
        Vector3 spawn = transform.position + Vector3.up * 0.6f;
        SpawnBullet(spawn, Quaternion.identity);
        if (HasRapid)
        {
            // Triple shot while rapid.
            SpawnBullet(spawn + Vector3.left * 0.35f, Quaternion.Euler(0, 0, 15f));
            SpawnBullet(spawn + Vector3.right * 0.35f, Quaternion.Euler(0, 0, -15f));
        }
        if (shootClip != null) audioSrc.PlayOneShot(shootClip, 0.4f);
    }

    void SpawnBullet(Vector3 pos, Quaternion rot)
    {
        // Templates are stored inactive; enable each clone before it spawns.
        var go = Instantiate(bulletPrefab, pos, rot);
        go.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Component-based detection — no custom tags required in TagManager.
        if (other.GetComponent<Enemy>() != null) HandleEnemyContact(other.gameObject);
        else
        {
            var pu = other.GetComponent<PowerUp>();
            if (pu != null) pu.Collect(this);
        }
    }

    void HandleEnemyContact(GameObject enemy)
    {
        if (invulnTimer > 0f) return;

        if (HasShield)
        {
            shieldTimer = 0f;                  // shield absorbs one hit
            if (hitClip != null) audioSrc.PlayOneShot(hitClip, 0.6f);
            CameraShake.Pulse(0.15f, 0.18f);
            Destroy(enemy);
            return;
        }

        if (hitClip != null) audioSrc.PlayOneShot(hitClip, 0.8f);
        CameraShake.Pulse(0.3f, 0.35f);
        invulnTimer = invulnAfterHit;
        GameManager.I.LoseLife();
        Destroy(enemy);
    }

    public void GrantRapid(float seconds) { rapidTimer = Mathf.Max(rapidTimer, seconds); }
    public void GrantShield(float seconds) { shieldTimer = Mathf.Max(shieldTimer, seconds); }
}
