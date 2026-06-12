using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Builds the Dungeon Key Run scene at runtime so the submitted project does not depend on
/// manually configured prefabs. This script owns layout, spawned actors, UI, and sprite fallback.
///
/// Authorship note:
/// - Student-completed code: runtime scene construction, floor layout rules, enemy setup,
///   asset fallback choices, final integration into the Unity project, and acceptance of the boss
///   progression/design rules used in the playable build.
/// - AI-assisted support: code review suggestions, implementation guidance for asset loading and
///   boss/elite enemy plumbing, generated boss sprite support, and comment/documentation wording.
///   The student completed the integration decisions and reviewed the final submitted code.
public class GameBootstrap : MonoBehaviour
{
    const int WallLayer = 8;
    const string ClearDungeonBasePath = "generated/clear-dungeon/";
    const string BossSpriteBasePath = ClearDungeonBasePath + "bosses/";
    const string PixelCuteBasePath = "generated/pixel-cute-dungeon/selected/";
    const string PixelCuteProjectileSpritePath = PixelCuteBasePath + "projectile.png";
    const string TinyDungeonBasePath = "thirdparty/kenney-tiny-dungeon/tiles/";
    const string PlayerSpritePath = ClearDungeonBasePath + "player.png";
    const string EnemySpritePath = TinyDungeonBasePath + "tile_0084.png";
    const string FloorSpritePath = ClearDungeonBasePath + "floor_tile.png";
    const string WallSpritePath = ClearDungeonBasePath + "wall_tile.png";
    const string KeySpritePath = ClearDungeonBasePath + "key.png";
    const string ExitSpritePath = ClearDungeonBasePath + "exit_door.png";
    const string ProjectileSpritePath = TinyDungeonBasePath + "tile_0126.png";
    const string UiPanelSpritePath = "thirdparty/kenney-ui-pack/png/Extra/Default/input_rectangle.png";
    const string UiButtonSpritePath = "thirdparty/kenney-ui-pack/png/Blue/Default/button_rectangle_depth_flat.png";
    const string KenneyPlayerSpritePath = "thirdparty/topdown-shooter/player.png";
    const string KenneyEnemySpritePath = "thirdparty/topdown-shooter/enemy.png";
    const string KenneyFloorSpritePath = "thirdparty/topdown-shooter/floor_tile.png";
    const string KenneyWallSpritePath = "thirdparty/topdown-shooter/wall_tile.png";
    const string MenuClickAudioPath = "thirdparty/kenney-audio/interface/Audio/click_002.ogg";
    const string ConfirmAudioPath = "thirdparty/kenney-audio/interface/Audio/confirmation_001.ogg";
    const string ShootAudioPath = "thirdparty/kenney-audio/rpg/Audio/knifeSlice.ogg";
    const string KeyAudioPath = "thirdparty/kenney-audio/rpg/Audio/handleCoins.ogg";
    const string HitAudioPath = "thirdparty/kenney-audio/impact/Audio/impactPunch_heavy_000.ogg";
    const string BulletImpactAudioPath = "thirdparty/kenney-audio/impact/Audio/impactMetal_light_000.ogg";
    const string EnemyDefeatAudioPath = "thirdparty/kenney-audio/impact/Audio/impactGeneric_light_000.ogg";
    const string ExitAudioPath = "thirdparty/kenney-audio/rpg/Audio/doorOpen_1.ogg";
    const string MusicAudioPath = "thirdparty/incompetech/8bit-dungeon-level.mp3";
    const float ClearDungeonPixelsPerUnit = 32f;
    const float PixelTilePixelsPerUnit = 16f;
    const float PixelActorPixelsPerUnit = 16f;
    const float KenneyTilePixelsPerUnit = 64f;
    // Difficulty constants are intentionally centralized so issue-driven balance changes can be
    // reviewed without hunting through individual enemy setup calls.
    const int BaseEnemyCount = 3;
    const int MaxEnemyCount = 17;
    const int BossFloorInterval = 3;
    const int BaseEnemyHealth = 3;
    const int MaxEnemyHealth = 80;
    const int MaxBossHealth = 360;
    const int BaseEnemyXP = 8;
    const int MaxEnemyXP = 45;
    const float BasePatrolSpeed = 1.85f;
    const float BaseChaseSpeed = 2.75f;
    const float MaxPatrolSpeed = 4.4f;
    const float MaxChaseSpeed = 6.8f;

    GameObject currentFloorRoot;
    Transform playerTransform;
    Player playerController;

    struct WallSpec
    {
        public Vector2 position;
        public Vector2 size;

        // What: Store wall position and size for runtime room construction.
        // Human: Authored the room wall layouts.
        // AI: Helped package wall data into a simple struct.
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

