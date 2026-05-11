using UnityEngine;

/// Static-accessor camera shake. Anyone calls CameraShake.Pulse(amount, duration).
public class CameraShake : MonoBehaviour
{
    static CameraShake instance;
    Vector3 home;
    float amount;
    float remaining;

    void Awake() { instance = this; home = transform.localPosition; }

    void LateUpdate()
    {
        if (remaining <= 0f) { transform.localPosition = home; return; }
        remaining -= Time.unscaledDeltaTime;
        Vector2 jitter = Random.insideUnitCircle * amount;
        transform.localPosition = home + new Vector3(jitter.x, jitter.y, 0f);
    }

    public static void Pulse(float strength, float duration)
    {
        if (instance == null) return;
        instance.amount = Mathf.Max(instance.amount, strength);
        instance.remaining = Mathf.Max(instance.remaining, duration);
    }
}
