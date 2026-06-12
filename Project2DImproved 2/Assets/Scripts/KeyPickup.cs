using UnityEngine;

/// Collectible goal item. Once picked up, GameManager records the key state and the exit door can
/// be used unless a boss-floor rule is still sealing the exit.
///
/// Authorship note:
/// - Student-owned implementation: key objective, pickup animation feel, and interaction with the
///   floor-clear loop.
/// - AI-assisted support: review suggestions and explanatory comments for keeping pickup state in
///   GameManager rather than on the collectible object.
public class KeyPickup : MonoBehaviour
{
    public float bobSpeed = 3f;
    public float bobHeight = 0.12f;

    Vector3 startPosition;

    // What: Remember the original key position so the bobbing animation has a stable center.
    // Human: Tuned the visual pickup motion.
    // AI: Suggested separating visual motion from pickup state.
    void Start()
    {
        startPosition = transform.position;
    }

    // What: Animate the key with a small hover and spin so it is easy to notice.
    // Human: Chose the key as the main floor objective.
    // AI: Helped document that this animation does not change gameplay collision.
    void Update()
    {
        // Bobbing and rotation are visual only. The collider remains on the root object so pickup
        // detection stays simple while the sprite moves.
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.Rotate(0f, 0f, 90f * Time.deltaTime);
    }

    // What: Report key collection to GameManager and hide this pickup object.
    // Human: Owned the key objective and floor-clear loop.
    // AI: Suggested keeping the collected flag in GameManager to avoid duplicate state.
    public void Collect()
    {
        // GameManager owns the key flag and objective text. The key object only reports the pickup
        // and hides itself so it cannot be collected twice.
        if (GameManager.I != null) GameManager.I.CollectKey();
        gameObject.SetActive(false);
    }

}
