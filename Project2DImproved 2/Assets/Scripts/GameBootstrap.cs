using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Builds the complete Dungeon Key Run vertical slice at runtime.
/// The project uses procedural sprites/audio so the repository stays small and legal-safe.
public class GameBootstrap : MonoBehaviour
{
    Transform playerTransform;

    void Awake()
    {
        BuildCamera();
        BuildGameManager();
        BuildDungeon();

        GameObject player = BuildPlayer();
        playerTransform = player.transform;

        BuildKey(new Vector3(5.8f, 3.3f, 0f));
        BuildExit(new Vector3(7.1f, -3.6f, 0f));
        BuildEnemies();

        BuildHUD(player.GetComponent<Player>());
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

        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.orthographic = true;
        cam.orthographicSize = 5.3f;
        cam.backgroundColor = new Color(0.045f, 0.04f, 0.055f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (cam.GetComponent<CameraShake>() == null) cam.gameObject.AddComponent<CameraShake>();
        if (cam.GetComponent<AudioSource>() == null) cam.gameObject.AddComponent<AudioSource>().playOnAwake = false;
    }

    void BuildGameManager()
    {
        if (GameManager.I != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    void BuildDungeon()
    {
        GameObject floor = new GameObject("DungeonFloor");
        floor.transform.localScale = new Vector3(16.5f, 9.2f, 1f);
        var floorSprite = floor.AddComponent<SpriteRenderer>();
        floorSprite.sprite = Art2D.Square(new Color(0.12f, 0.11f, 0.14f));
        floorSprite.sortingOrder = -5;

        // Boundary walls.
        BuildWall("Wall_Top", new Vector3(0f, 4.65f, 0f), new Vector2(16.5f, 0.35f));
        BuildWall("Wall_Bottom", new Vector3(0f, -4.65f, 0f), new Vector2(16.5f, 0.35f));
        BuildWall("Wall_Left", new Vector3(-8.25f, 0f, 0f), new Vector2(0.35f, 9.2f));
        BuildWall("Wall_Right", new Vector3(8.25f, 0f, 0f), new Vector2(0.35f, 9.2f));

        // Interior layout: enough walls to make route choice meaningful without becoming maze-heavy.
        BuildWall("Wall_StartBarrier", new Vector3(-3.8f, -2.1f, 0f), new Vector2(0.45f, 4.0f));
        BuildWall("Wall_CentreNorth", new Vector3(0.2f, 1.45f, 0f), new Vector2(5.2f, 0.45f));
        BuildWall("Wall_CentreSouth", new Vector3(2.2f, -1.75f, 0f), new Vector2(4.4f, 0.45f));
        BuildWall("Wall_KeyRoom", new Vector3(4.1f, 2.35f, 0f), new Vector2(0.45f, 3.0f));

        BuildLabel("Start", new Vector3(-6.6f, -4.25f, 0f), new Color(0.7f, 1f, 0.75f));
        BuildLabel("Key", new Vector3(5.8f, 2.7f, 0f), new Color(1f, 0.88f, 0.25f));
        BuildLabel("Exit", new Vector3(7.1f, -3.0f, 0f), new Color(0.45f, 0.9f, 1f));
    }

    void BuildWall(string name, Vector3 pos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = pos;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Square(new Color(0.28f, 0.26f, 0.34f));
        sr.sortingOrder = -1;

        var col = wall.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
    }

    GameObject BuildPlayer()
    {
        GameObject go = new GameObject("Player");
        go.tag = "Player";
        go.transform.position = new Vector3(-6.5f, -3.6f, 0f);
        go.transform.localScale = Vector3.one * 0.68f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Triangle(new Color(0.45f, 1f, 0.58f));
        sr.sortingOrder = 3;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.36f;
        col.isTrigger = false;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var p = go.AddComponent<Player>();
        p.hitClip = Art2D.Noise(0.25f, 16f);
        p.pickupClip = Art2D.Chime(740f, 0.35f);
        p.doorClip = Art2D.Tone(330f, 0.12f);
        return go;
    }

    void BuildKey(Vector3 pos)
    {
        GameObject key = new GameObject("GoldKey");
        key.transform.position = pos;
        key.transform.localScale = Vector3.one * 0.5f;

        var sr = key.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Diamond(new Color(1f, 0.82f, 0.2f));
        sr.sortingOrder = 2;

        var col = key.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        var rb = key.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        key.AddComponent<KeyPickup>();
    }

    void BuildExit(Vector3 pos)
    {
        GameObject door = new GameObject("ExitDoor");
        door.transform.position = pos;
        door.transform.localScale = new Vector3(0.85f, 1.25f, 1f);

        var sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Square(new Color(0.25f, 0.35f, 0.55f));
        sr.sortingOrder = 1;

        var col = door.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;

        door.AddComponent<ExitDoor>();
    }

    void BuildEnemies()
    {
        BuildEnemy("Guard_West", new Vector3(-1.4f, -3.25f, 0f), new Vector2(0f, 2.4f), Enemy.Kind.Patrol, 1.65f);
        BuildEnemy("Guard_Centre", new Vector3(1.4f, 0.05f, 0f), new Vector2(2.6f, 0f), Enemy.Kind.Patrol, 1.8f);
        BuildEnemy("Guard_KeyRoom", new Vector3(5.8f, 1.15f, 0f), new Vector2(0f, 1.9f), Enemy.Kind.Chaser, 1.55f);
    }

    void BuildEnemy(string name, Vector3 pos, Vector2 patrolOffset, Enemy.Kind kind, float speed)
    {
        GameObject enemy = new GameObject(name);
        enemy.transform.position = pos;
        enemy.transform.localScale = Vector3.one * 0.62f;

        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Diamond(kind == Enemy.Kind.Chaser
            ? new Color(1f, 0.45f, 0.25f)
            : new Color(0.9f, 0.3f, 0.35f));
        sr.sortingOrder = 2;

        var col = enemy.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.48f;

        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var guard = enemy.AddComponent<Enemy>();
        guard.kind = kind;
        guard.speed = speed;
        guard.patrolOffset = patrolOffset;
        guard.player = playerTransform;
        guard.deathClip = Art2D.Noise(0.15f, 30f);
    }

    void BuildHUD(Player player)
    {
        Canvas cv = MakeCanvas("HUDCanvas", 0);
        var hud = cv.gameObject.AddComponent<HUD>();
        hud.player = player;
        hud.root = cv.gameObject;

        hud.titleText = MakeText(cv.transform, "Dungeon Key Run", new Vector2(20, -20),
            new Vector2(0, 1), 34, TextAnchor.UpperLeft, Color.white);
        hud.livesText = MakeText(cv.transform, "Lives 3", new Vector2(20, -62),
            new Vector2(0, 1), 28, TextAnchor.UpperLeft, new Color(1f, 0.62f, 0.62f));
        hud.keyText = MakeText(cv.transform, "Key No", new Vector2(20, -100),
            new Vector2(0, 1), 28, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.35f));
        hud.timerText = MakeText(cv.transform, "Time 0s", new Vector2(-20, -20),
            new Vector2(1, 1), 28, TextAnchor.UpperRight, Color.white);
        hud.objectiveText = MakeText(cv.transform, "", new Vector2(0, 30),
            new Vector2(0.5f, 0), 24, TextAnchor.LowerCenter, new Color(0.85f, 0.9f, 1f));

        MakeText(cv.transform, "Move: WASD / Arrows   ESC: Pause", new Vector2(-20, 30),
            new Vector2(1, 0), 18, TextAnchor.LowerRight, new Color(1, 1, 1, 0.6f));
    }

    void BuildMenusCanvas()
    {
        Canvas cv = MakeCanvas("MenuCanvas", 10);
        var menus = cv.gameObject.AddComponent<Menus>();
        menus.clickClip = Art2D.Tone(550f, 0.07f);

        menus.mainPage = BuildMainPage(cv.transform, menus);
        menus.instructionsPage = BuildInfoPage(cv.transform, "How to Play",
            "Move: WASD or arrow keys\nPause: ESC\n\n" +
            "Goal: collect the gold key, avoid dungeon guards, then reach the blue exit door.\n" +
            "The exit only opens after the key is collected.\n\n" +
            "Design focus: one complete polished level with clear rules, feedback, and testing evidence.",
            menus.OnBackToMenu);
        menus.creditsPage = BuildInfoPage(cv.transform, "Credits",
            "Code, art, and audio are original for this Unity project.\n" +
            "Sprites are generated procedurally in Art2D.cs.\n" +
            "Sounds are synthesized at runtime in Art2D.cs.\n" +
            "No third-party art, music, fonts, or Asset Store packages are used.",
            menus.OnBackToMenu);
        menus.pausePage = BuildPausePage(cv.transform, menus);
        menus.gameOverPage = BuildEndPage(cv.transform, menus, out menus.gameOverScoreText);
    }

    GameObject BuildMainPage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "MainPage", new Color(0, 0, 0, 0.67f));

        MakeText(page.transform, "DUNGEON KEY RUN", new Vector2(0, 210),
            new Vector2(0.5f, 0.5f), 74, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.35f));
        MakeText(page.transform, "A focused Unity 2D top-down adventure vertical slice", new Vector2(0, 145),
            new Vector2(0.5f, 0.5f), 25, TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.85f));

        MakeButton(page.transform, "Start Game", new Vector2(0, 40), menus.OnStart);
        MakeButton(page.transform, "Instructions", new Vector2(0, -40), menus.OnInstructions);
        MakeButton(page.transform, "Credits", new Vector2(0, -120), menus.OnCredits);
        MakeButton(page.transform, "Quit", new Vector2(0, -200), menus.OnQuit);
        return page;
    }

    GameObject BuildInfoPage(Transform parent, string title, string body, UnityEngine.Events.UnityAction back)
    {
        GameObject page = MakePagePanel(parent, title + "Page", new Color(0, 0, 0, 0.72f));
        MakeText(page.transform, title, new Vector2(0, 230), new Vector2(0.5f, 0.5f), 58,
            TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.35f));
        var text = MakeText(page.transform, body, new Vector2(0, 0), new Vector2(0.5f, 0.5f), 25,
            TextAnchor.MiddleCenter, Color.white);
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 370);
        MakeButton(page.transform, "< Back", new Vector2(0, -250), back);
        page.SetActive(false);
        return page;
    }

    GameObject BuildPausePage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "PausePage", new Color(0, 0, 0, 0.7f));
        MakeText(page.transform, "PAUSED", new Vector2(0, 180), new Vector2(0.5f, 0.5f), 70,
            TextAnchor.MiddleCenter, Color.white);
        MakeButton(page.transform, "Resume", new Vector2(0, 40), menus.OnResume);
        MakeButton(page.transform, "Restart", new Vector2(0, -40), menus.OnRestart);
        MakeButton(page.transform, "Main Menu", new Vector2(0, -120), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    GameObject BuildEndPage(Transform parent, Menus menus, out Text messageText)
    {
        GameObject page = MakePagePanel(parent, "EndPage", new Color(0, 0, 0, 0.76f));
        messageText = MakeText(page.transform, "", new Vector2(0, 110), new Vector2(0.5f, 0.5f), 42,
            TextAnchor.MiddleCenter, Color.white);
        messageText.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 220);
        MakeButton(page.transform, "Play Again", new Vector2(0, -80), menus.OnRestart);
        MakeButton(page.transform, "Main Menu", new Vector2(0, -160), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    void BuildLabel(string content, Vector3 worldPos, Color color)
    {
        GameObject label = new GameObject("Label_" + content);
        label.transform.position = worldPos;
        var text = label.AddComponent<TextMesh>();
        text.text = content;
        text.fontSize = 42;
        text.characterSize = 0.08f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
    }

    Canvas MakeCanvas(string name, int sortOrder)
    {
        GameObject cv = new GameObject(name);
        Canvas c = cv.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrder;
        var scaler = cv.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cv.AddComponent<GraphicRaycaster>();
        return c;
    }

    GameObject MakePagePanel(Transform parent, string name, Color bg)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = bg;
        return panel;
    }

    Text MakeText(Transform parent, string content, Vector2 pos, Vector2 anchor,
        int size, TextAnchor align, Color col)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, 90);

        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = align;
        text.color = col;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    Button MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject("Btn_" + label);
        buttonObject.transform.SetParent(parent, false);
        var rt = buttonObject.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360, 70);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.36f, 0.78f, 0.95f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        var textRt = textObject.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;

        var text = textObject.AddComponent<Text>();
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
