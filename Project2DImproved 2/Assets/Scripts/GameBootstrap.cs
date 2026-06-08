using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Builds the Dungeon Key Run vertical slice at runtime from simple primitives.
public class GameBootstrap : MonoBehaviour
{
    const int WallLayer = 8;
    const string PixelCuteBasePath = "generated/pixel-cute-dungeon/selected/";
    const string PlayerSpritePath = PixelCuteBasePath + "player.png";
    const string EnemySpritePath = PixelCuteBasePath + "enemy.png";
    const string FloorSpritePath = PixelCuteBasePath + "floor_tile.png";
    const string WallSpritePath = PixelCuteBasePath + "wall_tile.png";
    const string KeySpritePath = PixelCuteBasePath + "key.png";
    const string ExitSpritePath = PixelCuteBasePath + "exit_gate.png";
    const string ProjectileSpritePath = PixelCuteBasePath + "projectile.png";
    const string UiPanelSpritePath = PixelCuteBasePath + "ui_panel.png";
    const string UiButtonSpritePath = PixelCuteBasePath + "ui_button.png";
    const string KenneyPlayerSpritePath = "thirdparty/topdown-shooter/player.png";
    const string KenneyEnemySpritePath = "thirdparty/topdown-shooter/enemy.png";
    const string KenneyFloorSpritePath = "thirdparty/topdown-shooter/floor_tile.png";
    const string KenneyWallSpritePath = "thirdparty/topdown-shooter/wall_tile.png";
    const float PixelTilePixelsPerUnit = 32f;
    const float PixelActorPixelsPerUnit = 48f;
    const float KenneyTilePixelsPerUnit = 64f;
    const int BaseEnemyCount = 3;
    const int MaxEnemyCount = 14;
    const int BaseEnemyHealth = 2;
    const int MaxEnemyHealth = 20;
    const int BaseEnemyXP = 9;
    const int MaxEnemyXP = 55;
    const float BasePatrolSpeed = 1.85f;
    const float BaseChaseSpeed = 2.75f;
    const float MaxPatrolSpeed = 3.8f;
    const float MaxChaseSpeed = 5.9f;

    GameObject currentFloorRoot;
    Transform playerTransform;
    Player playerController;

    struct WallSpec
    {
        public Vector2 position;
        public Vector2 size;

        public WallSpec(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }
    }

