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
    public LayerMask wallMask;

    Vector3 target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        Vector3 destination = ShouldChase() ? player.position : target;
        float speed = ShouldChase() ? chaseSpeed : patrolSpeed;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (!ShouldChase() && Vector3.Distance(transform.position, target) < 0.05f)
            target = target == pointA ? pointB : pointA;
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
}
