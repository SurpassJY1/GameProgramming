using UnityEngine;
using UnityEngine.UI;

/// Three-card passive upgrade picker shown after a floor is cleared.
public class PassiveUpgradeSelectionUI : MonoBehaviour
{
    const string UiPanelSpritePath = "generated/pixel-cute-dungeon/selected/ui_panel.png";

    readonly PassiveUpgradeKind[] allUpgrades =
    {
        PassiveUpgradeKind.MaxLivesUp,
        PassiveUpgradeKind.MoveSpeedUp,
        PassiveUpgradeKind.FireCooldownBonus,
        PassiveUpgradeKind.XPBonus
    };

    GameObject root;
    Button[] cardButtons = new Button[3];
    Text[] titleTexts = new Text[3];
    Text[] descriptionTexts = new Text[3];
    Text[] levelTexts = new Text[3];
    Image[] iconImages = new Image[3];
    PassiveUpgradeKind[] currentOptions = new PassiveUpgradeKind[3];

    public void Build(Transform parent)
    {
        root = new GameObject("PassiveUpgradeSelectionPage");
        root.transform.SetParent(parent, false);

        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;

        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(0.01f, 0.015f, 0.025f, 0.84f);

        Text title = MakeText(root.transform, "CHOOSE A PASSIVE UPGRADE", new Vector2(0f, 240f), 44, TextAnchor.MiddleCenter, new Color(0.58f, 1f, 0.78f));
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(980f, 80f);

        Text hint = MakeText(root.transform, "Select one passive to enter the next floor.", new Vector2(0f, 188f), 24, TextAnchor.MiddleCenter, new Color(0.86f, 0.92f, 1f));
        hint.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 55f);

        float[] xPositions = { -380f, 0f, 380f };
        for (int i = 0; i < cardButtons.Length; i++)
        {
            BuildCard(i, new Vector2(xPositions[i], -10f));
        }

        root.SetActive(false);
    }

    void Start()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnPassiveUpgradeAvailable += Show;
        GameManager.I.OnStateChanged += SyncVisibility;
        SyncVisibility();
    }

    void OnDestroy()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnPassiveUpgradeAvailable -= Show;
        GameManager.I.OnStateChanged -= SyncVisibility;
    }

    void Show()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.PassiveUpgrade) return;

        PickRandomOptions();
        RepaintCards();
        if (root != null) root.SetActive(true);
    }

    void SyncVisibility()
    {
        if (root == null || GameManager.I == null) return;

        bool shouldShow = GameManager.I.phase == GameManager.Phase.PassiveUpgrade;
        if (shouldShow && !root.activeSelf)
        {
            PickRandomOptions();
            RepaintCards();
        }

        root.SetActive(shouldShow);
    }

    void PickRandomOptions()
    {
        PassiveUpgradeKind[] pool = new PassiveUpgradeKind[allUpgrades.Length];
        for (int i = 0; i < allUpgrades.Length; i++) pool[i] = allUpgrades[i];

        for (int i = 0; i < currentOptions.Length; i++)
        {
            int selectedIndex = Random.Range(i, pool.Length);
            PassiveUpgradeKind selected = pool[selectedIndex];
            pool[selectedIndex] = pool[i];
            pool[i] = selected;
            currentOptions[i] = selected;
        }
    }

    void RepaintCards()
    {
        GameManager gm = GameManager.I;
        if (gm == null) return;

        for (int i = 0; i < currentOptions.Length; i++)
        {
            PassiveUpgradeKind upgrade = currentOptions[i];
            if (titleTexts[i] != null) titleTexts[i].text = gm.GetPassiveUpgradeDisplayName(upgrade);
            if (descriptionTexts[i] != null) descriptionTexts[i].text = gm.GetPassiveUpgradeDescription(upgrade);
            if (levelTexts[i] != null) levelTexts[i].text = "Current Lv. " + gm.GetPassiveUpgradeLevel(upgrade);
            if (iconImages[i] != null) iconImages[i].sprite = Art2D.PassiveUpgradeIcon(upgrade);
        }
    }

    void ChooseOption(int index)
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.PassiveUpgrade) return;

        if (root != null) root.SetActive(false);
        GameManager.I.ChoosePassiveUpgrade(currentOptions[index]);
    }

    void BuildCard(int index, Vector2 position)
    {
        GameObject card = new GameObject("PassiveUpgradeCard_" + index);
        card.transform.SetParent(root.transform, false);

        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(320f, 360f);

        Image image = card.AddComponent<Image>();
        image.sprite = Art2D.FromPngFile(UiPanelSpritePath, 100f);
        image.color = new Color(0.85f, 0.96f, 0.9f, 0.96f);

        Button button = card.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.85f, 0.96f, 0.9f, 0.96f);
        colors.highlightedColor = new Color(0.88f, 1f, 0.82f, 1f);
        colors.pressedColor = new Color(0.68f, 0.84f, 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        int capturedIndex = index;
        button.onClick.AddListener(() => ChooseOption(capturedIndex));
        cardButtons[index] = button;

        MakeImage(card.transform, "CardAccent", new Vector2(0f, 172f), new Vector2(290f, 5f), new Color(0.58f, 1f, 0.78f, 0.9f));
        Image iconGlow = MakeImage(card.transform, "IconGlow", new Vector2(0f, 42f), new Vector2(84f, 84f), new Color(0.58f, 1f, 0.78f, 0.14f));
        iconGlow.sprite = Art2D.SolidCircle(Color.white, 96);
        iconImages[index] = MakeImage(card.transform, "UpgradeIcon", new Vector2(0f, 42f), new Vector2(68f, 68f), Color.white);
        iconImages[index].sprite = Art2D.PassiveUpgradeIcon(PassiveUpgradeKind.MaxLivesUp);

        titleTexts[index] = MakeText(card.transform, "", new Vector2(0f, 112f), 30, TextAnchor.MiddleCenter, new Color(0.58f, 1f, 0.78f));
        titleTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 76f);

        descriptionTexts[index] = MakeText(card.transform, "", new Vector2(0f, -52f), 23, TextAnchor.MiddleCenter, Color.white);
        descriptionTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(252f, 112f);

        levelTexts[index] = MakeText(card.transform, "", new Vector2(0f, -126f), 21, TextAnchor.MiddleCenter, new Color(0.72f, 0.94f, 1f));
        levelTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(250f, 46f);
    }

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
