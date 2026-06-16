using UnityEngine;

/// Falls toward the bottom; "Chaser" variants curve toward the player.
public class Enemy : MonoBehaviour
{
    public enum Kind { Straight, Chaser, Zigzag }

    public Kind kind = Kind.Straight;
    public float speed = 3.5f;
    public int scoreReward = 10;
    public AudioClip deathClip;
    public Transform player;       // wired by spawner; null for Straight

    float bornX;
    float bornTime;
    AudioSource sharedAudio;

    void Start()
    {
        bornX = transform.position.x;
        bornTime = Time.time;
        // One shared AudioSource on the Camera covers death blips so destroyed
        // enemies still play their sound.
        if (Camera.main != null) sharedAudio = Camera.main.GetComponent<AudioSource>();
    }

    void Update()
    {
        Vector3 p = transform.position;
        switch (kind)
        {
            case Kind.Straight:
                p += Vector3.down * speed * Time.deltaTime;
                break;
            case Kind.Chaser:
                Vector3 toPlayer = (player != null)
                    ? (player.position - p).normalized
                    : Vector3.down;
                // Bias downward so they actually leave the screen.
                Vector3 dir = (toPlayer + Vector3.down * 0.6f).normalized;
                p += dir * speed * Time.deltaTime;
                break;
            case Kind.Zigzag:
                p.y -= speed * Time.deltaTime;
                p.x = bornX + Mathf.Sin((Time.time - bornTime) * 4f) * 1.5f;
                break;
        }
        transform.position = p;

        // Off-screen cleanup.
        if (p.y < -7f) Destroy(gameObject);
    }

    public void Kill()
    {
        if (GameManager.I != null) GameManager.I.AddScore(scoreReward);
        if (sharedAudio != null && deathClip != null) sharedAudio.PlayOneShot(deathClip, 0.5f);
        // Tiny burst — a short scale puff before destroy.
        SpawnBurst(transform.position, GetComponent<SpriteRenderer>()?.color ?? Color.white);
        CameraShake.Pulse(0.05f, 0.08f);
        Destroy(gameObject);
    }

    static void SpawnBurst(Vector3 pos, Color color)
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject p = new GameObject("Particle");
            p.transform.position = pos;
            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = Art2D.SolidCircle(color, 16);
            sr.sortingOrder = 5;
            p.transform.localScale = Vector3.one * 0.25f;
            var fly = p.AddComponent<ParticleFly>();
            fly.velocity = Random.insideUnitCircle * 4f;
        }
    }
}

/// Tiny inline particle: a bit drifts outward, fades, dies after 0.4s.
public class ParticleFly : MonoBehaviour
{
    public Vector2 velocity;
    public float life = 0.4f;
    float t;
    SpriteRenderer sr;

    void Start() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        if (sr != null)
        {
            var c = sr.color; c.a = 1f - (t / life); sr.color = c;
        }
        if (t >= life) Destroy(gameObject);
    }
}