        // What: Store two patrol endpoints for one enemy spawn route.
        // Human: Authored patrol positions for each room variant.
        // AI: Helped keep patrol data separate from enemy component setup.
        public EnemyPatrolSpec(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    // EnemyConfig keeps progression data in one place: each enemy type unlocks on a floor,
    // then contributes stats, XP value, sprite choice, and optional ability behaviour.
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

        // What: Store all design/config data for one enemy type.
        // Human: Chose enemy names, unlock floors, stats, and ability identities.
        // AI: Helped organize the data table so spawning and scaling share one source.
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

    // Student-completed design data: the unlock floors, relative difficulty, and enemy names are
    // part of the submitted game design. AI-assisted support helped organize this table so the
    // runtime spawner, boss encounters, and later elite variants can use one source of truth.
    static readonly EnemyConfig[] EnemyConfigs =
    {
        new EnemyConfig(EnemyKind.SlimeScout, "Slime Scout", 1, 0.85f, 0.85f, 0.86f, 2.8f, 0.85f, TinyDungeonBasePath + "tile_0084.png", 0.82f),
        new EnemyConfig(EnemyKind.TinyBat, "Tiny Bat", 2, 0.65f, 1.35f, 1.42f, 4.0f, 0.95f, TinyDungeonBasePath + "tile_0085.png", 0.72f),
        new EnemyConfig(EnemyKind.ShieldGuard, "Shield Guard", 3, 1.75f, 0.7f, 0.76f, 3.0f, 1.35f, TinyDungeonBasePath + "tile_0096.png", 0.95f),
        new EnemyConfig(EnemyKind.SparkSpitter, "Spark Spitter", 4, 1.05f, 0.78f, 0.72f, 4.7f, 1.35f, TinyDungeonBasePath + "tile_0097.png", 0.84f, hasRangedAttack: true),
        new EnemyConfig(EnemyKind.BombSprite, "Bomb Sprite", 5, 1.0f, 0.74f, 0.95f, 3.7f, 1.25f, TinyDungeonBasePath + "tile_0100.png", 0.88f, explodesOnProximity: true),
        new EnemyConfig(EnemyKind.FrostWisp, "Frost Wisp", 6, 1.1f, 0.95f, 0.94f, 4.4f, 1.45f, TinyDungeonBasePath + "tile_0099.png", 0.82f, hasRangedAttack: true),
        new EnemyConfig(EnemyKind.DashImp, "Dash Imp", 7, 1.2f, 1.02f, 1.12f, 4.2f, 1.55f, TinyDungeonBasePath + "tile_0086.png", 0.86f, hasDashAttack: true),
        new EnemyConfig(EnemyKind.HealerFairy, "Healer Fairy", 8, 0.9f, 0.92f, 0.88f, 3.5f, 1.65f, TinyDungeonBasePath + "tile_0087.png", 0.78f, healsAllies: true),
        new EnemyConfig(EnemyKind.SummonerShade, "Summoner Shade", 9, 1.35f, 0.72f, 0.8f, 4.4f, 1.9f, TinyDungeonBasePath + "tile_0108.png", 0.92f, summonsAllies: true),
        new EnemyConfig(EnemyKind.CrystalBrute, "Crystal Brute", 10, 2.8f, 0.72f, 1.08f, 4.0f, 2.6f, TinyDungeonBasePath + "tile_0110.png", 1.12f, elite: true),

        // Boss configs serve two roles:
        // 1. On boss floors, BuildEnemy receives bossEncounter=true, so these become large,
        //    high-health bosses with a health bar and exit lock.
        // 2. On later floors, the same configs spawn as smaller elite enemies with reduced
        //    strength. The unlock floors below are intentionally one floor after each boss debut
        //    so a short classroom/demo run can show the "previous boss returns as elite" feature.
        new EnemyConfig(EnemyKind.SlimeKing, "Slime King", 4, 2.15f, 0.7f, 0.85f, 5.6f, 2.35f, BossSpriteBasePath + "slime_king.png", 0.86f, summonsAllies: true, elite: true),
        new EnemyConfig(EnemyKind.FrostQueen, "Frost Queen", 7, 2.25f, 0.72f, 0.9f, 6.0f, 2.55f, BossSpriteBasePath + "frost_queen.png", 0.86f, hasRangedAttack: true, hasDashAttack: true, elite: true),
        new EnemyConfig(EnemyKind.ShadeOverlord, "Shade Overlord", 10, 2.45f, 0.68f, 0.86f, 6.1f, 2.75f, BossSpriteBasePath + "shade_overlord.png", 0.88f, hasRangedAttack: true, summonsAllies: true, elite: true),
        new EnemyConfig(EnemyKind.CrystalTitan, "Crystal Titan", 13, 2.75f, 0.6f, 0.8f, 6.4f, 3.0f, BossSpriteBasePath + "crystal_titan.png", 0.9f, hasRangedAttack: true, hasDashAttack: true, elite: true)
    };

    // Four room variants rotate by floor number. The run can continue indefinitely while still
    // keeping the authored route planning layout readable for a short presentation demo.
    struct RoomVariant
    {
        public Vector3 playerStart;
        public Vector2 keyPosition;
        public Vector2 exitPosition;
        public WallSpec[] walls;
        public EnemyPatrolSpec[] enemies;

        // What: Store one complete room variant: player start, key, exit, walls, and patrols.
        // Human: Authored the room layouts and objective placement.
        // AI: Helped model the layout as data instead of scene prefabs.
        public RoomVariant(Vector3 playerStart, Vector2 keyPosition, Vector2 exitPosition, WallSpec[] walls, EnemyPatrolSpec[] enemies)
        {
            this.playerStart = playerStart;
            this.keyPosition = keyPosition;
            this.exitPosition = exitPosition;
            this.walls = walls;
            this.enemies = enemies;
        }
    }

    // What: Build the whole playable scene from code when the Unity scene starts.
    // Human: Owned the decision to use runtime construction and final scene composition.
    // AI: Helped order setup steps so manager, player, floors, HUD, and menus connect correctly.
    void Awake()
    {
        BuildCamera();
        BuildGameManager();
        BuildMusicController();
        GameObject player = BuildPlayer();
        playerTransform = player.transform;
        playerController = player.GetComponent<Player>();
        if (GameManager.I != null) GameManager.I.OnFloorStarted += BuildCurrentFloor;
        BuildFloor(1);
        BuildHUD();
        BuildMenusCanvas();
        EnsureEventSystem();
    }

    // What: Remove event subscriptions when this bootstrap object is destroyed.
    // Human: Owned runtime scene lifecycle.
    // AI: Suggested cleanup so rebuilding the scene does not duplicate floor listeners.
    void OnDestroy()
    {
        if (GameManager.I != null) GameManager.I.OnFloorStarted -= BuildCurrentFloor;
    }

    // What: Create or configure the main orthographic camera and camera helper components.
    // Human: Chose camera size, background color, and top-down framing.
    // AI: Helped attach shake and smooth-follow helpers consistently.
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

    // What: Ensure the central GameManager and run event audio bridge exist.
    // Human: Designed GameManager as the run-state owner.
    // AI: Helped attach milestone audio without manual scene setup.
    void BuildGameManager()
    {
        if (GameManager.I == null) new GameObject("GameManager").AddComponent<GameManager>();
        if (GameManager.I != null && GameManager.I.GetComponent<RunEventAudio>() == null)
        {
            RunEventAudio eventAudio = GameManager.I.gameObject.AddComponent<RunEventAudio>();
            eventAudio.floorClearedClip = LoadOggClip(ConfirmAudioPath) ?? LoadOggClip(ExitAudioPath);
            eventAudio.runEndedClip = LoadOggClip(HitAudioPath);
        }
    }

    // What: Try the project-generated sprite first, then a fallback sprite path.
    // Human: Chose primary and fallback art assets.
    // AI: Helped make a reusable two-path sprite loader.
    Sprite LoadWorldSprite(string primaryPath, float primaryPixelsPerUnit, string fallbackPath, float fallbackPixelsPerUnit)
    {
        return Art2D.FromPngFile(primaryPath, primaryPixelsPerUnit)
            ?? Art2D.FromPngFile(fallbackPath, fallbackPixelsPerUnit);
    }

    // What: Load one pixel-art sprite from StreamingAssets.
    // Human: Chose sprite paths and pixels-per-unit values.
    // AI: Helped keep null/empty path handling safe.
    Sprite LoadPixelSprite(string path, float pixelsPerUnit = PixelActorPixelsPerUnit)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return Art2D.FromPngFile(path, pixelsPerUnit);
    }

    // What: Load a UI sprite using the shared UI pixels-per-unit value.
    // Human: Chose UI panel/button image assets.
    // AI: Helped wrap the loader for concise UI construction.
    Sprite LoadUiSprite(string path)
    {
        return Art2D.FromPngFile(path, 100f);
    }

    // What: Load one OGG audio clip from StreamingAssets.
    // Human: Chose which OGG clips map to game events.
    // AI: Helped provide a typed wrapper around Art2D.FromAudioFile.
    AudioClip LoadOggClip(string path)
    {
        return Art2D.FromAudioFile(path, AudioType.OGGVORBIS);
    }

    // What: Load one MP3 audio clip from StreamingAssets.
    // Human: Chose the licensed background music file.
    // AI: Helped provide a typed wrapper around Art2D.FromAudioFile.
    AudioClip LoadMp3Clip(string path)
    {
        return Art2D.FromAudioFile(path, AudioType.MPEG);
    }

    // What: Create the background music object and configure phase-based volume control.
    // Human: Chose the music asset and volume levels.
    // AI: Helped keep music control in RuntimeMusicController.
    void BuildMusicController()
    {
        AudioClip music = LoadMp3Clip(MusicAudioPath);
        if (music == null) return;

        GameObject musicObject = new GameObject("Licensed Background Music");
        AudioSource musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = 0.24f;
        musicSource.playOnAwake = false;

        RuntimeMusicController controller = musicObject.AddComponent<RuntimeMusicController>();
        controller.source = musicSource;
        controller.menuVolume = 0.16f;
        controller.playingVolume = 0.24f;
        controller.pausedVolume = 0.08f;
    }