    struct EnemyPatrolSpec
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EnemyPatrolSpec(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    struct EnemyConfig
    {
        public EnemyKind kind;
        public string displayName;
        public int unlockFloor;
        public float healthMultiplier;
        public float patrolSpeedMultiplier;
        public float chaseSpeedMultiplier;
        public float chaseRange;
        public float xpMultiplier;
        public string spritePath;
        public float scale;
        public bool hasRangedAttack;
        public bool hasDashAttack;
        public bool explodesOnProximity;
        public bool healsAllies;
        public bool summonsAllies;
        public bool elite;

        public EnemyConfig(
            EnemyKind kind,
            string displayName,
            int unlockFloor,
            float healthMultiplier,
            float patrolSpeedMultiplier,
            float chaseSpeedMultiplier,
            float chaseRange,
            float xpMultiplier,
            string spritePath,
            float scale,
            bool hasRangedAttack = false,
            bool hasDashAttack = false,
            bool explodesOnProximity = false,
            bool healsAllies = false,
            bool summonsAllies = false,
            bool elite = false)
        {
            this.kind = kind;
            this.displayName = displayName;
            this.unlockFloor = unlockFloor;
            this.healthMultiplier = healthMultiplier;
            this.patrolSpeedMultiplier = patrolSpeedMultiplier;
            this.chaseSpeedMultiplier = chaseSpeedMultiplier;
            this.chaseRange = chaseRange;
            this.xpMultiplier = xpMultiplier;
            this.spritePath = spritePath;
            this.scale = scale;
            this.hasRangedAttack = hasRangedAttack;
            this.hasDashAttack = hasDashAttack;
            this.explodesOnProximity = explodesOnProximity;
            this.healsAllies = healsAllies;
            this.summonsAllies = summonsAllies;
            this.elite = elite;
        }
    }

    static readonly EnemyConfig[] EnemyConfigs =
    {
        new EnemyConfig(EnemyKind.SlimeScout, "Slime Scout", 1, 0.85f, 0.85f, 0.86f, 2.8f, 0.85f, null, 0.82f),
        new EnemyConfig(EnemyKind.TinyBat, "Tiny Bat", 2, 0.65f, 1.35f, 1.42f, 4.0f, 0.95f, null, 0.72f),
        new EnemyConfig(EnemyKind.ShieldGuard, "Shield Guard", 3, 1.75f, 0.7f, 0.76f, 3.0f, 1.35f, null, 0.95f),
        new EnemyConfig(EnemyKind.SparkSpitter, "Spark Spitter", 4, 1.05f, 0.78f, 0.72f, 4.7f, 1.35f, null, 0.84f, hasRangedAttack: true),
        new EnemyConfig(EnemyKind.BombSprite, "Bomb Sprite", 5, 1.0f, 0.74f, 0.95f, 3.7f, 1.25f, null, 0.88f, explodesOnProximity: true),
        new EnemyConfig(EnemyKind.FrostWisp, "Frost Wisp", 6, 1.1f, 0.95f, 0.94f, 4.4f, 1.45f, null, 0.82f, hasRangedAttack: true),
        new EnemyConfig(EnemyKind.DashImp, "Dash Imp", 7, 1.2f, 1.02f, 1.12f, 4.2f, 1.55f, null, 0.86f, hasDashAttack: true),
        new EnemyConfig(EnemyKind.HealerFairy, "Healer Fairy", 8, 0.9f, 0.92f, 0.88f, 3.5f, 1.65f, null, 0.78f, healsAllies: true),
        new EnemyConfig(EnemyKind.SummonerShade, "Summoner Shade", 9, 1.35f, 0.72f, 0.8f, 4.4f, 1.9f, null, 0.92f, summonsAllies: true),
        new EnemyConfig(EnemyKind.CrystalBrute, "Crystal Brute", 10, 2.8f, 0.72f, 1.08f, 4.0f, 2.6f, null, 1.12f, elite: true)
    };

    struct RoomVariant
    {
        public Vector3 playerStart;
        public Vector2 keyPosition;
        public Vector2 exitPosition;
        public WallSpec[] walls;
        public EnemyPatrolSpec[] enemies;

        public RoomVariant(Vector3 playerStart, Vector2 keyPosition, Vector2 exitPosition, WallSpec[] walls, EnemyPatrolSpec[] enemies)
        {
            this.playerStart = playerStart;
            this.keyPosition = keyPosition;
            this.exitPosition = exitPosition;
            this.walls = walls;
            this.enemies = enemies;
        }
    }

    void Awake()
    {
        BuildCamera();
        BuildGameManager();
        GameObject player = BuildPlayer();
        playerTransform = player.transform;
        playerController = player.GetComponent<Player>();
        if (GameManager.I != null) GameManager.I.OnFloorStarted += BuildCurrentFloor;
        BuildFloor(1);
        BuildHUD();
        BuildMenusCanvas();
        EnsureEventSystem();
    }

    void OnDestroy()
    {
        if (GameManager.I != null) GameManager.I.OnFloorStarted -= BuildCurrentFloor;
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
        cam.backgroundColor = new Color(0.055f, 0.045f, 0.075f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        if (cam.GetComponent<AudioSource>() == null) cam.gameObject.AddComponent<AudioSource>().playOnAwake = false;
        if (cam.GetComponent<CameraShake>() == null) cam.gameObject.AddComponent<CameraShake>();
        if (cam.GetComponent<SmoothCameraFollow>() == null) cam.gameObject.AddComponent<SmoothCameraFollow>();
    }

    void BuildGameManager()
    {
        if (GameManager.I != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    Sprite LoadWorldSprite(string primaryPath, float primaryPixelsPerUnit, string fallbackPath, float fallbackPixelsPerUnit)
    {
        return Art2D.FromPngFile(primaryPath, primaryPixelsPerUnit)
            ?? Art2D.FromPngFile(fallbackPath, fallbackPixelsPerUnit);
    }

    Sprite LoadPixelSprite(string path, float pixelsPerUnit = PixelActorPixelsPerUnit)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return Art2D.FromPngFile(path, pixelsPerUnit);
    }

    Sprite LoadUiSprite(string path)
    {
        return Art2D.FromPngFile(path, 100f);
    }

    void BuildCurrentFloor()
    {
        int floor = GameManager.I != null ? Mathf.Max(1, GameManager.I.currentFloor) : 1;
        BuildFloor(floor);
    }

    void BuildFloor(int floor)
    {
        if (currentFloorRoot != null)
        {
            currentFloorRoot.SetActive(false);
            Destroy(currentFloorRoot);
        }

        currentFloorRoot = new GameObject("Floor_" + floor);

        RoomVariant room = GetRoomVariant(floor);
        if (playerController != null) playerController.ResetForNewFloor(room.playerStart);
        else if (playerTransform != null) playerTransform.position = room.playerStart;

        Sprite floorSprite = LoadWorldSprite(FloorSpritePath, PixelTilePixelsPerUnit, KenneyFloorSpritePath, KenneyTilePixelsPerUnit)
            ?? Art2D.SoftRectangle(new Color(0.17f, 0.17f, 0.2f), new Color(0.08f, 0.09f, 0.12f), 100, 100);
        Sprite wallSprite = LoadWorldSprite(WallSpritePath, PixelTilePixelsPerUnit, KenneyWallSpritePath, KenneyTilePixelsPerUnit)
            ?? Art2D.Square(new Color(0.75f, 0.75f, 0.75f));
        BuildAmbientBackdrop();
        BuildTiledFloor(floorSprite);

        // Perimeter walls.
        BuildWall("North Wall", new Vector2(0f, 4.5f), new Vector2(14f, 0.5f), wallSprite);
        BuildWall("South Wall", new Vector2(0f, -4.5f), new Vector2(14f, 0.5f), wallSprite);
        BuildWall("West Wall", new Vector2(-7f, 0f), new Vector2(0.5f, 9f), wallSprite);
        BuildWall("East Wall", new Vector2(7f, 0f), new Vector2(0.5f, 9f), wallSprite);

        for (int i = 0; i < room.walls.Length; i++)
            BuildWall("Room Wall " + i, room.walls[i].position, room.walls[i].size, wallSprite);

        BuildKey(room.keyPosition);
        BuildExit(room.exitPosition);

        int enemyCount = EnemyCountForFloor(floor);
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyPatrolSpec patrol = PatrolForEnemy(room, i);
            EnemyKind kind = ChooseEnemyKindForFloor(floor, i);
            BuildEnemy(kind + "_" + floor + "_" + i, kind, playerTransform, patrol.pointA, patrol.pointB, floor);
        }
    }

    RoomVariant GetRoomVariant(int floor)
    {
        switch ((floor - 1) % 4)
        {
            case 1:
                return new RoomVariant(
                    new Vector3(-5.5f, 3.1f, 0f),
                    new Vector2(5.2f, -3.0f),
                    new Vector2(5.6f, 3.0f),
                    new[]
                    {
                        new WallSpec(new Vector2(-3.8f, 1.6f), new Vector2(2.4f, 0.5f)),
                        new WallSpec(new Vector2(-1.4f, -1.2f), new Vector2(0.5f, 4.0f)),
                        new WallSpec(new Vector2(1.6f, 1.2f), new Vector2(0.5f, 4.0f)),
                        new WallSpec(new Vector2(4.2f, -1.7f), new Vector2(2.2f, 0.5f)),
                        new WallSpec(new Vector2(0.1f, 0.1f), new Vector2(1.1f, 1.1f))
                    },
                    new[]
                    {
                        new EnemyPatrolSpec(new Vector3(-4.6f, -2.4f, 0f), new Vector3(-2.2f, -2.4f, 0f)),
                        new EnemyPatrolSpec(new Vector3(2.5f, 2.3f, 0f), new Vector3(5.1f, 2.3f, 0f))
                    });
            case 2:
                return new RoomVariant(
                    new Vector3(5.4f, -3.1f, 0f),
                    new Vector2(-5.2f, 3.0f),
                    new Vector2(-5.6f, -3.1f),
                    new[]
                    {
                        new WallSpec(new Vector2(-3.6f, -0.8f), new Vector2(0.5f, 4.8f)),
                        new WallSpec(new Vector2(-0.8f, 1.8f), new Vector2(3.0f, 0.5f)),
                        new WallSpec(new Vector2(1.8f, -1.8f), new Vector2(3.0f, 0.5f)),
                        new WallSpec(new Vector2(4.5f, 0.6f), new Vector2(0.5f, 3.2f)),
                        new WallSpec(new Vector2(0.4f, -0.1f), new Vector2(1.0f, 1.0f))
                    },
                    new[]
                    {
                        new EnemyPatrolSpec(new Vector3(3.2f, 2.9f, 0f), new Vector3(5.4f, 2.9f, 0f)),
                        new EnemyPatrolSpec(new Vector3(-4.9f, -2.9f, 0f), new Vector3(-2.0f, -2.9f, 0f))
                    });
            case 3:
                return new RoomVariant(
                    new Vector3(0f, -3.4f, 0f),
                    new Vector2(0f, 3.3f),
                    new Vector2(5.8f, 0f),
                    new[]
                    {
                        new WallSpec(new Vector2(-4.5f, 1.3f), new Vector2(2.5f, 0.5f)),
                        new WallSpec(new Vector2(-2.3f, -1.5f), new Vector2(0.5f, 2.7f)),
                        new WallSpec(new Vector2(0f, 0.3f), new Vector2(2.0f, 0.5f)),
                        new WallSpec(new Vector2(2.4f, 1.7f), new Vector2(0.5f, 3.0f)),
                        new WallSpec(new Vector2(4.7f, -2.0f), new Vector2(2.1f, 0.5f))
                    },
                    new[]
                    {
                        new EnemyPatrolSpec(new Vector3(-5.0f, -2.8f, 0f), new Vector3(-3.2f, -0.4f, 0f)),
                        new EnemyPatrolSpec(new Vector3(1.2f, 2.9f, 0f), new Vector3(4.8f, 2.9f, 0f))
                    });
            default:
                return new RoomVariant(
                    new Vector3(-5.6f, -3.2f, 0f),
                    new Vector2(5.4f, 3.1f),
                    new Vector2(5.7f, -3.2f),
                    new[]
                    {
                        new WallSpec(new Vector2(-2.2f, 1.2f), new Vector2(0.5f, 4.0f)),
                        new WallSpec(new Vector2(2.2f, -1.2f), new Vector2(0.5f, 4.0f)),
                        new WallSpec(new Vector2(3.8f, 1.9f), new Vector2(2.4f, 0.5f)),
                        new WallSpec(new Vector2(-3.8f, -1.9f), new Vector2(2.4f, 0.5f)),
                        new WallSpec(new Vector2(-0.3f, 0.4f), new Vector2(1.0f, 1.0f)),
                        new WallSpec(new Vector2(0.9f, -0.9f), new Vector2(1.0f, 1.0f)),
                        new WallSpec(new Vector2(-5.2f, 2.8f), new Vector2(1.4f, 0.5f)),
                        new WallSpec(new Vector2(4.9f, 3.0f), new Vector2(1.8f, 0.5f)),
                        new WallSpec(new Vector2(4.8f, -2.8f), new Vector2(1.8f, 0.5f)),
                        new WallSpec(new Vector2(-5.0f, -2.6f), new Vector2(1.6f, 0.5f)),
                        new WallSpec(new Vector2(-4.2f, 0.2f), new Vector2(0.5f, 1.8f)),
                        new WallSpec(new Vector2(4.1f, -0.1f), new Vector2(0.5f, 1.8f))
                    },
                    new[]
                    {
                        new EnemyPatrolSpec(new Vector3(-4f, 2f, 0f), new Vector3(-1f, 2f, 0f)),
                        new EnemyPatrolSpec(new Vector3(2f, -1f, 0f), new Vector3(5f, -1f, 0f))
                    });
        }
    }

    void BuildTiledFloor(Sprite floorSprite)
    {
        GameObject root = new GameObject("Dungeon Floor");
        root.transform.SetParent(currentFloorRoot.transform);
        for (int x = -7; x <= 6; x++)
        {
            for (int y = -5; y <= 4; y++)
            {
                GameObject tile = new GameObject("FloorTile");
                tile.transform.SetParent(root.transform);
                tile.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0f);
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = floorSprite;
                sr.sortingOrder = -5;
                float checker = ((x + y) & 1) == 0 ? 1f : 0.96f;
                sr.color = new Color(checker, checker, checker, 1f);
                tile.transform.localScale = Vector3.one;
            }
        }
    }

    void BuildAmbientBackdrop()
    {
        GameObject backdrop = new GameObject("Ambient Backdrop");
        backdrop.transform.SetParent(currentFloorRoot.transform);
        backdrop.transform.position = new Vector3(0f, 0f, 0f);
        backdrop.transform.localScale = new Vector3(15.5f, 10.5f, 1f);
        SpriteRenderer sr = backdrop.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SoftRectangle(new Color(0.13f, 0.1f, 0.18f), new Color(0.045f, 0.035f, 0.06f), 128, 96);
        sr.sortingOrder = -8;

        GameObject centerGlow = new GameObject("Center Floor Glow");
        centerGlow.transform.SetParent(currentFloorRoot.transform);
        centerGlow.transform.position = new Vector3(0f, 0f, 0f);
        centerGlow.transform.localScale = new Vector3(9.8f, 5.8f, 1f);
        SpriteRenderer glow = centerGlow.AddComponent<SpriteRenderer>();
        glow.sprite = Art2D.SolidCircle(new Color(0.58f, 0.44f, 0.76f, 0.08f), 128);
        glow.sortingOrder = -7;
    }

    void BuildWall(string name, Vector2 pos, Vector2 size, Sprite wallSprite)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(currentFloorRoot.transform);
        wall.layer = WallLayer;
        wall.transform.position = pos;
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;

        GameObject backing = new GameObject("WallBacking");
        backing.transform.SetParent(wall.transform);
        backing.transform.localPosition = Vector3.zero;
        backing.transform.localScale = new Vector3(size.x + 0.08f, size.y + 0.08f, 1f);
        SpriteRenderer backingRenderer = backing.AddComponent<SpriteRenderer>();
        backingRenderer.sprite = Art2D.SoftRectangle(new Color(0.18f, 0.13f, 0.25f), new Color(0.08f, 0.06f, 0.12f), 100, 100);
        backingRenderer.sortingOrder = -2;

        // Build wall visuals from tiled sprites so walls stay crisp and readable.
        int xTiles = Mathf.Max(1, Mathf.CeilToInt(size.x));
        int yTiles = Mathf.Max(1, Mathf.CeilToInt(size.y));
        float tileWidth = size.x / xTiles;
        float tileHeight = size.y / yTiles;
        float originX = -size.x * 0.5f + tileWidth * 0.5f;
        float originY = -size.y * 0.5f + tileHeight * 0.5f;

        for (int x = 0; x < xTiles; x++)
        {
            for (int y = 0; y < yTiles; y++)
            {
                GameObject tile = new GameObject("WallTile");
                tile.transform.SetParent(wall.transform);
                tile.transform.localPosition = new Vector3(originX + x * tileWidth, originY + y * tileHeight, 0f);
                tile.transform.localScale = new Vector3(tileWidth, tileHeight, 1f);
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = wallSprite;
                sr.sortingOrder = -1;
                sr.color = Color.white;
            }
        }
    }

