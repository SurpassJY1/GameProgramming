using UnityEngine;
using UnityEngine.UI;

/// Shows short first-run goal prompts near the player without blocking movement or combat.
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

    public void Build(Transform parent)
    {
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

    void Start()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnGameStarted += ResetForRun;
        GameManager.I.OnStateChanged += CheckForKeyPrompt;
    }

    void OnDestroy()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnGameStarted -= ResetForRun;
        GameManager.I.OnStateChanged -= CheckForKeyPrompt;
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;

        FollowPlayer();
        timer -= Time.unscaledDeltaTime;
        group.alpha = Mathf.Clamp01(timer / 0.7f);
        if (timer <= 0f) root.SetActive(false);
    }

    void ResetForRun()
    {
        shownReachExit = false;
        player = null;

        if (GameManager.I != null && GameManager.I.ShouldShowTutorialPrompts())
        {
            ShowPrompt("Find the gold key");
        }
    }

    void CheckForKeyPrompt()
    {
        GameManager gm = GameManager.I;
        if (gm == null || !gm.ShouldShowTutorialPrompts()) return;
        if (shownReachExit || gm.phase != GameManager.Phase.Playing || !gm.hasKey) return;

        ShowPrompt("Reach the blue exit");
        shownReachExit = true;
    }

    void ShowPrompt(string message)
    {
        if (root == null || promptText == null) return;

        promptText.text = message;
        timer = PromptDuration;
        group.alpha = 1f;
        root.SetActive(true);
        FollowPlayer();
    }

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
            promptRt.anchoredPosition = new Vector2(0f, 90f);
            return;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(player.position + Vector3.up * 0.95f);
        promptRt.position = screenPosition;
    }
}
