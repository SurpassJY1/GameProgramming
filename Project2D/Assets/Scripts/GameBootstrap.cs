using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Builds the entire 2D shooter scene at Awake — no prefabs/assets required.
///
/// What it builds:
///   - Camera (orthographic) with CameraShake + AudioListener
///   - GameManager singleton
///   - Player ship + Bullet/Enemy/PowerUp prefab templates (deactivated parents)
///   - EnemySpawner + PowerUpSpawner
///   - Starfield background (cosmetic)
///   - HUD canvas (score, hi-score, lives, level, time, power, objective)
///   - Menus canvas (Main / Instructions / Credits / Pause / GameOver)
///   - EventSystem
///
/// Tag-free design: trigger callbacks use GetComponent&lt;Enemy&gt;() / Bullet /
/// PowerUp instead of CompareTag, so this project works in any default Unity
/// project without editing TagManager.asset.
public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        BuildCamera();
        BuildStarfield();
        BuildGameManager();

        // Build templates first so spawners can reference them.
        GameObject bullet  = BuildBulletTemplate();
        GameObject straight = BuildEnemyTemplate("Enemy_Straight", new Color(0.95f, 0.4f, 0.4f), 6);
        GameObject chaser   = BuildEnemyTemplate("Enemy_Chaser",   new Color(0.95f, 0.6f, 0.2f), 12);
        GameObject zigzag   = BuildEnemyTemplate("Enemy_Zigzag",   new Color(0.85f, 0.4f, 0.85f), 8);

        GameObject puRapid  = BuildPowerUpTemplate("PU_Rapid",  PowerUp.Kind.Rapid,
                                                   new Color(1f, 0.85f, 0.2f));
        GameObject puShield = BuildPowerUpTemplate("PU_Shield", PowerUp.Kind.Shield,
                                                   new Color(0.4f, 0.8f, 1f));
        GameObject puLife   = BuildPowerUpTemplate("PU_Life",   PowerUp.Kind.ExtraLife,
                                                   new Color(1f, 0.4f, 0.5f));

        GameObject player = BuildPlayer(bullet);
        BuildEnemySpawner(player.transform, straight, chaser, zigzag);
        BuildPowerUpSpawner(puRapid, puShield, puLife);

        BuildHUD(player.GetComponent<Player>());
        BuildMenusCanvas();
        EnsureEventSystem();
    }

    // ───────────────────────── Camera + background ─────────────────────────
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
        cam.transform.rotation = Quaternion.identity;
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.09f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        if (cam.GetComponent<CameraShake>() == null)
            cam.gameObject.AddComponent<CameraShake>();
        if (cam.GetComponent<AudioSource>() == null)
            cam.gameObject.AddComponent<AudioSource>().playOnAwake = false;
    }

    void BuildStarfield()
    {
        GameObject root = new GameObject("Starfield");
        for (int i = 0; i < 80; i++)
        {
            GameObject s = new GameObject("Star");
            s.transform.SetParent(root.transform);
            s.transform.position = new Vector3(
                Random.Range(-9.5f, 9.5f), Random.Range(-5.5f, 5.5f), 5f);
            float scale = Random.Range(0.04f, 0.10f);
            s.transform.localScale = Vector3.one * scale;
            var sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = Art2D.SolidCircle(Color.white * Random.Range(0.5f, 1f));
            sr.sortingOrder = -10;
        }
    }

    // ───────────────────────── GameManager ─────────────────────────
    void BuildGameManager()
    {
        if (GameManager.I != null) return;
        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    // ───────────────────────── Player ─────────────────────────
    GameObject BuildPlayer(GameObject bulletTemplate)
    {
        GameObject go = new GameObject("Player");
        // "Player" is a built-in tag — safe to set without TagManager edits.
        go.tag = "Player";
        go.transform.position = new Vector3(0, -3, 0);
        go.transform.localScale = Vector3.one * 0.7f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Triangle(new Color(0.4f, 0.95f, 0.5f));
        sr.sortingOrder = 2;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        var p = go.AddComponent<Player>();
        p.bulletPrefab = bulletTemplate;
        p.shootClip = Art2D.Tone(880f, 0.08f);
        p.hitClip = Art2D.Noise(0.3f, 14f);

        return go;
    }

    // ───────────────────────── Templates ─────────────────────────
    /// Bullet "prefab" — kept inactive in scene; spawners Instantiate copies.
    GameObject BuildBulletTemplate()
    {
        GameObject go = new GameObject("BulletTemplate");
        go.transform.localScale = Vector3.one * 0.18f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(1f, 1f, 0.6f));
        sr.sortingOrder = 3;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        go.AddComponent<Bullet>();
        // Inactive template — Player.Shoot clones it and enables the clone.
        go.SetActive(false);
        return go;
    }

    GameObject BuildEnemyTemplate(string name, Color color, int score)
    {
        GameObject go = new GameObject(name);
        go.transform.localScale = Vector3.one * 0.6f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Diamond(color);
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        var e = go.AddComponent<Enemy>();
        e.scoreReward = score;
        e.deathClip = Art2D.Noise(0.15f, 30f);
        // Inactive template — spawners clone it and enable the clone.
        go.SetActive(false);
        return go;
    }

    GameObject BuildPowerUpTemplate(string name, PowerUp.Kind kind, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.localScale = Vector3.one * 0.55f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.Square(color);
        sr.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one * 0.9f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        var p = go.AddComponent<PowerUp>();
        p.kind = kind;
        p.pickupClip = Art2D.Chime(660f, 0.4f);
        go.SetActive(false);
        return go;
    }

    void BuildEnemySpawner(Transform player, GameObject straight, GameObject chaser, GameObject zigzag)
    {
        GameObject go = new GameObject("EnemySpawner");
        var s = go.AddComponent<EnemySpawner>();
        s.straightPrefab = straight;
        s.chaserPrefab = chaser;
        s.zigzagPrefab = zigzag;
        s.player = player;
    }

    void BuildPowerUpSpawner(GameObject rapid, GameObject shield, GameObject life)
    {
        GameObject go = new GameObject("PowerUpSpawner");
        var s = go.AddComponent<PowerUpSpawner>();
        s.rapidPrefab = rapid;
        s.shieldPrefab = shield;
        s.extraLifePrefab = life;
    }

    // ───────────────────────── HUD ─────────────────────────
    void BuildHUD(Player player)
    {
        Canvas cv = MakeCanvas("HUDCanvas", 0);
        var hud = cv.gameObject.AddComponent<HUD>();
        hud.player = player;
        hud.root = cv.gameObject;

        // Top-left strip.
        hud.scoreText     = MakeText(cv.transform, "Score 0",   new Vector2(20, -20),
                                     new Vector2(0, 1), 36, TextAnchor.UpperLeft, Color.white);
        hud.highScoreText = MakeText(cv.transform, "Hi 0",      new Vector2(20, -60),
                                     new Vector2(0, 1), 24, TextAnchor.UpperLeft,
                                     new Color(1f, 0.9f, 0.5f));
        hud.livesText     = MakeText(cv.transform, "Lives 3",   new Vector2(20, -100),
                                     new Vector2(0, 1), 28, TextAnchor.UpperLeft,
                                     new Color(1f, 0.6f, 0.6f));

        // Top-right strip.
        hud.levelText  = MakeText(cv.transform, "Level 1",  new Vector2(-20, -20),
                                  new Vector2(1, 1), 36, TextAnchor.UpperRight,
                                  new Color(0.7f, 1f, 1f));
        hud.timerText  = MakeText(cv.transform, "Time 0s",  new Vector2(-20, -60),
                                  new Vector2(1, 1), 24, TextAnchor.UpperRight, Color.white);
        hud.powerText  = MakeText(cv.transform, "",         new Vector2(-20, -100),
                                  new Vector2(1, 1), 24, TextAnchor.UpperRight,
                                  new Color(1f, 0.95f, 0.6f));

        // Bottom-center objective.
        hud.objectiveText = MakeText(cv.transform, "",      new Vector2(0, 30),
                                  new Vector2(0.5f, 0), 22, TextAnchor.LowerCenter,
                                  new Color(0.85f, 0.85f, 1f));

        // ESC hint
        MakeText(cv.transform, "ESC = pause",                new Vector2(-20, 30),
                 new Vector2(1, 0), 18, TextAnchor.LowerRight,
                 new Color(1, 1, 1, 0.5f));
    }

    // ───────────────────────── Menus canvas ─────────────────────────
    void BuildMenusCanvas()
    {
        Canvas cv = MakeCanvas("MenuCanvas", 10);
        var menus = cv.gameObject.AddComponent<Menus>();
        menus.clickClip = Art2D.Tone(550f, 0.07f);

        menus.mainPage = BuildMainPage(cv.transform, menus);
        menus.instructionsPage = BuildInfoPage(cv.transform,
            "How to Play",
            "Move:  WASD or arrow keys\n" +
            "Shoot: Space or Left Mouse\n" +
            "Pause: ESC\n\n" +
            "Survive enemies, grab power-ups:\n" +
            "  Yellow  = Rapid Fire (triple shot)\n" +
            "  Blue    = Shield (absorbs one hit)\n" +
            "  Pink    = Extra Life",
            menus.OnBackToMenu);
        menus.creditsPage = BuildInfoPage(cv.transform,
            "Credits",
            "Code, art, audio: yours truly.\n" +
            "Sprites generated procedurally (Art2D.cs).\n" +
            "Audio synthesized procedurally (Art2D.cs).\n" +
            "No external assets used.\n\n" +
            "Built for the 2D Game Improvement assignment.",
            menus.OnBackToMenu);
        menus.pausePage = BuildPausePage(cv.transform, menus);
        menus.gameOverPage = BuildGameOverPage(cv.transform, menus, out menus.gameOverScoreText);
    }

    GameObject BuildMainPage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "MainPage", new Color(0, 0, 0, 0.6f));

        MakeText(page.transform, "STAR BLASTER",
            new Vector2(0, 220), new Vector2(0.5f, 0.5f), 80,
            TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.4f));
        MakeText(page.transform, "A 2D arcade survival shooter",
            new Vector2(0, 150), new Vector2(0.5f, 0.5f), 26,
            TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.85f));

        MakeButton(page.transform, "Start Game",       new Vector2(0,  40), menus.OnStart);
        MakeButton(page.transform, "Instructions",     new Vector2(0, -40), menus.OnInstructions);
        MakeButton(page.transform, "Credits",          new Vector2(0,-120), menus.OnCredits);
        MakeButton(page.transform, "Quit",             new Vector2(0,-200), menus.OnQuit);

        return page;
    }

    GameObject BuildInfoPage(Transform parent, string title, string body,
        UnityEngine.Events.UnityAction back)
    {
        GameObject page = MakePagePanel(parent, title + "Page", new Color(0, 0, 0, 0.7f));
        MakeText(page.transform, title,
            new Vector2(0, 230), new Vector2(0.5f, 0.5f), 60,
            TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.4f));
        var t = MakeText(page.transform, body,
            new Vector2(0, 0), new Vector2(0.5f, 0.5f), 26,
            TextAnchor.MiddleCenter, Color.white);
        t.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 350);
        MakeButton(page.transform, "< Back", new Vector2(0, -250), back);
        page.SetActive(false);
        return page;
    }

    GameObject BuildPausePage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "PausePage", new Color(0, 0, 0, 0.7f));
        MakeText(page.transform, "PAUSED",
            new Vector2(0, 180), new Vector2(0.5f, 0.5f), 70,
            TextAnchor.MiddleCenter, Color.white);
        MakeButton(page.transform, "Resume",       new Vector2(0,  40), menus.OnResume);
        MakeButton(page.transform, "Restart",      new Vector2(0, -40), menus.OnRestart);
        MakeButton(page.transform, "Main Menu",    new Vector2(0,-120), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    GameObject BuildGameOverPage(Transform parent, Menus menus, out Text scoreText)
    {
        GameObject page = MakePagePanel(parent, "GameOverPage", new Color(0, 0, 0, 0.75f));
        MakeText(page.transform, "GAME OVER",
            new Vector2(0, 200), new Vector2(0.5f, 0.5f), 80,
            TextAnchor.MiddleCenter, new Color(1f, 0.4f, 0.4f));
        scoreText = MakeText(page.transform, "",
            new Vector2(0, 60), new Vector2(0.5f, 0.5f), 30,
            TextAnchor.MiddleCenter, Color.white);
        scoreText.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 120);
        MakeButton(page.transform, "Play Again",   new Vector2(0, -60), menus.OnRestart);
        MakeButton(page.transform, "Main Menu",    new Vector2(0,-140), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    // ───────────────────────── UI helpers ─────────────────────────
    Canvas MakeCanvas(string name, int sortOrder)
    {
        GameObject cv = new GameObject(name);
        Canvas c = cv.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrder;
        var sc = cv.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cv.AddComponent<GraphicRaycaster>();
        return c;
    }

    GameObject MakePagePanel(Transform parent, string name, Color bg)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(parent, false);
        var rt = p.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = p.AddComponent<Image>(); img.color = bg;
        return p;
    }

    Text MakeText(Transform parent, string content, Vector2 pos, Vector2 anchor,
        int size, TextAnchor align, Color col)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, 80);
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.alignment = align; t.color = col;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    Button MakeButton(Transform parent, string label, Vector2 pos,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject b = new GameObject("Btn_" + label);
        b.transform.SetParent(parent, false);
        var rt = b.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360, 70);
        var img = b.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.85f, 0.95f);
        var btn = b.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        GameObject t = new GameObject("Text");
        t.transform.SetParent(b.transform, false);
        var trt = t.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tx = t.AddComponent<Text>();
        tx.text = label;
        tx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tx.fontSize = 30; tx.alignment = TextAnchor.MiddleCenter;
        tx.color = Color.white;
        return btn;
    }

    void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }
}