    GameObject BuildPlayer()
    {
        GameObject go = new GameObject("Player");
        go.transform.position = new Vector3(-5.6f, -3.2f, 0f);
        go.transform.localScale = Vector3.one * 0.9f;
        AddShadow(go.transform, new Vector2(0f, -0.34f), new Vector3(0.95f, 0.34f, 1f), -0.02f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadWorldSprite(PlayerSpritePath, PixelActorPixelsPerUnit, KenneyPlayerSpritePath, 100f)
            ?? Art2D.SolidCircle(new Color(0.45f, 0.95f, 0.55f));
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
        PlayerCombat combat = go.AddComponent<PlayerCombat>();
        combat.fireCooldown = 0.22f;
        combat.bulletSpeed = 12f;
        combat.damage = 1;
        combat.bulletLifetime = 1.2f;
        combat.bulletPrefab = BuildPlayerBulletPrefab();
        combat.shootClip = Art2D.Tone(920f, 0.08f, 22f);

        SmoothCameraFollow follow = Camera.main != null ? Camera.main.GetComponent<SmoothCameraFollow>() : null;
        if (follow != null) follow.target = go.transform;
        return go;
    }

    GameObject BuildPlayerBulletPrefab()
    {
        GameObject bullet = new GameObject("Player Bullet Prefab");
        bullet.SetActive(false);
        bullet.transform.localScale = new Vector3(0.58f, 0.42f, 1f);

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(ProjectileSpritePath, 64f) ?? Art2D.Projectile();
        sr.sortingOrder = 4;

        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        Bullet projectile = bullet.AddComponent<Bullet>();
        projectile.wallMask = 1 << WallLayer;
        return bullet;
    }

    void BuildKey(Vector2 position)
    {
        GameObject key = new GameObject("Gold Key");
        key.transform.SetParent(currentFloorRoot.transform);
        Vector2 safePos = ResolveFreeCirclePosition(position, 0.3f);
        key.transform.position = new Vector3(safePos.x, safePos.y, 0f);
        key.transform.localScale = Vector3.one * 0.45f;
        AddGlow(key.transform, new Color(1f, 0.76f, 0.16f, 0.28f), new Vector3(2.2f, 2.2f, 1f), 0);
        SpriteRenderer sr = key.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(KeySpritePath) ?? Art2D.Key();
        sr.sortingOrder = 2;
        CircleCollider2D col = key.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.55f;
        key.AddComponent<KeyPickup>();
    }

    void BuildExit(Vector2 position)
    {
        GameObject exit = new GameObject("Exit Door");
        exit.transform.SetParent(currentFloorRoot.transform);
        Vector2 safePos = ResolveFreeBoxPosition(position, new Vector2(0.9f, 1.2f));
        exit.transform.position = new Vector3(safePos.x, safePos.y, 0f);
        exit.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
        AddGlow(exit.transform, new Color(0.12f, 0.72f, 1f, 0.22f), new Vector3(2.4f, 2.2f, 1f), 0);
        SpriteRenderer sr = exit.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(ExitSpritePath) ?? Art2D.ExitGate();
        sr.sortingOrder = 1;
        BoxCollider2D col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
        exit.AddComponent<ExitDoor>();
    }

    int EnemyCountForFloor(int floor)
    {
        int floorIndex = Mathf.Max(0, floor - 1);
        int addedEnemies = Mathf.CeilToInt(floorIndex * 0.75f);
        return Mathf.Clamp(BaseEnemyCount + addedEnemies, BaseEnemyCount, MaxEnemyCount);
    }

    EnemyPatrolSpec PatrolForEnemy(RoomVariant room, int enemyIndex)
    {
        EnemyPatrolSpec basePatrol = room.enemies[enemyIndex % room.enemies.Length];
        if (enemyIndex < room.enemies.Length) return basePatrol;

        float offsetStep = 0.45f + (enemyIndex % 3) * 0.25f;
        Vector3 offset = new Vector3(
            ((enemyIndex % 2) == 0 ? 1f : -1f) * offsetStep,
            (((enemyIndex / 2) % 2) == 0 ? 1f : -1f) * offsetStep,
            0f);
        return new EnemyPatrolSpec(basePatrol.pointA + offset, basePatrol.pointB - offset);
    }

    EnemyKind ChooseEnemyKindForFloor(int floor, int enemyIndex)
    {
        if (floor <= 1) return EnemyKind.SlimeScout;

        if (enemyIndex == 0)
        {
            for (int i = 0; i < EnemyConfigs.Length; i++)
                if (EnemyConfigs[i].unlockFloor == floor) return EnemyConfigs[i].kind;
        }

        int totalWeight = 0;
        for (int i = 0; i < EnemyConfigs.Length; i++)
        {
            if (EnemyConfigs[i].unlockFloor > floor) continue;
            totalWeight += EnemySpawnWeight(EnemyConfigs[i], floor);
        }

        if (totalWeight <= 0) return EnemyKind.SlimeScout;

        int seed = floor * 73856093 ^ enemyIndex * 19349663;
        int roll = Mathf.Abs(seed) % totalWeight;
        for (int i = 0; i < EnemyConfigs.Length; i++)
        {
            EnemyConfig config = EnemyConfigs[i];
            if (config.unlockFloor > floor) continue;

            int weight = EnemySpawnWeight(config, floor);
            if (roll < weight) return config.kind;
            roll -= weight;
        }

        return EnemyKind.SlimeScout;
    }

    int EnemySpawnWeight(EnemyConfig config, int floor)
    {
        int floorsUnlocked = Mathf.Max(0, floor - config.unlockFloor);
        int weight = config.elite ? 1 : Mathf.Max(2, 9 - floorsUnlocked);

        switch (config.kind)
        {
            case EnemyKind.SlimeScout:
                weight = floor <= 3 ? 10 : Mathf.Max(3, 8 - floor / 2);
                break;
            case EnemyKind.TinyBat:
                weight = floor <= 5 ? 7 : 5;
                break;
            case EnemyKind.ShieldGuard:
                weight = 5;
                break;
            case EnemyKind.SparkSpitter:
            case EnemyKind.FrostWisp:
            case EnemyKind.DashImp:
                weight = 4;
                break;
            case EnemyKind.HealerFairy:
            case EnemyKind.SummonerShade:
                weight = 2;
                break;
            case EnemyKind.CrystalBrute:
                weight = floor >= 10 ? 1 + Mathf.Min(2, (floor - 10) / 3) : 0;
                break;
        }

        return Mathf.Max(0, weight);
    }

    EnemyConfig GetEnemyConfig(EnemyKind kind)
    {
        for (int i = 0; i < EnemyConfigs.Length; i++)
            if (EnemyConfigs[i].kind == kind) return EnemyConfigs[i];

        return EnemyConfigs[0];
    }

    void BuildEnemy(string name, EnemyKind kind, Transform player, Vector3 a, Vector3 b, int floor)
    {
        EnemyConfig config = GetEnemyConfig(kind);
        Vector2 safeA = ResolveFreeCirclePosition(new Vector2(a.x, a.y), 0.38f);
        Vector2 safeB = ResolveFreeCirclePosition(new Vector2(b.x, b.y), 0.38f);
        safeA = ResolveAwayFromPlayer(safeA, 1.35f);
        safeB = ResolveAwayFromPlayer(safeB, 1.35f);
        a = new Vector3(safeA.x, safeA.y, 0f);
        b = new Vector3(safeB.x, safeB.y, 0f);

        GameObject enemy = new GameObject(name);
        enemy.transform.SetParent(currentFloorRoot.transform);
        enemy.transform.position = a;
        enemy.transform.localScale = Vector3.one * config.scale;
        AddShadow(enemy.transform, new Vector2(0f, -0.34f), new Vector3(0.95f, 0.34f, 1f), -0.02f);
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(config.spritePath, PixelActorPixelsPerUnit)
            ?? Art2D.EnemySprite(kind)
            ?? LoadWorldSprite(EnemySpritePath, PixelActorPixelsPerUnit, KenneyEnemySpritePath, 100f)
            ?? Art2D.Diamond(new Color(0.92f, 0.25f, 0.25f));
        sr.sortingOrder = 2;
        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        Enemy guard = enemy.AddComponent<Enemy>();
        guard.kind = kind;
        guard.displayName = config.displayName;
        guard.player = player;
        guard.pointA = a;
        guard.pointB = b;
        guard.wallMask = 1 << WallLayer;
        ApplyEnemyScaling(guard, floor, config);
        AddEnemyAbilityIfNeeded(enemy, guard, config);
    }

    void AddEnemyAbilityIfNeeded(GameObject enemy, Enemy guard, EnemyConfig config)
    {
        bool hasAbility = config.hasRangedAttack || config.hasDashAttack || config.explodesOnProximity ||
            config.healsAllies || config.summonsAllies || config.elite;
        if (!hasAbility) return;

        EnemyAbilityController ability = enemy.AddComponent<EnemyAbilityController>();
        ability.owner = guard;
        ability.kind = config.kind;
        ability.player = playerTransform;
        ability.wallMask = 1 << WallLayer;
        ability.projectileSprite = LoadPixelSprite(ProjectileSpritePath, 64f) ?? Art2D.Projectile(64, 24);
        ability.slimeSprite = Art2D.EnemySprite(EnemyKind.SlimeScout);
        ability.spawnRoot = currentFloorRoot != null ? currentFloorRoot.transform : null;
    }

    void AddShadow(Transform parent, Vector2 offset, Vector3 scale, float zOffset)
    {
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(parent, false);
        shadow.transform.localPosition = new Vector3(offset.x, offset.y, zOffset);
        shadow.transform.localScale = scale;
        SpriteRenderer sr = shadow.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(0f, 0f, 0f, 0.34f), 64);
        sr.sortingOrder = -2;
    }

