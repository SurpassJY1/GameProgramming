using UnityEngine;
using UnityEngine.UI;

/// Three-card weapon upgrade picker shown when XP triggers a level-up.
/// It is generated at runtime so the project can rebuild a complete playable UI from code.
///
/// Authorship note:
/// - Student-owned implementation: weapon upgrade list, level-up timing, card text, and final
///   interaction flow.
/// - AI-assisted support: layout review, runtime UI organization suggestions, and explanatory
///   comments for random option selection and card repainting.
public class UpgradeSelectionUI : MonoBehaviour
{
    const string UiPanelSpritePath = "generated/pixel-cute-dungeon/selected/ui_panel.png";

    readonly WeaponUpgradeKind[] allUpgrades =
    {
        WeaponUpgradeKind.ExtraProjectile,
        WeaponUpgradeKind.RapidFire,
        WeaponUpgradeKind.DamageUp,
        WeaponUpgradeKind.PiercingShot,
        WeaponUpgradeKind.BurnShot,
        WeaponUpgradeKind.SlowShot,
        WeaponUpgradeKind.ExplosiveShot
    };

    GameObject root;
    Button[] cardButtons = new Button[3];
    Text[] titleTexts = new Text[3];
    Text[] descriptionTexts = new Text[3];
    Text[] levelTexts = new Text[3];
    Image[] iconImages = new Image[3];
    WeaponUpgradeKind[] currentOptions = new WeaponUpgradeKind[3];

    // What: Build the full weapon-upgrade overlay under the runtime UI canvas.
    // Human: Chose the three-card upgrade interaction.
    // AI: Helped organize generated RectTransforms and cached widget references.
    public void Build(Transform parent)
    {
        // GameBootstrap calls this after creating the UI canvas. No scene prefab is required.
        root = new GameObject("UpgradeSelectionPage");
        root.transform.SetParent(parent, false);

        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;

        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(0.01f, 0.015f, 0.025f, 0.82f);

        Text title = MakeText(root.transform, "CHOOSE A WEAPON UPGRADE", new Vector2(0f, 240f), 44, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.42f));
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(980f, 80f);

