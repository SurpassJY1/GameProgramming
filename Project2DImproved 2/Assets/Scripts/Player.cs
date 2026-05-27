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

    Vector3 startPosition;
    float invulnerabilityTimer;
    AudioSource audioSrc;
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        startPosition = transform.position;
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

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        MoveWithWallCheck(input);

        if (invulnerabilityTimer > 0f) invulnerabilityTimer -= Time.deltaTime;
        UpdateTint();
    }

    void MoveWithWallCheck(Vector2 input)
    {
        if (input.sqrMagnitude <= 0f) return;

        Vector2 current = transform.position;
        Vector2 delta = input * (moveSpeed * Time.deltaTime);
        if (!Physics2D.CircleCast(current, radius, delta.normalized, delta.magnitude, wallMask))
        {
            transform.position = current + delta;
            return;
        }

        Vector2 xDelta = new Vector2(delta.x, 0f);
        if (Mathf.Abs(xDelta.x) > 0.001f &&
            !Physics2D.CircleCast(current, radius, xDelta.normalized, Mathf.Abs(xDelta.x), wallMask))
            transform.position += (Vector3)xDelta;

        Vector2 yDelta = new Vector2(0f, delta.y);
        if (Mathf.Abs(yDelta.y) > 0.001f &&
            !Physics2D.CircleCast(transform.position, radius, yDelta.normalized, Mathf.Abs(yDelta.y), wallMask))
            transform.position += (Vector3)yDelta;
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
        invulnerabilityTimer = 0f;
        if (sr != null) sr.color = baseColor;
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
        GameManager.I.PlayerHit();

        Vector3 away = (transform.position - source).normalized;
        transform.position += away * 0.35f;
    }

    void Play(AudioClip clip, float volume)
    {
        if (clip != null) audioSrc.PlayOneShot(clip, volume);
    }
}