    void AddGlow(Transform parent, Color color, Vector3 scale, int sortingOrder)
    {
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(parent, false);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = scale;
        SpriteRenderer sr = glow.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(color, 96);
        sr.sortingOrder = sortingOrder;
    }

    void ApplyEnemyScaling(Enemy enemy, int floor, EnemyConfig config)
    {
        int floorIndex = Mathf.Max(0, floor - 1);
        int healthGrowth = Mathf.FloorToInt(floorIndex * 1.15f) + Mathf.FloorToInt(floorIndex / 3f);
        enemy.maxHealth = Mathf.Min(MaxEnemyHealth, Mathf.Max(1, Mathf.RoundToInt((BaseEnemyHealth + healthGrowth) * config.healthMultiplier)));
        enemy.currentHealth = enemy.maxHealth;
        enemy.patrolSpeed = Mathf.Min(MaxPatrolSpeed, (BasePatrolSpeed + floorIndex * 0.12f) * config.patrolSpeedMultiplier);
        enemy.chaseSpeed = Mathf.Min(MaxChaseSpeed, (BaseChaseSpeed + floorIndex * 0.2f) * config.chaseSpeedMultiplier);
        enemy.chaseRange = config.chaseRange;
        enemy.xpReward = Mathf.Min(MaxEnemyXP, Mathf.Max(1, Mathf.RoundToInt((BaseEnemyXP + floorIndex * 4) * config.xpMultiplier)));
    }