    // What: Rebuild the room that matches GameManager.currentFloor.
    // Human: Designed floor-based progression.
    // AI: Helped connect this method to GameManager.OnFloorStarted.
    void BuildCurrentFloor()
    {
        int floor = GameManager.I != null ? Mathf.Max(1, GameManager.I.currentFloor) : 1;
        BuildFloor(floor);
    }

    // What: Destroy the old floor and generate the current room, objective objects, enemies, and boss.
    // Human: Authored room progression and enemy/boss pacing.
    // AI: Helped organize the rebuild sequence and comments.
    void BuildFloor(int floor)
    {
        // Floor rebuild flow:
        // 1. Remove the previous generated floor root.
        // 2. Select one of the authored room variants.
        // 3. Move the player to that room's start.
        // 4. Rebuild floor tiles, walls, key, exit, and floor-scaled enemies.
        // This keeps each floor self-contained and avoids stale objects carrying into the next room.
        if (currentFloorRoot != null)
        {
            currentFloorRoot.SetActive(false);
            Destroy(currentFloorRoot);
        }

        currentFloorRoot = new GameObject("Floor_" + floor);

        RoomVariant room = GetRoomVariant(floor);
        if (playerController != null) playerController.ResetForNewFloor(room.playerStart);
        else if (playerTransform != null) playerTransform.position = room.playerStart;

        Sprite floorSprite = LoadWorldSprite(FloorSpritePath, ClearDungeonPixelsPerUnit, KenneyFloorSpritePath, KenneyTilePixelsPerUnit)
            ?? Art2D.SoftRectangle(new Color(0.17f, 0.17f, 0.2f), new Color(0.08f, 0.09f, 0.12f), 100, 100);
        Sprite wallSprite = LoadWorldSprite(WallSpritePath, ClearDungeonPixelsPerUnit, KenneyWallSpritePath, KenneyTilePixelsPerUnit)
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

        // Later floors add enemies and unlock stronger enemy types, but the same key-to-exit
        // objective remains so the core rule is easy to understand. Every third floor adds a boss
        // spike with fewer supporting enemies.
        bool bossFloor = IsBossFloor(floor);
        int enemyCount = EnemyCountForFloor(floor);
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyPatrolSpec patrol = PatrolForEnemy(room, i);
            EnemyKind kind = ChooseEnemyKindForFloor(floor, i);
            BuildEnemy(kind + "_" + floor + "_" + i, kind, playerTransform, patrol.pointA, patrol.pointB, floor);
        }

        if (bossFloor)
        {
            EnemyPatrolSpec patrol = BossPatrolForRoom();
            EnemyKind bossKind = BossKindForFloor(floor);
            BuildEnemy(bossKind + "_Boss_" + floor, bossKind, playerTransform, patrol.pointA, patrol.pointB, floor, true);
        }
    }

