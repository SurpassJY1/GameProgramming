using UnityEngine;

/// Travels straight up (or along its rotation), kills enemies on contact,
/// self-destructs after `lifetime` seconds.
public class Bullet : MonoBehaviour
{
    public float speed = 14f;
    public float lifetime = 1.5f;
    public int scoreOnKill = 10;

    float born;

    void Start() { born = Time.time; }

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
        if (Time.time - born >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;
        enemy.Kill();
        Destroy(gameObject);
    }
}