        Text hint = MakeText(root.transform, "Select one upgrade to continue the run.", new Vector2(0f, 188f), 24, TextAnchor.MiddleCenter, new Color(0.86f, 0.92f, 1f));
        hint.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 55f);

        float[] xPositions = { -380f, 0f, 380f };
        for (int i = 0; i < cardButtons.Length; i++)
        {
            BuildCard(i, new Vector2(xPositions[i], -10f));
        }

        root.SetActive(false);
    }

    // What: Subscribe to level-up events after GameManager exists.
    // Human: Decided that XP level-ups pause gameplay.
    // AI: Helped use events so this UI appears only when needed.
    void Start()
    {
        if (GameManager.I == null) return;

        // Weapon upgrade choices appear when XP crosses the next-level threshold.
        GameManager.I.OnLevelUpAvailable += Show;
        GameManager.I.OnStateChanged += SyncVisibility;
        SyncVisibility();
    }

    // What: Unsubscribe from events when the UI object is destroyed.
    // Human: Owned scene lifecycle through runtime construction.
    // AI: Suggested cleanup to avoid stale event handlers.
    void OnDestroy()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnLevelUpAvailable -= Show;
        GameManager.I.OnStateChanged -= SyncVisibility;
    }

    // What: Roll options, repaint cards, and show the level-up overlay.
    // Human: Chose the upgrade list and presentation timing.
    // AI: Helped keep card rerolling tied to opening the overlay.
    void Show()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.LevelUp) return;

        // Re-roll options every level-up so the player sees a fresh three-card choice.
        PickRandomOptions();
        RepaintCards();
        if (root != null) root.SetActive(true);
    }

    // What: Keep the overlay active only during the LevelUp phase.
    // Human: Defined phase-based upgrade selection.
    // AI: Suggested this defensive sync for pause/restart edge cases.
    void SyncVisibility()
    {
        if (root == null || GameManager.I == null) return;

        bool shouldShow = GameManager.I.phase == GameManager.Phase.LevelUp;
        if (shouldShow && !root.activeSelf)
        {
            PickRandomOptions();
            RepaintCards();
        }

        root.SetActive(shouldShow);
    }

    // What: Select three unique weapon upgrades from the full upgrade pool.
    // Human: Chose random three-card selection instead of showing every upgrade.
    // AI: Suggested partial Fisher-Yates shuffling to avoid duplicates.
    void PickRandomOptions()
    {
        // Partial Fisher-Yates shuffle: selects three unique upgrades from the full pool.
        WeaponUpgradeKind[] pool = new WeaponUpgradeKind[allUpgrades.Length];
        for (int i = 0; i < allUpgrades.Length; i++) pool[i] = allUpgrades[i];

        for (int i = 0; i < currentOptions.Length; i++)
        {
            int selectedIndex = Random.Range(i, pool.Length);
            WeaponUpgradeKind selected = pool[selectedIndex];
            pool[selectedIndex] = pool[i];
            pool[i] = selected;
            currentOptions[i] = selected;
        }
    }

    // What: Copy the current upgrade option data into the visible card widgets.
    // Human: Wrote final upgrade names, descriptions, and icon choices.
    // AI: Helped centralize display text in GameManager and icons in Art2D.
    void RepaintCards()
    {
        GameManager gm = GameManager.I;
        if (gm == null) return;

        // GameManager owns upgrade labels and levels; this class only renders the current choices.
        for (int i = 0; i < currentOptions.Length; i++)
        {
            WeaponUpgradeKind upgrade = currentOptions[i];
            if (titleTexts[i] != null) titleTexts[i].text = gm.GetWeaponUpgradeDisplayName(upgrade);
            if (descriptionTexts[i] != null) descriptionTexts[i].text = gm.GetWeaponUpgradeDescription(upgrade);
            if (levelTexts[i] != null) levelTexts[i].text = "Current Lv. " + gm.GetWeaponUpgradeLevel(upgrade);
            if (iconImages[i] != null) iconImages[i].sprite = Art2D.WeaponUpgradeIcon(upgrade);
        }
    }

    // What: Apply the selected weapon upgrade and close the overlay.
    // Human: Chose that one card is selected per level-up.
    // AI: Helped route upgrade application through GameManager for consistent state.
    void ChooseOption(int index)
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.LevelUp) return;

        // Upgrade application resumes gameplay through GameManager after the selection is recorded.
        if (root != null) root.SetActive(false);
        GameManager.I.ChooseUpgrade(currentOptions[index]);
    }

    // What: Construct one clickable upgrade card and cache its child UI references.
    // Human: Chose card size, colors, and screen layout.
    // AI: Helped break card creation into a reusable method.
    void BuildCard(int index, Vector2 position)
    {
        // Build a reusable card shell, then cache child text/image references for RepaintCards.
        GameObject card = new GameObject("UpgradeCard_" + index);
        card.transform.SetParent(root.transform, false);

        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(320f, 360f);

        Image image = card.AddComponent<Image>();
        image.sprite = Art2D.FromPngFile(UiPanelSpritePath, 100f);
        image.color = new Color(0.85f, 0.9f, 1f, 0.96f);

        Button button = card.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.85f, 0.9f, 1f, 0.96f);
        colors.highlightedColor = new Color(1f, 0.96f, 0.78f, 1f);
        colors.pressedColor = new Color(0.72f, 0.78f, 0.95f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        int capturedIndex = index;
        button.onClick.AddListener(() => ChooseOption(capturedIndex));
        cardButtons[index] = button;

        MakeImage(card.transform, "CardAccent", new Vector2(0f, 172f), new Vector2(290f, 5f), new Color(1f, 0.72f, 0.18f, 0.92f));
        Image iconGlow = MakeImage(card.transform, "IconGlow", new Vector2(0f, 42f), new Vector2(84f, 84f), new Color(1f, 0.72f, 0.18f, 0.16f));
        iconGlow.sprite = Art2D.SolidCircle(Color.white, 96);
        iconImages[index] = MakeImage(card.transform, "UpgradeIcon", new Vector2(0f, 42f), new Vector2(68f, 68f), Color.white);
        iconImages[index].sprite = Art2D.WeaponUpgradeIcon(WeaponUpgradeKind.ExtraProjectile);

        titleTexts[index] = MakeText(card.transform, "", new Vector2(0f, 112f), 30, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.42f));
        titleTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 76f);

        descriptionTexts[index] = MakeText(card.transform, "", new Vector2(0f, -52f), 23, TextAnchor.MiddleCenter, Color.white);
        descriptionTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(252f, 112f);

        levelTexts[index] = MakeText(card.transform, "", new Vector2(0f, -126f), 21, TextAnchor.MiddleCenter, new Color(0.72f, 0.94f, 1f));
        levelTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(250f, 46f);
    }

    // What: Create a Text object with common sizing and font defaults.
    // Human: Chose the visible UI text content and color style.
    // AI: Suggested a helper to avoid repeating RectTransform/Text setup.
    Text MakeText(Transform parent, string content, Vector2 position, int size, TextAnchor align, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(280f, 80f);

        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = align;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    // What: Create an Image object with common RectTransform setup.
    // Human: Chose where icons and accents appear on each card.
    // AI: Suggested a helper to keep runtime UI construction readable.
    Image MakeImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }
}
