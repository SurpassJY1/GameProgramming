using UnityEngine;

/// Static-accessor camera shake. Gameplay scripts call CameraShake.Pulse(amount, duration)
/// without needing a serialized reference to the active camera.
///
/// Authorship note:
/// - Student-owned implementation: camera feedback requirement, shake strength/duration tuning,
///   and integration with player hit and combat feedback.
/// - AI-assisted support: review suggestions and explanatory comments describing why this class
///   uses a small singleton-style accessor for a runtime-built scene.
public class CameraShake : MonoBehaviour
{
    static CameraShake instance;
    float amount;
    float remaining;
    public Vector3 CurrentOffset { get; private set; }

    // What: Register the active camera shake component so static helper calls can find it.
    // Human: Chose the simple one-camera feedback design for this project.
    // AI: Suggested documenting the singleton-style access pattern for a runtime-built scene.
    void Awake() { instance = this; }

    // What: Decay the current shake and expose the per-frame offset to SmoothCameraFollow.
    // Human: Tuned the shake feel used when the player takes damage.
    // AI: Helped explain why the offset is calculated in LateUpdate.
    void LateUpdate()
    {
        // LateUpdate runs after player/enemy movement so the offset is applied to the final camera
        // position for this frame. SmoothCameraFollow adds this offset after its own damped follow.
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

    // What: Start or strengthen a shake burst from any gameplay script.
    // Human: Decided which events should call this feedback hook.
    // AI: Suggested using max values so weaker pulses do not erase stronger active shakes.
    public static void Pulse(float strength, float duration)
    {
        if (instance == null) return;
        // Multiple impacts in the same moment should extend/intensify the shake instead of
        // replacing a stronger shake with a weaker one.
        instance.amount = Mathf.Max(instance.amount, strength);
        instance.remaining = Mathf.Max(instance.remaining, duration);
    }

    // What: Return the current shake offset, or zero if no camera shake object exists yet.
    // Human: Required a safe camera helper that does not break scene startup.
    // AI: Helped keep this accessor null-safe for generated scenes.
    public static Vector3 Offset()
    {
        return instance != null ? instance.CurrentOffset : Vector3.zero;
    }
}