    // What: Return the authored room layout selected for this floor.
    // Human: Authored all room variants, wall positions, and patrol paths.
    // AI: Helped rotate layouts by floor number for variety without procedural risk.
    RoomVariant GetRoomVariant(int floor)
    {
        // Authored layouts are stored as data instead of Unity prefabs so the design is visible in
        // one script. The modulo rotation gives repeated runs variety without adding procedural
        // generation risk right before the presentation.
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

    // What: Fill the room with visible floor tiles.
    // Human: Chose tile scale and checker brightness.
    // AI: Helped generate tiles in loops instead of manual scene placement.
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
                float checker = ((x + y) & 1) == 0 ? 0.96f : 0.88f;
                sr.color = new Color(checker, checker, checker + 0.02f, 1f);
                tile.transform.localScale = Vector3.one;
            }
        }
    }

    // What: Add dark backdrop and subtle center glow behind the floor.
    // Human: Chose the dungeon mood and readability treatment.
    // AI: Helped create fallback procedural backdrop sprites.
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

    // What: Build one wall collider and its tiled visual pieces.
    // Human: Authored wall positions and collision sizes.
    // AI: Helped tile the visuals so long walls remain crisp.
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
        backingRenderer.sprite = Art2D.SoftRectangle(new Color(0.08f, 0.06f, 0.12f), new Color(0.02f, 0.018f, 0.035f), 100, 100);
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
                sr.color = new Color(0.72f, 0.76f, 0.88f, 1f);
            }
        }
    }

    // What: Create the player object with movement, combat, collider, audio, and camera follow.
    // Human: Tuned player stats, controls, and feedback.
    // AI: Helped wire all runtime-created components together.
    GameObject BuildPlayer()
    {
        GameObject go = new GameObject("Player");
        go.transform.position = new Vector3(-5.6f, -3.2f, 0f);
        go.transform.localScale = Vector3.one * 1.08f;
        AddShadow(go.transform, new Vector2(0f, -0.34f), new Vector3(0.95f, 0.34f, 1f), -0.02f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadWorldSprite(PlayerSpritePath, ClearDungeonPixelsPerUnit, KenneyPlayerSpritePath, 100f)
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
        player.keyClip = LoadOggClip(KeyAudioPath) ?? Art2D.Chime(660f, 0.35f);
        player.hitClip = LoadOggClip(HitAudioPath) ?? Art2D.Noise(0.25f, 12f);
        player.winClip = LoadOggClip(ConfirmAudioPath) ?? LoadOggClip(ExitAudioPath) ?? Art2D.Chime(880f, 0.6f);
        PlayerCombat combat = go.AddComponent<PlayerCombat>();
        combat.fireCooldown = 0.22f;
        combat.bulletSpeed = 12f;
        combat.damage = 1;
        combat.bulletLifetime = 1.2f;
        combat.bulletPrefab = BuildPlayerBulletPrefab();
        combat.shootClip = LoadOggClip(ShootAudioPath) ?? Art2D.Tone(920f, 0.08f, 22f);

        SmoothCameraFollow follow = Camera.main != null ? Camera.main.GetComponent<SmoothCameraFollow>() : null;
        if (follow != null) follow.target = go.transform;
        return go;
    }

    // What: Create an inactive bullet prefab used by PlayerCombat.
    // Human: Chose bullet visuals, size, and impact sound.
    // AI: Helped build a prefab-like object entirely in code.
    GameObject BuildPlayerBulletPrefab()
    {
        GameObject bullet = new GameObject("Player Bullet Prefab");
        bullet.SetActive(false);
        bullet.transform.localScale = new Vector3(0.58f, 0.42f, 1f);

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(ProjectileSpritePath, 16f)
            ?? LoadPixelSprite(PixelCuteProjectileSpritePath, 64f)
            ?? Art2D.Projectile();
        sr.sortingOrder = 4;

        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        Bullet projectile = bullet.AddComponent<Bullet>();
        projectile.wallMask = 1 << WallLayer;
        projectile.impactClip = LoadOggClip(BulletImpactAudioPath);
        return bullet;
    }

    // What: Create the floor key pickup at a wall-safe position.
    // Human: Designed key collection as the first objective.
    // AI: Helped use safe-position helpers so keys do not spawn inside walls.
    void BuildKey(Vector2 position)
    {
        GameObject key = new GameObject("Gold Key");
        key.transform.SetParent(currentFloorRoot.transform);
        Vector2 safePos = ResolveFreeCirclePosition(position, 0.3f);
        key.transform.position = new Vector3(safePos.x, safePos.y, 0f);
        key.transform.localScale = Vector3.one * 0.45f;
        AddGlow(key.transform, new Color(1f, 0.76f, 0.16f, 0.28f), new Vector3(2.2f, 2.2f, 1f), 0);
        SpriteRenderer sr = key.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(KeySpritePath, ClearDungeonPixelsPerUnit) ?? Art2D.Key();
        sr.sortingOrder = 2;
        CircleCollider2D col = key.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.55f;
        key.AddComponent<KeyPickup>();
    }

    // What: Create the exit door at a wall-safe position.
    // Human: Designed the key-to-exit objective loop.
    // AI: Helped keep visual door state separate from GameManager exit rules.
    void BuildExit(Vector2 position)
    {
        GameObject exit = new GameObject("Exit Door");
        exit.transform.SetParent(currentFloorRoot.transform);
        Vector2 safePos = ResolveFreeBoxPosition(position, new Vector2(0.9f, 1.2f));
        exit.transform.position = new Vector3(safePos.x, safePos.y, 0f);
        exit.transform.localScale = new Vector3(1.15f, 1.45f, 1f);
        AddGlow(exit.transform, new Color(0.12f, 0.72f, 1f, 0.22f), new Vector3(2.4f, 2.2f, 1f), 0);
        SpriteRenderer sr = exit.AddComponent<SpriteRenderer>();
        sr.sprite = LoadPixelSprite(ExitSpritePath, ClearDungeonPixelsPerUnit) ?? Art2D.ExitGate();
        sr.sortingOrder = 1;
        BoxCollider2D col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
        exit.AddComponent<ExitDoor>();
    }

    // What: Calculate how many non-boss enemies spawn on a floor.
    // Human: Requested higher difficulty and tuned enemy-count pacing.
    // AI: Helped shape the late-floor pressure formula.
    int EnemyCountForFloor(int floor)
    {
        int floorIndex = Mathf.Max(0, floor - 1);
        // Enemy count ramps faster after floor 5 so early floors remain approachable while mid/late
        // floors apply the higher pressure requested by the difficulty issue.
        int lateFloorPressure = Mathf.Max(0, floor - 5);
        int addedEnemies = Mathf.CeilToInt(floorIndex * 0.95f + lateFloorPressure * 0.35f);
        int count = Mathf.Clamp(BaseEnemyCount + addedEnemies, BaseEnemyCount, MaxEnemyCount);
        return IsBossFloor(floor) ? Mathf.Max(3, count - 1) : count;
    }

    // What: Return whether this floor should include a formal boss encounter.
    // Human: Chose every third floor as the boss interval.
    // AI: Helped isolate the rule in one helper.
    bool IsBossFloor(int floor)
    {
        return floor > 0 && floor % BossFloorInterval == 0;
    }

    // What: Pick or offset an enemy patrol path for a spawn index.
    // Human: Authored base patrol routes.
    // AI: Helped offset extra enemies so repeated patrols are not identical.
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

    // What: Pick a safe central patrol route for a boss encounter.
    // Human: Wanted bosses to fight near the room center.
    // AI: Helped keep boss spawn points away from player and walls.
    EnemyPatrolSpec BossPatrolForRoom()
    {
        Vector2 center = ResolveAwayFromPlayer(ResolveFreeCirclePosition(Vector2.zero, 0.9f), 2.4f);
        Vector2 left = ResolveFreeCirclePosition(center + new Vector2(-1.2f, 0.55f), 0.9f);
        Vector2 right = ResolveFreeCirclePosition(center + new Vector2(1.2f, -0.55f), 0.9f);
        return new EnemyPatrolSpec(new Vector3(left.x, left.y, 0f), new Vector3(right.x, right.y, 0f));
    }

    // What: Choose which boss type appears on this boss floor.
    // Human: Designed the boss order.
    // AI: Helped express the repeated order with modulo arithmetic.
    EnemyKind BossKindForFloor(int floor)
    {
        // Student-completed rule: every third floor is a milestone boss fight. AI-assisted support
        // helped express the rule as a compact modulo rotation. Floor 3 starts with Slime King,
        // then each later boss floor advances one slot: 6 Frost Queen, 9 Shade Overlord,
        // 12 Crystal Titan, then loops.
        int bossIndex = Mathf.Max(0, floor / BossFloorInterval - 1) % 4;
        switch (bossIndex)
        {
            case 0: return EnemyKind.SlimeKing;
            case 1: return EnemyKind.FrostQueen;
            case 2: return EnemyKind.ShadeOverlord;
            default: return EnemyKind.CrystalTitan;
        }
    }

    // What: Return whether an enemy kind is one of the formal boss-capable kinds.
    // Human: Chose which enemy types can be bosses.
    // AI: Helped prevent elite enemies from being mistaken for real bosses.
    bool IsBossKind(EnemyKind kind)
    {
        // These kinds can appear in two modes. bossEncounter=true means real boss fight; false
        // means reduced elite enemy. Keeping the predicate explicit avoids accidentally treating
        // normal elite enemies, such as Crystal Brute, as boss-health-bar encounters.
        return kind == EnemyKind.SlimeKing ||
            kind == EnemyKind.FrostQueen ||
            kind == EnemyKind.ShadeOverlord ||
            kind == EnemyKind.CrystalTitan;
    }

    // What: Choose the enemy kind for one spawn slot on the current floor.
    // Human: Designed unlock floors and demo guarantees.
    // AI: Helped combine guaranteed showcases with deterministic weighted selection.
    EnemyKind ChooseEnemyKindForFloor(int floor, int enemyIndex)
    {
        if (floor <= 1) return EnemyKind.SlimeScout;

        // Guarantee that a newly unlocked enemy appears at least once on its first floor.
        // This makes progression visible during testing and presentation.
        if (enemyIndex == 0)
        {
            // Presentation/demo rule requested by the student: the first floor after a boss fight
            // should visibly show that boss returning as a weaker elite enemy. This guarantee is
            // deterministic so it does not depend on random weighted selection during a live demo.
            EnemyKind featuredEliteBoss;
            if (TryFeaturedEliteBossForFloor(floor, out featuredEliteBoss)) return featuredEliteBoss;

            for (int i = 0; i < EnemyConfigs.Length; i++)
                if (!IsBossKind(EnemyConfigs[i].kind) && EnemyConfigs[i].unlockFloor == floor)
                    return EnemyConfigs[i].kind;
        }

        if (enemyIndex == 1)
        {
            // The second spawn slot becomes a deterministic pressure enemy on later floors. This
            // guarantees the harder mechanics are seen even if weighted selection rolls basic foes.
            EnemyKind pressureEnemy;
            if (TryFeaturedPressureEnemyForFloor(floor, out pressureEnemy)) return pressureEnemy;
        }

        // After the guaranteed slot, use deterministic weighted selection from unlocked enemies.
        // Deterministic seeds keep a floor stable without storing extra random state.
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

    // What: Calculate weighted-spawn chance for one unlocked enemy type.
    // Human: Tuned enemy variety and late-floor pressure.
    // AI: Helped encode weights so advanced threats become more common later.
    int EnemySpawnWeight(EnemyConfig config, int floor)
    {
        int floorsUnlocked = Mathf.Max(0, floor - config.unlockFloor);
        int weight = config.elite ? 1 : Mathf.Max(2, 9 - floorsUnlocked);
        int latePressure = Mathf.Max(0, floor - 6);

        switch (config.kind)
        {
            case EnemyKind.SlimeKing:
            case EnemyKind.FrostQueen:
            case EnemyKind.ShadeOverlord:
            case EnemyKind.CrystalTitan:
                // Boss types enter the ordinary enemy pool only after their showcase boss fight.
                // The low weight makes them special, while the guaranteed floor rule above ensures
                // each one appears soon enough for assessment without requiring a long run.
                weight = floor >= config.unlockFloor ? 2 + Mathf.Min(3, (floor - config.unlockFloor) / 3) : 0;
                break;
            case EnemyKind.SlimeScout:
                weight = floor <= 3 ? 10 : Mathf.Max(2, 8 - floor / 2);
                break;
            case EnemyKind.TinyBat:
                weight = floor <= 5 ? 7 : Mathf.Max(3, 5 - latePressure / 4);
                break;
            case EnemyKind.ShieldGuard:
                weight = 5 + Mathf.Min(2, latePressure / 5);
                break;
            case EnemyKind.SparkSpitter:
            case EnemyKind.FrostWisp:
            case EnemyKind.DashImp:
                weight = 4 + Mathf.Min(4, latePressure / 2);
                break;
            case EnemyKind.HealerFairy:
            case EnemyKind.SummonerShade:
                weight = 2 + Mathf.Min(3, latePressure / 2);
                break;
            case EnemyKind.CrystalBrute:
                weight = floor >= 10 ? 2 + Mathf.Min(4, (floor - 10) / 2) : 0;
                break;
        }

        return Mathf.Max(0, weight);
    }

    // What: Guarantee an ability-heavy pressure enemy on later floors.
    // Human: Requested more challenge when late floors became too easy.
    // AI: Helped rotate through unlocked pressure enemies deterministically.
    bool TryFeaturedPressureEnemyForFloor(int floor, out EnemyKind kind)
    {
        kind = EnemyKind.SlimeScout;
        if (floor < 6) return false;

        // Rotate through ability-heavy enemies. If a candidate has not unlocked yet, keep scanning
        // until an unlocked pressure enemy is found.
        EnemyKind[] pressureCycle =
        {
            EnemyKind.SparkSpitter,
            EnemyKind.FrostWisp,
            EnemyKind.DashImp,
            EnemyKind.HealerFairy,
            EnemyKind.SummonerShade,
            EnemyKind.CrystalBrute
        };

        int start = Mathf.Abs(floor * 37) % pressureCycle.Length;
        for (int i = 0; i < pressureCycle.Length; i++)
        {
            EnemyKind candidate = pressureCycle[(start + i) % pressureCycle.Length];
            if (GetEnemyConfig(candidate).unlockFloor <= floor)
            {
                kind = candidate;
                return true;
            }
        }

        return false;
    }

    // What: Guarantee that previous bosses return as elite enemies on showcase floors.
    // Human: Designed the boss-return-as-elite feature for presentation pacing.
    // AI: Helped keep this guarantee separate from normal spawn weights.
    bool TryFeaturedEliteBossForFloor(int floor, out EnemyKind kind)
    {
        // Student-completed pacing requirement: short presentation time means the "boss becomes
        // elite" mechanic must appear quickly. These floors are exactly one floor after each boss debut:
        // floor 4 after Slime King, 7 after Frost Queen, 10 after Shade Overlord, and 13 after
        // Crystal Titan. AI-assisted support helped isolate the demo guarantee from the normal
        // weighted spawn logic so the rest of the enemy pool remains data-driven.
        kind = EnemyKind.SlimeScout;
        switch (floor)
        {
            case 4:
                kind = EnemyKind.SlimeKing;
                return true;
            case 7:
                kind = EnemyKind.FrostQueen;
                return true;
            case 10:
                kind = EnemyKind.ShadeOverlord;
                return true;
            case 13:
                kind = EnemyKind.CrystalTitan;
                return true;
            default:
                return false;
        }
    }

    // What: Look up the config row for an enemy kind.
    // Human: Chose config values and enemy identities.
    // AI: Helped centralize lookup with a safe default.
    EnemyConfig GetEnemyConfig(EnemyKind kind)
    {
        for (int i = 0; i < EnemyConfigs.Length; i++)
            if (EnemyConfigs[i].kind == kind) return EnemyConfigs[i];

        return EnemyConfigs[0];
    }

    // What: Build one enemy or boss object with visuals, physics, stats, and abilities.
    // Human: Designed enemy stats, sprites, boss/elite behaviour, and progression role.
    // AI: Helped wire runtime components and scale stats from config.
    void BuildEnemy(string name, EnemyKind kind, Transform player, Vector3 a, Vector3 b, int floor, bool bossEncounter = false)
    {
        // Enemy construction is split into data-driven setup and component setup:
        // EnemyConfig decides what this enemy is, ApplyEnemyScaling decides how hard it is on this
        // floor, and EnemyAbilityController is only added when the type needs a special behaviour.
        EnemyConfig config = GetEnemyConfig(kind);

        // Boss enemy kinds are reused after their boss floor. The bossEncounter flag is what decides
        // whether this object is the real floor boss or a later elite enemy:
        // - true: larger model, higher stats, boss health bar, and sealed exit until defeated.
        // - false: same distinctive boss sprite/abilities, but smaller and tuned as a strong minion.
        bool bossKind = IsBossKind(config.kind);
        bool boss = bossEncounter && bossKind;
        float safeRadius = boss ? 0.78f : 0.38f;
        float playerClearance = boss ? 2.4f : 1.35f;
        Vector2 safeA = ResolveFreeCirclePosition(new Vector2(a.x, a.y), safeRadius);
        Vector2 safeB = ResolveFreeCirclePosition(new Vector2(b.x, b.y), safeRadius);
        safeA = ResolveAwayFromPlayer(safeA, playerClearance);
        safeB = ResolveAwayFromPlayer(safeB, playerClearance);
        a = new Vector3(safeA.x, safeA.y, 0f);
        b = new Vector3(safeB.x, safeB.y, 0f);

        GameObject enemy = new GameObject(name);
        enemy.transform.SetParent(currentFloorRoot.transform);
        enemy.transform.position = a;
        // Formal boss encounters get an extra model scale multiplier. Elite versions keep the
        // smaller config.scale so they read as special enemies without occupying boss screen space.
        enemy.transform.localScale = Vector3.one * (boss ? config.scale * 1.6f : config.scale);
        AddShadow(enemy.transform, new Vector2(0f, boss ? -0.42f : -0.34f), boss ? new Vector3(1.45f, 0.46f, 1f) : new Vector3(0.95f, 0.34f, 1f), -0.02f);
        if (bossKind) AddGlow(enemy.transform, BossGlowColor(kind), boss ? new Vector3(1.55f, 1.55f, 1f) : new Vector3(1.12f, 1.12f, 1f), 1);
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        float spritePixelsPerUnit = bossKind ? ClearDungeonPixelsPerUnit : PixelActorPixelsPerUnit;
        sr.sprite = LoadPixelSprite(config.spritePath, spritePixelsPerUnit)
            ?? Art2D.EnemySprite(kind)
            ?? LoadWorldSprite(EnemySpritePath, PixelActorPixelsPerUnit, KenneyEnemySpritePath, 100f)
            ?? Art2D.Diamond(new Color(0.92f, 0.25f, 0.25f));
        sr.sortingOrder = 2;
        if (bossKind) sr.color = BossTint(kind);
        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = boss ? 0.68f : (bossKind ? 0.58f : 0.5f);
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
        guard.defeatClip = LoadOggClip(EnemyDefeatAudioPath);
        guard.isBoss = boss;
        ApplyEnemyScaling(guard, floor, config, boss);
        AddEnemyAbilityIfNeeded(enemy, guard, config, floor);

        // Only true boss encounters register with GameManager. Elite boss-kind enemies are normal
        // kills: they do not show the bottom boss HP bar and do not lock the exit.
        if (boss && GameManager.I != null) GameManager.I.RegisterBossSpawned(guard);
    }

    // What: Pick a tint for boss-kind enemy sprites.
    // Human: Chose boss color identities.
    // AI: Helped keep tint selection in one helper.
    Color BossTint(EnemyKind kind)
    {
        switch (kind)
        {
            case EnemyKind.SlimeKing: return new Color(0.66f, 1f, 0.48f, 1f);
            case EnemyKind.FrostQueen: return new Color(0.68f, 0.96f, 1f, 1f);
            case EnemyKind.ShadeOverlord: return new Color(0.92f, 0.58f, 1f, 1f);
            case EnemyKind.CrystalTitan: return new Color(1f, 0.58f, 0.94f, 1f);
            default: return Color.white;
        }
    }

    // What: Pick a glow color for boss-kind enemies.
    // Human: Chose boss readability and visual emphasis.
    // AI: Helped match glow colors to boss identities.
    Color BossGlowColor(EnemyKind kind)
    {
        switch (kind)
        {
            case EnemyKind.SlimeKing: return new Color(0.38f, 1f, 0.28f, 0.16f);
            case EnemyKind.FrostQueen: return new Color(0.28f, 0.86f, 1f, 0.18f);
            case EnemyKind.ShadeOverlord: return new Color(0.82f, 0.22f, 1f, 0.18f);
            case EnemyKind.CrystalTitan: return new Color(1f, 0.28f, 0.52f, 0.18f);
            default: return new Color(1f, 1f, 1f, 0.12f);
        }
    }

    // What: Attach EnemyAbilityController when an enemy kind needs special behaviour.
    // Human: Designed which enemy types have abilities.
    // AI: Helped pass the correct sprites, wall mask, player, spawn root, and difficulty scale.
    void AddEnemyAbilityIfNeeded(GameObject enemy, Enemy guard, EnemyConfig config, int floor)
    {
        bool hasAbility = config.hasRangedAttack || config.hasDashAttack || config.explodesOnProximity ||
            config.healsAllies || config.summonsAllies || config.elite;
        if (!hasAbility) return;

        // The base Enemy component handles movement and health. Special attacks live in a small
        // companion component so simple early-floor enemies stay easy to inspect.
        EnemyAbilityController ability = enemy.AddComponent<EnemyAbilityController>();
        ability.owner = guard;
        ability.kind = config.kind;
        ability.player = playerTransform;
        ability.wallMask = 1 << WallLayer;
        ability.difficultyScale = EnemyPressureForFloor(floor);
        ability.projectileSprite = LoadPixelSprite(ProjectileSpritePath, 16f)
            ?? LoadPixelSprite(PixelCuteProjectileSpritePath, 64f)
            ?? Art2D.Projectile(64, 24);
        ability.slimeSprite = Art2D.EnemySprite(EnemyKind.SlimeScout);
        ability.spawnRoot = currentFloorRoot != null ? currentFloorRoot.transform : null;
    }

    // What: Add a soft oval shadow under an actor or object.
    // Human: Chose shadows to improve readability on the floor.
    // AI: Helped implement the reusable child-object shadow helper.
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

    // What: Add a soft circular glow behind an important object.
    // Human: Chose glow colors for keys, exits, and boss-like enemies.
    // AI: Helped implement a reusable glow child object.
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

    // What: Apply floor-scaled health, speed, range, and XP values to an enemy.
    // Human: Tuned difficulty, boss multipliers, and XP rewards.
    // AI: Helped centralize the scaling formula and cap late-game values.
    void ApplyEnemyScaling(Enemy enemy, int floor, EnemyConfig config, bool bossEncounter)
    {
        int floorIndex = Mathf.Max(0, floor - 1);
        float pressure = EnemyPressureForFloor(floor);
        int lateFloor = Mathf.Max(0, floor - 5);
        // Scaling is capped, but late floors now add a clearer threat ramp so upgrades do not
        // trivialize the run after the first few rooms.
        int healthGrowth = Mathf.FloorToInt(floorIndex * 1.8f) +
            Mathf.FloorToInt(lateFloor * lateFloor * 0.28f) +
            Mathf.FloorToInt(lateFloor * 1.35f);
        int healthCap = bossEncounter ? MaxBossHealth : MaxEnemyHealth;

        // The same boss config can appear in two strengths. Formal bosses multiply the base config
        // again, while later elite versions use only the lower config multipliers from EnemyConfigs.
        // This directly supports the requested pacing: floor 4 can show the floor 3 boss as an elite
        // without making it as punishing as the real floor 3 boss fight.
        float healthMultiplier = bossEncounter ? config.healthMultiplier * 3.45f : config.healthMultiplier;
        enemy.maxHealth = Mathf.Min(healthCap, Mathf.Max(1, Mathf.RoundToInt((BaseEnemyHealth + healthGrowth) * healthMultiplier)));
        enemy.currentHealth = enemy.maxHealth;
        enemy.patrolSpeed = Mathf.Min(MaxPatrolSpeed, (BasePatrolSpeed + floorIndex * 0.14f) * config.patrolSpeedMultiplier * pressure);
        enemy.chaseSpeed = Mathf.Min(MaxChaseSpeed, (BaseChaseSpeed + floorIndex * 0.24f) * config.chaseSpeedMultiplier * pressure);
        enemy.chaseRange = Mathf.Min(7.2f, config.chaseRange + Mathf.Max(0f, pressure - 1f) * 1.4f);
        int xpCap = bossEncounter ? MaxEnemyXP * 3 : MaxEnemyXP;
        float xpMultiplier = bossEncounter ? config.xpMultiplier * 1.75f : config.xpMultiplier;
        enemy.xpReward = Mathf.Min(xpCap, Mathf.Max(1, Mathf.RoundToInt((BaseEnemyXP + floorIndex * 3) * xpMultiplier)));
    }

    // What: Convert a floor number into a shared late-game pressure multiplier.
    // Human: Requested stronger late-game challenge after enemies were too easy.
    // AI: Helped cap the scalar so endless floors remain playable.
    float EnemyPressureForFloor(int floor)
    {
        // Shared scalar for late-floor health/speed/range/ability pressure. The cap prevents endless
        // runs from becoming numerically impossible while still making higher floors feel sharper.
        return Mathf.Min(2.1f, 1f + Mathf.Max(0, floor - 4) * 0.075f);
    }

    // What: Move a desired spawn position away from the player if it is too close.
    // Human: Wanted floor starts to be fair and not spawn enemies on top of the player.
    // AI: Helped combine distance checks with wall-safe resolution.
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

    // What: Find a nearby wall-free position for a circular object.
    // Human: Required keys/enemies/bosses to avoid spawning inside walls.
    // AI: Helped implement ring-based search around the desired point.
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

    // What: Find a nearby wall-free position for a rectangular object.
    // Human: Required exit doors to avoid overlapping walls.
    // AI: Helped implement ring-based search using box overlap checks.
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

    // What: Build the full gameplay HUD canvas and wire its references.
    // Human: Chose HUD stats, layout, colors, and required boss bar.
    // AI: Helped generate UI elements and assign them to the HUD component.
    void BuildHUD()
    {
        Canvas cv = MakeCanvas("HUDCanvas", 0);
        HUD hud = cv.gameObject.AddComponent<HUD>();
        hud.root = cv.gameObject;
        MakeUiImage(cv.transform, "HudPanel", new Vector2(14, -14), new Vector2(0, 1), new Vector2(315, 244), new Color(0.04f, 0.05f, 0.08f, 0.74f), UiPanelSpritePath);
        MakeUiImage(cv.transform, "HudAccent", new Vector2(14, -14), new Vector2(0, 1), new Vector2(4, 244), new Color(0.28f, 0.82f, 1f, 0.8f));
        MakeUiImage(cv.transform, "TimerPanel", new Vector2(-14, -14), new Vector2(1, 1), new Vector2(205, 52), new Color(0.04f, 0.05f, 0.08f, 0.74f), UiPanelSpritePath);
        MakeUiImage(cv.transform, "ObjectivePanel", new Vector2(0, 18), new Vector2(0.5f, 0), new Vector2(700, 48), new Color(0.04f, 0.05f, 0.08f, 0.68f), UiPanelSpritePath);
        hud.floorText = MakeText(cv.transform, "Floor: 1", new Vector2(22, -20), new Vector2(0, 1), 24, TextAnchor.UpperLeft, new Color(0.72f, 0.95f, 1f));
        hud.livesText = MakeText(cv.transform, "Lives: 3", new Vector2(22, -52), new Vector2(0, 1), 21, TextAnchor.UpperLeft, Color.white);
        hud.keyText = MakeText(cv.transform, "Key: Missing", new Vector2(22, -82), new Vector2(0, 1), 20, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.45f));
        hud.enemiesText = MakeText(cv.transform, "Defeated: 0", new Vector2(22, -112), new Vector2(0, 1), 19, TextAnchor.UpperLeft, new Color(1f, 0.72f, 0.62f));
        hud.scoreText = MakeText(cv.transform, "Score: 250", new Vector2(22, -140), new Vector2(0, 1), 20, TextAnchor.UpperLeft, new Color(0.75f, 1f, 0.72f));
        hud.levelText = MakeText(cv.transform, "Level: 1", new Vector2(22, -168), new Vector2(0, 1), 20, TextAnchor.UpperLeft, new Color(0.72f, 0.95f, 1f));
        hud.xpText = MakeText(cv.transform, "XP: 0 / 20", new Vector2(22, -196), new Vector2(0, 1), 18, TextAnchor.UpperLeft, new Color(0.82f, 0.9f, 1f));
        hud.xpBarFill = MakeHudBar(cv.transform, new Vector2(22, -222), new Vector2(245, 12), new Color(0.05f, 0.08f, 0.11f, 0.82f), new Color(0.35f, 0.85f, 1f, 0.95f));
        hud.weaponText = MakeText(cv.transform, "Weapon: Basic Shot", new Vector2(18, 72), new Vector2(0, 0), 18, TextAnchor.LowerLeft, new Color(1f, 0.86f, 0.42f));
        hud.weaponText.GetComponent<RectTransform>().sizeDelta = new Vector2(560, 40);
        hud.passiveText = MakeText(cv.transform, "Passives: None", new Vector2(18, 36), new Vector2(0, 0), 18, TextAnchor.LowerLeft, new Color(0.75f, 1f, 0.72f));
        hud.passiveText.GetComponent<RectTransform>().sizeDelta = new Vector2(560, 40);
        hud.timerText = MakeText(cv.transform, "Time: 0s", new Vector2(-18, -18), new Vector2(1, 1), 24, TextAnchor.UpperRight, Color.white);
        hud.objectiveText = MakeText(cv.transform, "", new Vector2(0, 30), new Vector2(0.5f, 0), 24, TextAnchor.LowerCenter, new Color(0.85f, 0.9f, 1f));
        BuildBossHealthBar(cv.transform, hud);
        MakeText(cv.transform, "ESC = pause", new Vector2(-20, 30), new Vector2(1, 0), 18, TextAnchor.LowerRight, new Color(1, 1, 1, 0.55f));
    }

    // What: Build the bottom boss health bar used during formal boss fights.
    // Human: Required boss health visibility during boss encounters.
    // AI: Helped keep boss-bar references grouped on HUD.
    void BuildBossHealthBar(Transform parent, HUD hud)
    {
        GameObject root = new GameObject("BossHealthBar");
        root.transform.SetParent(parent, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 88f);
        rt.sizeDelta = new Vector2(620f, 52f);

        Image panel = root.AddComponent<Image>();
        panel.sprite = Art2D.FromPngFile(UiPanelSpritePath, 100f, FilterMode.Bilinear);
        panel.color = new Color(0.05f, 0.025f, 0.06f, 0.86f);

        hud.bossBarText = MakeText(root.transform, "Dungeon Boss", new Vector2(0, 13), new Vector2(0.5f, 0.5f), 20, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.96f));
        hud.bossBarText.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 24f);
        hud.bossBarFill = MakeHudBar(root.transform, new Vector2(20, -28), new Vector2(580, 16), new Color(0.12f, 0.025f, 0.04f, 0.95f), new Color(0.95f, 0.16f, 0.34f, 0.98f));
        hud.bossBarRoot = root;
        root.SetActive(false);
    }

    // What: Create a positioned UI Image with optional sprite.
    // Human: Chose UI panel/accent placement.
    // AI: Helped factor repeated runtime Image setup into one helper.
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

    // What: Create a filled horizontal UI bar and return its fill Image.
    // Human: Chose XP and boss health bars as readable progress indicators.
    // AI: Helped configure Unity's filled Image mode in code.
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

    // What: Build the menu canvas, menu pages, upgrade pages, and tutorial prompt.
    // Human: Chose page content, navigation, and upgrade UI flow.
    // AI: Helped assemble all generated UI under one canvas.
    void BuildMenusCanvas()
    {
        Canvas cv = MakeCanvas("MenuCanvas", 10);
        Menus menus = cv.gameObject.AddComponent<Menus>();
        menus.clickClip = LoadOggClip(MenuClickAudioPath) ?? Art2D.Tone(550f, 0.07f);
        menus.mainPage = BuildMainPage(cv.transform, menus);
        menus.instructionsPage = BuildInfoPage(cv.transform, "How to Play",
            "Move: WASD or arrow keys\nPause: ESC\n\nGoal: collect the gold key, then reach the blue exit door.\nAvoid red guards. They patrol and chase if you get too close.",
            menus.OnBackToMenu);
        menus.creditsPage = BuildInfoPage(cv.transform, "Credits",
            "Code and integration are original module work.\nArt: project-generated readable player/floor/wall/key/door sprites, plus Kenney Tiny Dungeon, Kenney UI Pack, and Kenney Topdown Shooter fallback assets, all documented in CREDITS.md.\nSFX: Kenney Interface, RPG, and Impact Sounds, all CC0.\nMusic: \"8bit Dungeon Level\" Kevin MacLeod (incompetech.com), CC BY 4.0.\nFull source and license details are documented in CREDITS.md.",
            menus.OnBackToMenu);
        menus.pausePage = BuildPausePage(cv.transform, menus);
        menus.winPage = BuildEndPage(cv.transform, menus, "ESCAPED", new Color(0.45f, 1f, 0.65f), out menus.winText);
        menus.gameOverPage = BuildEndPage(cv.transform, menus, "CAUGHT", new Color(1f, 0.45f, 0.45f), out menus.gameOverText);
        cv.gameObject.AddComponent<UpgradeSelectionUI>().Build(cv.transform);
        cv.gameObject.AddComponent<PassiveUpgradeSelectionUI>().Build(cv.transform);
        cv.gameObject.AddComponent<TutorialPromptUI>().Build(cv.transform);
    }

    // What: Build the title page with start, instructions, credits, and quit buttons.
    // Human: Wrote title/subtitle and chose menu actions.
    // AI: Helped create the page from reusable UI helpers.
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

    // What: Build a generic information page with title, body, and back button.
    // Human: Wrote instructions and credits text.
    // AI: Helped reuse one page builder for both info pages.
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

    // What: Build the pause menu page.
    // Human: Chose pause menu options.
    // AI: Helped wire buttons to Menus callbacks.
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

    // What: Build a win or game-over page and return its result Text reference.
    // Human: Chose end-page titles and retry/home actions.
    // AI: Helped use one builder for both endings.
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

    // What: Create a screen-space Canvas with a scaler and raycaster.
    // Human: Chose runtime UI instead of authored canvas prefabs.
    // AI: Helped configure scaling for 1920x1080 reference resolution.
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

    // What: Create a full-screen page root with a colored background Image.
    // Human: Chose overlay colors for menus and end screens.
    // AI: Helped factor repeated page setup into one helper.
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

    // What: Create a runtime Text object with common font, anchor, size, and color setup.
    // Human: Wrote all visible UI copy and chose text styling.
    // AI: Helped centralize Text setup so UI code is easier to read.
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

    // What: Create a runtime Button with background, accent line, label, and click action.
    // Human: Chose button labels, positions, and navigation flow.
    // AI: Helped configure Unity Button colors and child label setup in code.
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

    // What: Ensure Unity's UI event system exists so generated buttons can receive clicks.
    // Human: Needed menus and upgrade cards to be clickable in an empty generated scene.
    // AI: Helped add EventSystem and StandaloneInputModule when missing.
    void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }
}

