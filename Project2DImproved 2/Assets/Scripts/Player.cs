using UnityEngine;

/// Top-down dungeon player: 8-way movement, wall collision, key pickup,
/// exit interaction, and brief invulnerability after enemy contact.
public class Player : MonoBehaviour
{
    public float moveSpeed = 4.8f;
    public float invulnAfterHit = 1.0f;
    public AudioClip hitClip;
    public AudioClip pickupClip;
    public AudioClip doorClip;

    // Kept for compatibility with older prototype scripts that may still exist.
    public GameObject bulletPrefab;
    public float rapidTimer;
    public float shieldTimer;
    public bool HasShield => false;
    public bool HasRapid => false;

    Rigidbody2D rb;
    AudioSource audioSrc;
    SpriteRenderer sr;
    Color baseColor;
    Vector2 input;
    float invulnTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing)
        {
            input = Vector2.zero;
            return;
        }

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (input.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        UpdateDamageTint();
    }

    void FixedUpdate()
    {
        if (rb == null || GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }

    void ResetForNewRun()
    {
        transform.position = new Vector3(-6.5f, -3.6f, 0f);
        transform.rotation = Quaternion.identity;
        invulnTimer = 0f;
        if (sr != null) sr.color = baseColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var key = other.GetComponent<KeyPickup>();
        if (key != null)
        {
            key.Collect();
            if (pickupClip != null) audioSrc.PlayOneShot(pickupClip, 0.7f);
            return;
        }

        if (other.GetComponent<ExitDoor>() != null)
        {
            if (doorClip != null) audioSrc.PlayOneShot(doorClip, 0.7f);
            GameManager.I.TryExit();
            return;
        }

        if (other.GetComponent<Enemy>() != null) HandleEnemyContact();
    }

    void HandleEnemyContact()
    {
        if (invulnTimer > 0f) return;

        invulnTimer = invulnAfterHit;
        if (hitClip != null) audioSrc.PlayOneShot(hitClip, 0.8f);
        CameraShake.Pulse(0.25f, 0.25f);
        GameManager.I.LoseLife();
    }

    void UpdateDamageTint()
    {
        if (sr == null) return;
        sr.color = invulnTimer > 0f && Mathf.PingPong(Time.unscaledTime * 12f, 1f) > 0.5f
            ? new Color(1f, 0.45f, 0.45f, 0.65f)
            : baseColor;
    }

    public void GrantRapid(float seconds) { rapidTimer = Mathf.Max(rapidTimer, seconds); }
    public void GrantShield(float seconds) { shieldTimer = Mathf.Max(shieldTimer, seconds); }
}
