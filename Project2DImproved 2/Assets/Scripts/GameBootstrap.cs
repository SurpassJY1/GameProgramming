using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Builds the Dungeon Key Run vertical slice at runtime from simple primitives.
public class GameBootstrap : MonoBehaviour
{
    const int WallLayer = 8;

    void Awake()
    {
        BuildCamera();
        BuildGameManager();
        BuildDungeon();
        GameObject player = BuildPlayer();
        BuildKey();
        BuildExit();
        BuildEnemy("Guard_A", player.transform, new Vector3(-4f, 2f, 0f), new Vector3(-1f, 2f, 0f));
        BuildEnemy("Guard_B", player.transform, new Vector3(2f, -1f, 0f), new Vector3(5f, -1f, 0f));
        BuildHUD();
        BuildMenusCanvas();
        EnsureEventSystem();
    }

    void BuildCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }

        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 5.6f;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        if (cam.GetComponent<AudioSource>() == null) cam.gameObject.AddComponent<AudioSource>().playOnAwake = false;
    }

    void BuildGameManager()
    {
        if (GameManager.I != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    void BuildDungeon()
    {
        GameObject floor = new GameObject("Dungeon Floor");
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(14f, 9f, 1f);
        SpriteRenderer floorSr = floor.AddComponent<SpriteRenderer>();
        floorSr.sprite = Art2D.Square(new Color(0.13f, 0.13f, 0.16f));
        floorSr.sortingOrder = -5;

        BuildWall("North Wall", new Vector2(0f, 4.5f), new Vector2(14f, 0.5f));
        BuildWall("South Wall", new Vector2(0f, -4.5f), new Vector2(14f, 0.5f));
        BuildWall("West Wall", new Vector2(-7f, 0f), new Vector2(0.5f, 9f));
        BuildWall("East Wall", new Vector2(7f, 0f), new Vector2(0.5f, 9f));
        BuildWall("Left Chamber Divider", new Vector2(-2.2f, 1.4f), new Vector2(0.45f, 4.4f));
        BuildWall("Right Chamber Divider", new Vector2(2.2f, -1.4f), new Vector2(0.45f, 4.4f));
        BuildWall("Upper Block", new Vector2(3.9f, 1.7f), new Vector2(2.2f, 0.45f));
        BuildWall("Lower Block", new Vector2(-3.9f, -1.7f), new Vector2(2.2f, 0.45f));
    }

    void BuildWall(string name, Vector2 pos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.layer = WallLayer;
        wall.transform.position = pos;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Square(new Color(0.32f, 0.34f, 0.42f));
        sr.sortingOrder = -1;
        wall.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    GameObject BuildPlayer()
    {
        GameObject go = new GameObject("Player");
        go.transform.position = new Vector3(-5.6f, -3.2f, 0f);
        go.transform.localScale = Vector3.one * 0.75f;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(0.45f, 0.95f, 0.55f));
        sr.sortingOrder = 3;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.42f;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        Player player = go.AddComponent<Player>();
        player.wallMask = 1 << WallLayer;
        player.keyClip = Art2D.Chime(660f, 0.35f);
        player.hitClip = Art2D.Noise(0.25f, 12f);
        player.winClip = Art2D.Chime(880f, 0.6f);
        return go;
    }

    void BuildKey()
    {
        GameObject key = new GameObject("Gold Key");
        key.transform.position = new Vector3(5.4f, 3.1f, 0f);
        key.transform.localScale = Vector3.one * 0.45f;
        SpriteRenderer sr = key.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Diamond(new Color(1f, 0.78f, 0.16f));
        sr.sortingOrder = 2;
        CircleCollider2D col = key.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.55f;
        key.AddComponent<KeyPickup>();
    }

    void BuildExit()
    {
        GameObject exit = new GameObject("Exit Door");
        exit.transform.position = new Vector3(5.7f, -3.2f, 0f);
        exit.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
        SpriteRenderer sr = exit.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Square(new Color(0.25f, 0.35f, 0.55f));
        sr.sortingOrder = 1;
        BoxCollider2D col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
        exit.AddComponent<ExitDoor>();
    }

    void BuildEnemy(string name, Transform player, Vector3 a, Vector3 b)
    {
        GameObject enemy = new GameObject(name);
        enemy.transform.position = a;
        enemy.transform.localScale = Vector3.one * 0.65f;
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Diamond(new Color(0.92f, 0.25f, 0.25f));
        sr.sortingOrder = 2;
        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        Enemy guard = enemy.AddComponent<Enemy>();
        guard.player = player;
        guard.pointA = a;
        guard.pointB = b;
        guard.wallMask = 1 << WallLayer;
    }

    void BuildHUD()
    {
        Canvas cv = MakeCanvas("HUDCanvas", 0);
        HUD hud = cv.gameObject.AddComponent<HUD>();
        hud.root = cv.gameObject;
        hud.livesText = MakeText(cv.transform, "Lives: 3", new Vector2(20, -20), new Vector2(0, 1), 30, TextAnchor.UpperLeft, Color.white);
        hud.keyText = MakeText(cv.transform, "Key: Missing", new Vector2(20, -60), new Vector2(0, 1), 26, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.45f));
        hud.timerText = MakeText(cv.transform, "Time: 0s", new Vector2(-20, -20), new Vector2(1, 1), 28, TextAnchor.UpperRight, Color.white);
        hud.objectiveText = MakeText(cv.transform, "", new Vector2(0, 30), new Vector2(0.5f, 0), 24, TextAnchor.LowerCenter, new Color(0.85f, 0.9f, 1f));
        MakeText(cv.transform, "ESC = pause", new Vector2(-20, 30), new Vector2(1, 0), 18, TextAnchor.LowerRight, new Color(1, 1, 1, 0.55f));
    }

    void BuildMenusCanvas()
    {
        Canvas cv = MakeCanvas("MenuCanvas", 10);
        Menus menus = cv.gameObject.AddComponent<Menus>();
        menus.clickClip = Art2D.Tone(550f, 0.07f);
        menus.mainPage = BuildMainPage(cv.transform, menus);
        menus.instructionsPage = BuildInfoPage(cv.transform, "How to Play",
            "Move: WASD or arrow keys\nPause: ESC\n\nGoal: collect the gold key, then reach the blue exit door.\nAvoid red guards. They patrol and chase if you get too close.",
            menus.OnBackToMenu);
        menus.creditsPage = BuildInfoPage(cv.transform, "Credits",
            "Code, sprites and sound effects are original for this module project.\nVisuals and audio are generated at runtime in Art2D.cs.\nNo third-party asset packs are used.",
            menus.OnBackToMenu);
        menus.pausePage = BuildPausePage(cv.transform, menus);
        menus.winPage = BuildEndPage(cv.transform, menus, "ESCAPED", new Color(0.45f, 1f, 0.65f), out menus.winText);
        menus.gameOverPage = BuildEndPage(cv.transform, menus, "CAUGHT", new Color(1f, 0.45f, 0.45f), out menus.gameOverText);
    }

    GameObject BuildMainPage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "MainPage", new Color(0, 0, 0, 0.65f));
        MakeText(page.transform, "DUNGEON KEY RUN", new Vector2(0, 220), new Vector2(0.5f, 0.5f), 72, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f));
        MakeText(page.transform, "Find the key. Avoid the guards. Escape the dungeon.", new Vector2(0, 145), new Vector2(0.5f, 0.5f), 26, TextAnchor.MiddleCenter, Color.white);
        MakeButton(page.transform, "Start Game", new Vector2(0, 40), menus.OnStart);
        MakeButton(page.transform, "Instructions", new Vector2(0, -40), menus.OnInstructions);
        MakeButton(page.transform, "Credits", new Vector2(0, -120), menus.OnCredits);
        MakeButton(page.transform, "Quit", new Vector2(0, -200), menus.OnQuit);
        return page;
    }

    GameObject BuildInfoPage(Transform parent, string title, string body, UnityEngine.Events.UnityAction back)
    {
        GameObject page = MakePagePanel(parent, title + "Page", new Color(0, 0, 0, 0.72f));
        MakeText(page.transform, title, new Vector2(0, 220), new Vector2(0.5f, 0.5f), 56, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f));
        Text text = MakeText(page.transform, body, new Vector2(0, 20), new Vector2(0.5f, 0.5f), 28, TextAnchor.MiddleCenter, Color.white);
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 360);
        MakeButton(page.transform, "< Back", new Vector2(0, -240), back);
        page.SetActive(false);
        return page;
    }

    GameObject BuildPausePage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "PausePage", new Color(0, 0, 0, 0.72f));
        MakeText(page.transform, "PAUSED", new Vector2(0, 180), new Vector2(0.5f, 0.5f), 68, TextAnchor.MiddleCenter, Color.white);
        MakeButton(page.transform, "Resume", new Vector2(0, 40), menus.OnResume);
        MakeButton(page.transform, "Restart", new Vector2(0, -40), menus.OnRestart);
        MakeButton(page.transform, "Main Menu", new Vector2(0, -120), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    GameObject BuildEndPage(Transform parent, Menus menus, string title, Color titleColor, out Text resultText)
    {
        GameObject page = MakePagePanel(parent, title + "Page", new Color(0, 0, 0, 0.78f));
        MakeText(page.transform, title, new Vector2(0, 195), new Vector2(0.5f, 0.5f), 76, TextAnchor.MiddleCenter, titleColor);
        resultText = MakeText(page.transform, "", new Vector2(0, 70), new Vector2(0.5f, 0.5f), 30, TextAnchor.MiddleCenter, Color.white);
        resultText.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 120);
        MakeButton(page.transform, "Play Again", new Vector2(0, -60), menus.OnRestart);
        MakeButton(page.transform, "Main Menu", new Vector2(0, -140), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    Canvas MakeCanvas(string name, int sortOrder)
    {
        GameObject cv = new GameObject(name);
        Canvas canvas = cv.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        CanvasScaler scaler = cv.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cv.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    GameObject MakePagePanel(Transform parent, string name, Color bg)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = bg;
        return panel;
    }

    Text MakeText(Transform parent, string content, Vector2 pos, Vector2 anchor, int size, TextAnchor align, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, 80);
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

    Button MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360, 70);
        go.AddComponent<Image>().color = new Color(0.18f, 0.33f, 0.7f, 0.95f);
        Button button = go.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        GameObject labelGo = new GameObject("Text");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform trt = labelGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        Text text = labelGo.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return button;
    }

    void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }
}