/// Runtime music volume controller. The AudioSource is built by GameBootstrap, then this component
/// adjusts volume based on GameManager's current phase.
///
/// Authorship note:
/// - Student-owned implementation: music choice, menu/gameplay/pause volume targets, and final
///   integration into the generated scene.
/// - AI-assisted support: event-driven organization suggestions and comments explaining why music
///   follows GameManager phases instead of each UI page setting volume independently.
public class RuntimeMusicController : MonoBehaviour
{
    public AudioSource source;
    public float menuVolume = 0.16f;
    public float playingVolume = 0.24f;
    public float pausedVolume = 0.08f;

    // What: Subscribe music volume control to GameManager and start the music loop.
    // Human: Chose background music and phase volumes.
    // AI: Helped keep music continuous across phase changes.
    void Start()
    {
        // The clip starts in menus and continues across phase changes. Only volume changes, which
        // avoids restarting the loop each time the player pauses or levels up.
        if (GameManager.I != null) GameManager.I.OnStateChanged += SyncToPhase;
        if (source != null && source.clip != null && !source.isPlaying) source.Play();
        SyncToPhase();
    }

    // What: Unsubscribe the music controller from GameManager events.
    // Human: Owned runtime object lifecycle.
    // AI: Suggested cleanup to avoid duplicate callbacks.
    void OnDestroy()
    {
        if (GameManager.I != null) GameManager.I.OnStateChanged -= SyncToPhase;
    }

