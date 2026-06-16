using UnityEngine;
using UnityEngine.UI;

/// Shows short first-run goal prompts near the player without blocking movement or combat.
/// Prompts are intentionally lightweight: they appear for the first few successful runs, then stop
/// appearing once the player has demonstrated the floor objective.
///
/// Authorship note:
/// - Student-owned implementation: tutorial goals, prompt wording, and non-blocking behaviour.
/// - AI-assisted support: runtime UI layout suggestions and comments explaining the PlayerPrefs
///   gating and player-follow positioning.
public class TutorialPromptUI : MonoBehaviour
{
    const float PromptDuration = 2.6f;

    GameObject root;
    CanvasGroup group;
    Text promptText;
    RectTransform promptRt;
    Transform player;
    Camera mainCamera;
    float timer;
    bool shownReachExit;

    // What: Build the tutorial prompt panel under the runtime HUD canvas.
    // Human: Chose the tutorial prompt wording and non-blocking style.
    // AI: Helped structure the generated UI objects and RectTransforms.
    public void Build(Transform parent)
    {
        // The prompt is built as a small overlay panel so it can be attached to the runtime-created
        // HUD canvas without requiring an authored prefab.
        root = new GameObject("TutorialPrompt");
        root.transform.SetParent(parent, false);

        promptRt = root.AddComponent<RectTransform>();
        promptRt.anchorMin = promptRt.anchorMax = new Vector2(0.5f, 0.5f);
        promptRt.pivot = new Vector2(0.5f, 0.5f);
        promptRt.sizeDelta = new Vector2(360f, 60f);

        group = root.AddComponent<CanvasGroup>();

        GameObject background = new GameObject("Background");
        background.transform.SetParent(root.transform, false);
        RectTransform bgRt = background.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        Image bg = background.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.58f);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(root.transform, false);
        RectTransform textRt = textObject.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(14f, 6f);
        textRt.offsetMax = new Vector2(-14f, -6f);

        promptText = textObject.AddComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 24;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = new Color(1f, 0.92f, 0.48f);
        promptText.horizontalOverflow = HorizontalWrapMode.Wrap;
        promptText.verticalOverflow = VerticalWrapMode.Overflow;

        root.SetActive(false);
    }

    // What: Subscribe to run-start and state-change events used by tutorial prompts.
    // Human: Chose when tutorial prompts should appear.
    // AI: Helped use events instead of frame-by-frame checks for key collection.
    void Start()
    {
        if (GameManager.I == null) return;

        // A new run may show the first prompt, while state changes are enough to detect key pickup
        // and show the exit prompt.
        GameManager.I.OnGameStarted += ResetForRun;
        GameManager.I.OnStateChanged += CheckForKeyPrompt;
    }

    // What: Remove tutorial event subscriptions when the UI is destroyed.
    // Human: Owned runtime UI lifecycle.
    // AI: Suggested cleanup to avoid duplicate prompt calls.
    void OnDestroy()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnGameStarted -= ResetForRun;
        GameManager.I.OnStateChanged -= CheckForKeyPrompt;
    }

    // What: Follow the player, fade the prompt, and hide it when its timer expires.
    // Human: Tuned prompt duration and visual behaviour.
    // AI: Helped explain why unscaled time keeps the fade reliable during pauses.
    void Update()
    {
        if (root == null || !root.activeSelf) return;

        // Use unscaled time so the prompt fades correctly even if gameplay is paused by another UI.
        FollowPlayer();
        timer -= Time.unscaledDeltaTime;
        group.alpha = Mathf.Clamp01(timer / 0.7f);
        if (timer <= 0f) root.SetActive(false);
    }

    // What: Reset per-run tutorial flags and optionally show the first objective prompt.
    // Human: Chose the tutorial progression rules.
    // AI: Helped gate repeated prompts through GameManager.ShouldShowTutorialPrompts.
    void ResetForRun()
    {
        shownReachExit = false;
        player = null;

        // GameManager decides whether tutorial prompts should still be shown for this profile.
        if (GameManager.I != null && GameManager.I.ShouldShowTutorialPrompts())
        {
            ShowPrompt("Find the gold key");
        }
    }

    // What: Show the exit prompt once after the player collects the key.
    // Human: Chose key pickup as the moment for the second tutorial hint.
    // AI: Helped make the prompt one-shot per run.
    void CheckForKeyPrompt()
    {
        GameManager gm = GameManager.I;
        if (gm == null || !gm.ShouldShowTutorialPrompts()) return;
        if (shownReachExit || gm.phase != GameManager.Phase.Playing || !gm.hasKey) return;

        // Show the second prompt exactly once per run, immediately after key collection.
        ShowPrompt("Reach the blue exit");
        shownReachExit = true;
    }

    // What: Put text into the prompt, restart its timer, and make it visible.
    // Human: Chose concise prompt messages.
    // AI: Helped keep prompt setup in one helper.
    void ShowPrompt(string message)
    {
        if (root == null || promptText == null) return;

        promptText.text = message;
        timer = PromptDuration;
        group.alpha = 1f;
        root.SetActive(true);
        FollowPlayer();
    }

    // What: Place the prompt near the player's screen position.
    // Human: Wanted hints near the character instead of fixed menu text.
    // AI: Helped implement world-to-screen following with a safe fallback.
    void FollowPlayer()
    {
        if (promptRt == null) return;
        if (mainCamera == null) mainCamera = Camera.main;
        if (player == null)
        {
            Player playerComponent = FindFirstObjectByType<Player>();
            if (playerComponent != null) player = playerComponent.transform;
        }

        if (mainCamera == null || player == null)
        {
            // Fallback keeps the prompt visible if the player or camera has not been created yet.
            promptRt.anchoredPosition = new Vector2(0f, 90f);
            return;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(player.position + Vector3.up * 0.95f);
        promptRt.position = screenPosition;
    }
}
