using UnityEngine;

/// Smooth camera follow with clamped dungeon bounds and additive shake offset.
public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;
    public Vector2 minBounds = new Vector2(-7f, -4.5f);
    public Vector2 maxBounds = new Vector2(7f, 4.5f);

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        float halfHeight = Camera.main != null ? Camera.main.orthographicSize : 5.5f;
        float halfWidth = halfHeight * (Screen.width / Mathf.Max(1f, (float)Screen.height));

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        float minX = minBounds.x + halfWidth;
        float maxX = maxBounds.x - halfWidth;
        float minY = minBounds.y + halfHeight;
        float maxY = maxBounds.y - halfHeight;

        // If viewport exceeds dungeon bounds, lock to center on that axis.
        if (minX > maxX) { float centerX = (minBounds.x + maxBounds.x) * 0.5f; minX = maxX = centerX; }
        if (minY > maxY) { float centerY = (minBounds.y + maxBounds.y) * 0.5f; minY = maxY = centerY; }

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        Vector3 smooth = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.position = smooth + CameraShake.Offset();
    }
}
