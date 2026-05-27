using UnityEngine;

/// Legacy classroom shooter pickup kept only so old assets still compile.
public class PowerUp : MonoBehaviour
{
    public enum Kind { Rapid, Shield, ExtraLife }

    public Kind kind = Kind.Rapid;
    public float fallSpeed = 1.5f;
    public float duration = 6f;       // for timed power-ups
    public AudioClip pickupClip;

    AudioSource sharedAudio;

    void Start()
    {
        if (Camera.main != null) sharedAudio = Camera.main.GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        // Gentle bob spin — easy to spot.
        transform.Rotate(0, 0, 90f * Time.deltaTime);
        if (transform.position.y < -7f) Destroy(gameObject);
    }

    public void Collect(Player p)
    {
        if (sharedAudio != null && pickupClip != null) sharedAudio.PlayOneShot(pickupClip, 0.6f);
        Destroy(gameObject);
    }
}
