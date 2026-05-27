using UnityEngine;

/// Exit trigger. The rule check lives in GameManager so UI feedback stays consistent.
public class ExitDoor : MonoBehaviour
{
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null || GameManager.I == null) return;
        sr.color = GameManager.I.hasKey
            ? new Color(0.3f, 0.85f, 1f)
            : new Color(0.25f, 0.35f, 0.55f);
    }
}
