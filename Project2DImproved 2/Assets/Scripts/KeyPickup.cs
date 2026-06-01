using UnityEngine;

/// Collectible goal item. Once picked up, the exit door can be used.
public class KeyPickup : MonoBehaviour
{
    public float bobSpeed = 3f;
    public float bobHeight = 0.12f;

    Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.Rotate(0f, 0f, 90f * Time.deltaTime);
    }

    public void Collect()
    {
        if (GameManager.I != null) GameManager.I.CollectKey();
        gameObject.SetActive(false);
    }

}