    Vector2 ResolveAwayFromPlayer(Vector2 desired, float minDistance)
    {
        if (playerTransform == null) return desired;
        Vector2 playerPosition = playerTransform.position;
        if (Vector2.Distance(desired, playerPosition) >= minDistance) return desired;

        Vector2 away = desired - playerPosition;
        if (away.sqrMagnitude < 0.001f) away = Vector2.right;
        Vector2 candidate = playerPosition + away.normalized * minDistance;
        return ResolveFreeCirclePosition(candidate, 0.38f);
    }

    Vector2 ResolveFreeCirclePosition(Vector2 desired, float radius)
    {
        int mask = 1 << WallLayer;
        if (Physics2D.OverlapCircle(desired, radius, mask) == null) return desired;

        for (int ring = 1; ring <= 10; ring++)
        {
            float distance = ring * 0.2f;
            for (int i = 0; i < 24; i++)
            {
                float angle = (Mathf.PI * 2f / 24f) * i;
                Vector2 candidate = desired + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                if (Physics2D.OverlapCircle(candidate, radius, mask) == null) return candidate;
            }
        }

        return desired;
    }

    Vector2 ResolveFreeBoxPosition(Vector2 desired, Vector2 size)
    {
        int mask = 1 << WallLayer;
        if (Physics2D.OverlapBox(desired, size, 0f, mask) == null) return desired;

        for (int ring = 1; ring <= 10; ring++)
        {
            float distance = ring * 0.2f;
            for (int i = 0; i < 24; i++)
            {
                float angle = (Mathf.PI * 2f / 24f) * i;
                Vector2 candidate = desired + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                if (Physics2D.OverlapBox(candidate, size, 0f, mask) == null) return candidate;
            }
        }

        return desired;
    }

