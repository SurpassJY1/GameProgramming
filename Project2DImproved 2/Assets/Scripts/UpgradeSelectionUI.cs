using UnityEngine;
using UnityEngine.UI;

/// Three-card weapon upgrade picker shown when XP triggers a level-up.
public class UpgradeSelectionUI : MonoBehaviour
{
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
    WeaponUpgradeKind[] currentOptions = new WeaponUpgradeKind[3];

    public void Build(Transform parent)
    {
        root = new GameObject("UpgradeSelectionPage");
        root.transform.SetParent(parent, false);

        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;

        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.76f);

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

    void Start()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnLevelUpAvailable += Show;
        GameManager.I.OnStateChanged += SyncVisibility;
        SyncVisibility();
    }

    void OnDestroy()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnLevelUpAvailable -= Show;
        GameManager.I.OnStateChanged -= SyncVisibility;
    }

    void Show()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.LevelUp) return;

        PickRandomOptions();
        RepaintCards();
        if (root != null) root.SetActive(true);
    }

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

    void PickRandomOptions()
    {
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

    void RepaintCards()
    {
        GameManager gm = GameManager.I;
        if (gm == null) return;

        for (int i = 0; i < currentOptions.Length; i++)
        {
            WeaponUpgradeKind upgrade = currentOptions[i];
            if (titleTexts[i] != null) titleTexts[i].text = gm.GetWeaponUpgradeDisplayName(upgrade);
            if (descriptionTexts[i] != null) descriptionTexts[i].text = gm.GetWeaponUpgradeDescription(upgrade);
            if (levelTexts[i] != null) levelTexts[i].text = "Current Lv. " + gm.GetWeaponUpgradeLevel(upgrade);
        }
    }

    void ChooseOption(int index)
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.LevelUp) return;

        if (root != null) root.SetActive(false);
        GameManager.I.ChooseUpgrade(currentOptions[index]);
    }

    void BuildCard(int index, Vector2 position)
    {
        GameObject card = new GameObject("UpgradeCard_" + index);
        card.transform.SetParent(root.transform, false);

        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(320f, 360f);

        Image image = card.AddComponent<Image>();
        image.color = new Color(0.12f, 0.16f, 0.22f, 0.96f);

        Button button = card.AddComponent<Button>();
        int capturedIndex = index;
        button.onClick.AddListener(() => ChooseOption(capturedIndex));
        cardButtons[index] = button;

        titleTexts[index] = MakeText(card.transform, "", new Vector2(0f, 112f), 30, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.42f));
        titleTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 76f);

        descriptionTexts[index] = MakeText(card.transform, "", new Vector2(0f, -10f), 23, TextAnchor.MiddleCenter, Color.white);
        descriptionTexts[index].GetComponent<RectTransform>().sizeDelta = new Vector2(252f, 150f);

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
}
