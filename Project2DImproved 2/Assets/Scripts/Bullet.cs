using UnityEngine;

/// Legacy classroom shooter projectile kept only so old assets still compile.
public class Bullet : MonoBehaviour
{
    public float speed = 14f;
    public int damage = 1;
    public float lifetime = 1.5f;

    float born;

    void Start() { born = Time.time; }

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
        if (Time.time - born >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Enemy>() != null) Destroy(gameObject);
    }
}