    void BuildHUD()
    {
        Canvas cv = MakeCanvas("HUDCanvas", 0);
        HUD hud = cv.gameObject.AddComponent<HUD>();
        hud.root = cv.gameObject;
        MakeUiImage(cv.transform, "HudPanel", new Vector2(16, -16), new Vector2(0, 1), new Vector2(545, 350), new Color(0.85f, 0.9f, 1f, 0.92f), UiPanelSpritePath);
        MakeUiImage(cv.transform, "HudAccent", new Vector2(16, -16), new Vector2(0, 1), new Vector2(5, 350), new Color(0.28f, 0.82f, 1f, 0.92f));
        MakeUiImage(cv.transform, "TimerPanel", new Vector2(-16, -16), new Vector2(1, 1), new Vector2(245, 62), new Color(0.85f, 0.9f, 1f, 0.92f), UiPanelSpritePath);
        MakeUiImage(cv.transform, "ObjectivePanel", new Vector2(0, 18), new Vector2(0.5f, 0), new Vector2(760, 52), new Color(0.85f, 0.9f, 1f, 0.86f), UiPanelSpritePath);
        hud.floorText = MakeText(cv.transform, "Floor: 1", new Vector2(20, -20), new Vector2(0, 1), 30, TextAnchor.UpperLeft, new Color(0.72f, 0.95f, 1f));
        hud.livesText = MakeText(cv.transform, "Lives: 3", new Vector2(20, -58), new Vector2(0, 1), 26, TextAnchor.UpperLeft, Color.white);
        hud.keyText = MakeText(cv.transform, "Key: Missing", new Vector2(20, -94), new Vector2(0, 1), 24, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.45f));
        hud.enemiesText = MakeText(cv.transform, "Defeated: 0", new Vector2(20, -128), new Vector2(0, 1), 22, TextAnchor.UpperLeft, new Color(1f, 0.72f, 0.62f));
        hud.levelText = MakeText(cv.transform, "Level: 1", new Vector2(20, -162), new Vector2(0, 1), 24, TextAnchor.UpperLeft, new Color(0.72f, 0.95f, 1f));
        hud.xpText = MakeText(cv.transform, "XP: 0 / 20", new Vector2(20, -196), new Vector2(0, 1), 20, TextAnchor.UpperLeft, new Color(0.82f, 0.9f, 1f));
        hud.xpBarFill = MakeHudBar(cv.transform, new Vector2(20, -226), new Vector2(280, 14), new Color(0.05f, 0.08f, 0.11f, 0.82f), new Color(0.35f, 0.85f, 1f, 0.95f));
        hud.weaponText = MakeText(cv.transform, "Weapon: Basic Shot", new Vector2(20, -256), new Vector2(0, 1), 19, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.42f));
        hud.weaponText.GetComponent<RectTransform>().sizeDelta = new Vector2(520, 100);
        hud.passiveText = MakeText(cv.transform, "Passives: None", new Vector2(20, -308), new Vector2(0, 1), 19, TextAnchor.UpperLeft, new Color(0.75f, 1f, 0.72f));
        hud.passiveText.GetComponent<RectTransform>().sizeDelta = new Vector2(520, 100);
        hud.timerText = MakeText(cv.transform, "Time: 0s", new Vector2(-20, -20), new Vector2(1, 1), 28, TextAnchor.UpperRight, Color.white);
        hud.objectiveText = MakeText(cv.transform, "", new Vector2(0, 30), new Vector2(0.5f, 0), 24, TextAnchor.LowerCenter, new Color(0.85f, 0.9f, 1f));
        MakeText(cv.transform, "ESC = pause", new Vector2(-20, 30), new Vector2(1, 0), 18, TextAnchor.LowerRight, new Color(1, 1, 1, 0.55f));
    }

    Image MakeUiImage(Transform parent, string name, Vector2 pos, Vector2 anchor, Vector2 size, Color color, string spritePath = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image image = go.AddComponent<Image>();
        image.color = color;
        if (!string.IsNullOrEmpty(spritePath)) image.sprite = LoadUiSprite(spritePath);
        return image;
    }

    Image MakeHudBar(Transform parent, Vector2 pos, Vector2 size, Color backgroundColor, Color fillColor)
    {
        GameObject background = new GameObject("XPBar");
        background.transform.SetParent(parent, false);
        RectTransform bgRt = background.AddComponent<RectTransform>();
        bgRt.anchorMin = bgRt.anchorMax = bgRt.pivot = new Vector2(0, 1);
        bgRt.anchoredPosition = pos;
        bgRt.sizeDelta = size;
        background.AddComponent<Image>().color = backgroundColor;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0f;
        return fillImage;
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
            "Code, audio, and the cute pixel visual set are generated for this module project.\nKenney CC0 sprites remain as documented fallback assets in README.",
            menus.OnBackToMenu);
        menus.pausePage = BuildPausePage(cv.transform, menus);
        menus.winPage = BuildEndPage(cv.transform, menus, "ESCAPED", new Color(0.45f, 1f, 0.65f), out menus.winText);
        menus.gameOverPage = BuildEndPage(cv.transform, menus, "CAUGHT", new Color(1f, 0.45f, 0.45f), out menus.gameOverText);
        cv.gameObject.AddComponent<UpgradeSelectionUI>().Build(cv.transform);
        cv.gameObject.AddComponent<PassiveUpgradeSelectionUI>().Build(cv.transform);
        cv.gameObject.AddComponent<TutorialPromptUI>().Build(cv.transform);
    }

    GameObject BuildMainPage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "MainPage", new Color(0.03f, 0.02f, 0.05f, 0.72f));
        MakeUiImage(page.transform, "TitleRuleTop", new Vector2(0, 292), new Vector2(0.5f, 0.5f), new Vector2(760, 4), new Color(0.28f, 0.82f, 1f, 0.68f));
        MakeUiImage(page.transform, "TitleRuleBottom", new Vector2(0, 108), new Vector2(0.5f, 0.5f), new Vector2(760, 4), new Color(1f, 0.72f, 0.18f, 0.7f));
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
        GameObject page = MakePagePanel(parent, title + "Page", new Color(0.03f, 0.02f, 0.05f, 0.76f));
        MakeUiImage(page.transform, "InfoPanel", new Vector2(0, 5), new Vector2(0.5f, 0.5f), new Vector2(980, 430), new Color(0.85f, 0.9f, 1f, 0.92f), UiPanelSpritePath);
        MakeText(page.transform, title, new Vector2(0, 220), new Vector2(0.5f, 0.5f), 56, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f));
        Text text = MakeText(page.transform, body, new Vector2(0, 20), new Vector2(0.5f, 0.5f), 28, TextAnchor.MiddleCenter, Color.white);
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 360);
        MakeButton(page.transform, "< Back", new Vector2(0, -240), back);
        page.SetActive(false);
        return page;
    }

    GameObject BuildPausePage(Transform parent, Menus menus)
    {
        GameObject page = MakePagePanel(parent, "PausePage", new Color(0.03f, 0.02f, 0.05f, 0.76f));
        MakeUiImage(page.transform, "PausePanel", new Vector2(0, 10), new Vector2(0.5f, 0.5f), new Vector2(520, 450), new Color(0.85f, 0.9f, 1f, 0.92f), UiPanelSpritePath);
        MakeText(page.transform, "PAUSED", new Vector2(0, 180), new Vector2(0.5f, 0.5f), 68, TextAnchor.MiddleCenter, Color.white);
        MakeButton(page.transform, "Resume", new Vector2(0, 40), menus.OnResume);
        MakeButton(page.transform, "Restart", new Vector2(0, -40), menus.OnRestart);
        MakeButton(page.transform, "Main Menu", new Vector2(0, -120), menus.OnReturnHome);
        page.SetActive(false);
        return page;
    }

    GameObject BuildEndPage(Transform parent, Menus menus, string title, Color titleColor, out Text resultText)
    {
        GameObject page = MakePagePanel(parent, title + "Page", new Color(0.03f, 0.02f, 0.05f, 0.8f));
        MakeUiImage(page.transform, "ResultPanel", new Vector2(0, 20), new Vector2(0.5f, 0.5f), new Vector2(980, 520), new Color(0.85f, 0.9f, 1f, 0.92f), UiPanelSpritePath);
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
        Image image = panel.AddComponent<Image>();
        image.color = bg;
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
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.92f, 0.96f, 1f, 1f);
        image.sprite = LoadUiSprite(UiButtonSpritePath);
        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.92f, 0.96f, 1f, 1f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.82f, 1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        MakeUiImage(go.transform, "BtnAccent", new Vector2(0, -31), new Vector2(0.5f, 0.5f), new Vector2(320, 3), new Color(1f, 0.8f, 0.25f, 0.72f));

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
