using UnityEngine;

/// Static-accessor camera shake. Anyone calls CameraShake.Pulse(amount, duration).
public class CameraShake : MonoBehaviour
{
    static CameraShake instance;
    float amount;
    float remaining;
    public Vector3 CurrentOffset { get; private set; }

    void Awake() { instance = this; }

    void LateUpdate()
    {
        if (remaining <= 0f)
        {
            CurrentOffset = Vector3.zero;
            amount = 0f;
            return;
        }

        remaining -= Time.unscaledDeltaTime;
        float normalizedLife = Mathf.Clamp01(remaining / Mathf.Max(0.001f, remaining + Time.unscaledDeltaTime));
        Vector2 jitter = Random.insideUnitCircle * amount * normalizedLife;
        CurrentOffset = new Vector3(jitter.x, jitter.y, 0f);
    }

    public static void Pulse(float strength, float duration)
    {
        if (instance == null) return;
        instance.amount = Mathf.Max(instance.amount, strength);
        instance.remaining = Mathf.Max(instance.remaining, duration);
    }

    public static Vector3 Offset()
    {
        return instance != null ? instance.CurrentOffset : Vector3.zero;
    }
}
