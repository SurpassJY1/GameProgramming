using UnityEngine;

/// Dungeon guard. Patrol enemies move between two points; chasers wake up when
/// the player is close. Older shooter modes remain so legacy scripts compile.
public class Enemy : MonoBehaviour
{
    public enum Kind { Straight, Chaser, Zigzag, Patrol }

    public Kind kind = Kind.Patrol;
    public float speed = 2.0f;
    public int scoreReward = 10;
    public AudioClip deathClip;
    public Transform player;
    public Vector2 patrolOffset = new Vector2(2.5f, 0f);
    public float chaseRange = 3.2f;

    Vector3 startPosition;
    Vector3 patrolTarget;
    float bornX;
    float bornTime;
    AudioSource sharedAudio;

    void Start()
    {
        startPosition = transform.position;
        patrolTarget = startPosition + (Vector3)patrolOffset;
        bornX = transform.position.x;
        bornTime = Time.time;
        if (Camera.main != null) sharedAudio = Camera.main.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (GameManager.I != null && GameManager.I.phase != GameManager.Phase.Playing) return;

        switch (kind)
        {
            case Kind.Patrol:
                Patrol();
                break;
            case Kind.Chaser:
                ChaseOrPatrol();
                break;
            case Kind.Straight:
            case Kind.Zigzag:
                LegacyShooterMove();
                break;
        }
    }

    void Patrol()
    {
        transform.position = Vector3.MoveTowards(transform.position, patrolTarget, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, patrolTarget) < 0.05f)
        {
            patrolTarget = Vector3.Distance(patrolTarget, startPosition) < 0.1f
                ? startPosition + (Vector3)patrolOffset
                : startPosition;
        }
    }

    void ChaseOrPatrol()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= chaseRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            return;
        }

        Patrol();
    }

    void LegacyShooterMove()
    {
        Vector3 p = transform.position;
        if (kind == Kind.Straight)
        {
            p += Vector3.down * speed * Time.deltaTime;
        }
        else
        {
            p.y -= speed * Time.deltaTime;
            p.x = bornX + Mathf.Sin((Time.time - bornTime) * 4f) * 1.5f;
        }

        transform.position = p;
        if (p.y < -7f) Destroy(gameObject);
    }

    public void Kill()
    {
        if (sharedAudio != null && deathClip != null) sharedAudio.PlayOneShot(deathClip, 0.5f);
        SpawnBurst(transform.position, GetComponent<SpriteRenderer>()?.color ?? Color.white);
        CameraShake.Pulse(0.05f, 0.08f);
        Destroy(gameObject);
    }

    public void ResetEnemy()
    {
        transform.position = startPosition;
        patrolTarget = startPosition + (Vector3)patrolOffset;
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