    // What: Adjust music volume based on menu, playing, paused, or upgrade phase.
    // Human: Tuned the volume levels for gameplay and UI reading.
    // AI: Helped implement phase-based volume ducking.
    void SyncToPhase()
    {
        if (source == null || GameManager.I == null) return;

        // Playing is loudest; paused/upgrade phases duck the track so UI feedback is easier to hear.
        switch (GameManager.I.phase)
        {
            case GameManager.Phase.Playing:
                source.volume = playingVolume;
                if (!source.isPlaying) source.Play();
                break;
            case GameManager.Phase.Paused:
            case GameManager.Phase.LevelUp:
            case GameManager.Phase.PassiveUpgrade:
                source.volume = pausedVolume;
                break;
            case GameManager.Phase.Menu:
                source.volume = menuVolume;
                if (!source.isPlaying) source.Play();
                break;
            default:
                source.volume = menuVolume;
                break;
        }
    }
}

/// Small event-audio bridge for run milestones such as clearing a floor or ending the run.
///
/// Authorship note:
/// - Student-owned implementation: milestone sound choices and final volume tuning.
/// - AI-assisted support: event subscription review and comments explaining why this audio is kept
///   separate from menu button clicks and weapon sounds.
public class RunEventAudio : MonoBehaviour
{
    public AudioClip floorClearedClip;
    public AudioClip runEndedClip;

