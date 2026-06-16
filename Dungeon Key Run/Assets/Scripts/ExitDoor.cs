using UnityEngine;

/// Exit trigger visual state. Player collision calls GameManager.TryExit from Player; this
/// component only colors the door so the lock state is readable during play.
///
/// Authorship note:
/// - Student-owned implementation: key/exit gameplay rule, door readability requirement, and final
///   color tuning used in the playable build.
/// - AI-assisted support: review suggestions and comments that separate visual feedback from
///   GameManager's actual rule checks.
public class ExitDoor : MonoBehaviour
{
    SpriteRenderer sr;

    // What: Cache the SpriteRenderer used to tint the door as locked or key-ready.
    // Human: Chose color feedback as the door's readability cue.
    // AI: Suggested caching the component instead of looking it up every frame.
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // What: Repaint the door color from GameManager's current key state.
    // Human: Defined the key-before-exit rule and final locked/unlocked colors.
    // AI: Helped clarify that boss-floor sealing is checked elsewhere.
    void Update()
    {
        if (sr == null || GameManager.I == null) return;
        // Blue means the key has been collected. Boss-floor sealing is still enforced by
        // GameManager.TryExit so this visual stays tied to the key objective only.
        sr.color = GameManager.I.hasKey
            ? new Color(0.3f, 0.85f, 1f)
            : new Color(0.25f, 0.35f, 0.55f);
    }
}