    AudioSource source;

    // What: Create the AudioSource used for run milestone sounds.
    // Human: Chose milestone sound behaviour.
    // AI: Helped keep this audio source separate from menu/music sources.
    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
    }

    // What: Subscribe run milestone audio to GameManager events.
    // Human: Chose floor-clear and run-end sounds.
    // AI: Helped use events so gameplay code does not play these sounds directly.
    void Start()
    {
        if (GameManager.I == null) return;

        // Subscribe to high-level run events so gameplay scripts do not need direct audio source
        // references for these global sounds.
        GameManager.I.OnFloorCleared += PlayFloorCleared;
        GameManager.I.OnRunEnded += PlayRunEnded;
    }

    // What: Remove run milestone audio event subscriptions.
    // Human: Owned generated object lifecycle.
    // AI: Suggested cleanup for safe scene rebuilds.
    void OnDestroy()
    {
        if (GameManager.I == null) return;

        GameManager.I.OnFloorCleared -= PlayFloorCleared;
        GameManager.I.OnRunEnded -= PlayRunEnded;
    }

    // What: Play the configured floor-clear sound.
    // Human: Chose the floor-clear feedback moment.
    // AI: Helped wrap playback in a named method for event subscription.
    void PlayFloorCleared()
    {
        Play(floorClearedClip, 0.7f);
    }

    // What: Play the configured run-ended sound.
    // Human: Chose the run-end feedback moment.
    // AI: Helped wrap playback in a named method for event subscription.
    void PlayRunEnded()
    {
        Play(runEndedClip, 0.55f);
    }

    // What: Play one audio clip if both clip and source are available.
    // Human: Tuned event sound volumes.
    // AI: Helped keep playback null-safe.
    void Play(AudioClip clip, float volume)
    {
        if (clip != null && source != null) source.PlayOneShot(clip, volume);
    }
}
