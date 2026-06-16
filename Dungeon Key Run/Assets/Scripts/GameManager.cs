using System;
using UnityEngine;

/// Central run state for Dungeon Key Run.
/// Other scripts report gameplay events here; this class decides phase changes, progression,
/// upgrade application, and HUD/menu notifications.
///
/// Authorship note:
/// - Student-completed code: final game flow, run state, floor progression, XP values,
///   upgrade rules, boss-floor requirement that the exit stays sealed until the boss is defeated,
///   and integration with player/UI scripts.
/// - AI-assisted support: code review suggestions, boss state-tracking guidance, HUD health-bar data
///   exposure guidance, and comment/documentation wording. The student completed the final logic
///   review and accepted it for the submitted project.
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    const string TutorialSuccessfulRunsKey = "DungeonKeyRun_TutorialSuccessfulRuns";
    const string BestScoreKey = "DungeonKeyRun_BestScore";
    const string BestFloorKey = "DungeonKeyRun_BestFloor";
    const string BestEnemiesKey = "DungeonKeyRun_BestEnemies";
    const string BestTimeKey = "DungeonKeyRun_BestTime";
    const string BestLevelKey = "DungeonKeyRun_BestLevel";
    const string LeaderboardScoreKey = "DungeonKeyRun_LeaderboardScore_";
    const string LeaderboardFloorKey = "DungeonKeyRun_LeaderboardFloor_";
    const string LeaderboardEnemiesKey = "DungeonKeyRun_LeaderboardEnemies_";
    const string LeaderboardTimeKey = "DungeonKeyRun_LeaderboardTime_";
    const string LeaderboardLevelKey = "DungeonKeyRun_LeaderboardLevel_";
    const int LeaderboardSize = 5;
    const int BossFloorInterval = 3;

    public enum Phase { Menu, Playing, Paused, LevelUp, PassiveUpgrade, Won, GameOver }
    public Phase phase = Phase.Menu;

    [Header("Tuning")]
    public int startLives = 3;
    public int startingLevel = 1;
    public int startingXPToNextLevel = 18;
    public float xpGrowthMultiplier = 1.36f;
    public int xpGrowthFlatBonus = 8;

    public int lives;
    public bool hasKey;
    public float elapsed;
    public string objective = "Find the gold key.";
    public int currentFloor;
    public int playerLevel;
    public int currentXP;
    public int xpToNextLevel;
    public int enemiesDefeated;
    public int floorsCleared;
    public int passiveUpgradesChosen;
    public int extraProjectileLevel;
    public int rapidFireLevel;
    public int damageUpLevel;
    public int piercingShotLevel;
    public int burnShotLevel;
    public int slowShotLevel;
    public int explosiveShotLevel;
    public int maxLivesUpLevel;
    public int moveSpeedUpLevel;
    public int fireCooldownBonusLevel;
    public int xpBonusLevel;
    public int finalRunScore;
    public bool lastRunWasNewRecord;
    public Vector3 keyObjectivePosition;
    public Vector3 exitObjectivePosition;
    public bool objectivePositionsReady;

    // Boss state is intentionally kept in GameManager instead of HUD. The gameplay rule is that a
    // formal boss encounter seals the exit, while the HUD only visualizes the same state. Elite
    // versions of past bosses never register here, so they behave like normal enemies.
    public bool bossAliveThisFloor;
    public Enemy activeBoss;

    int baseStartLives;
    bool countedTutorialSuccessThisRun;
    PlayerCombat playerCombat;
    Player player;

    public event Action OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnRunEnded;
    public event Action OnLevelUpAvailable;
    public event Action OnPassiveUpgradeAvailable;
    public event Action OnFloorStarted;
    public event Action OnFloorCleared;

    // What: Establish the singleton instance and remember the original life count.
    // Human: Chose a single central run-state manager.
    // AI: Helped keep duplicate GameManager objects from surviving.
    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        baseStartLives = startLives;
    }

    // What: Tick run time and handle pause input while gameplay is active.
    // Human: Chose Escape as pause and wanted elapsed time in HUD/end summary.
    // AI: Helped gate update work by phase.
    void Update()
    {
        if (phase != Phase.Playing) return;

        elapsed += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Escape)) Pause();
        OnStateChanged?.Invoke();
    }

    // What: Start or restart a full run from floor 1 with clean run-scoped state.
    // Human: Designed the run reset rules and starting values.
    // AI: Helped order events so listeners reset before the first floor rebuild.
    public void StartGame()
    {
        // A run reset is stronger than a floor reset: it clears all run-scoped upgrades, XP,
        // floor number, elapsed time, and key state before rebuilding Floor 1.
        hasKey = false;
        elapsed = 0f;
        countedTutorialSuccessThisRun = false;
        ResetRunProgress();
        lives = startLives;
        objective = FloorObjective();
        phase = Phase.Playing;
        Time.timeScale = 1f;
        // Event order matters: listeners reset run-scoped state before the first floor is rebuilt.
        OnGameStarted?.Invoke();
        OnFloorStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    // What: Record that the current floor key has been collected.
    // Human: Chose key collection as the main floor objective.
    // AI: Helped keep objective text and state together in GameManager.
    public void CollectKey()
    {
        if (phase != Phase.Playing || hasKey) return;

        hasKey = true;
        objective = "Floor " + currentFloor + ": Key collected. Enter the exit to choose a passive upgrade.";
        OnStateChanged?.Invoke();
    }

    // What: Validate exit rules and either explain why blocked or begin passive upgrade selection.
    // Human: Designed key-first and boss-defeat-before-exit rules.
    // AI: Helped centralize exit feedback so Player and ExitDoor stay simple.
    public void TryExit()
    {
        if (phase != Phase.Playing) return;

        if (!hasKey)
        {
            objective = "Floor " + currentFloor + ": The exit is locked. Collect the floor key first.";
            OnStateChanged?.Invoke();
            return;
        }

        if (bossAliveThisFloor)
        {
            // Student-completed rule: key collection is not enough on boss floors; the boss must
            // also be defeated. AI-assisted support helped wire this flag to Enemy/GameBootstrap
            // when a formal boss encounter starts or ends.
            objective = "Floor " + currentFloor + ": The exit is sealed. Defeat the boss first.";
            OnStateChanged?.Invoke();
            return;
        }

        // Clearing a floor pauses the action and lets the player choose a passive upgrade before
        // the next floor is generated.
        BeginPassiveUpgrade();
    }

    // What: Remove one life, update objective text, and end the run if lives reach zero.
    // Human: Tuned lives and game-over behaviour.
    // AI: Helped keep all life-loss state changes in one method.
    public void PlayerHit()
    {
        if (phase != Phase.Playing) return;

        lives = Mathf.Max(0, lives - 1);
        objective = lives > 0
            ? "Floor " + currentFloor + ": Careful. Survive, find the key, and keep going."
            : "Run ended on floor " + currentFloor + ".";

        if (lives == 0)
        {
            phase = Phase.GameOver;
            Time.timeScale = 0f;
            RecordRunResult();
            OnRunEnded?.Invoke();
        }

        OnStateChanged?.Invoke();
    }

    // What: Count a defeated enemy and award XP after passive XP bonuses.
    // Human: Chose enemy XP rewards and progression pacing.
    // AI: Helped apply XP bonus at reward time.
    public void RegisterEnemyDefeated(int xpReward)
    {
        if (phase != Phase.Playing) return;

        // XP bonus is applied at reward time so enemies can keep simple per-type XP values.
        enemiesDefeated++;
        AddXP(Mathf.RoundToInt(xpReward * XPBonusMultiplier()));
    }

    // What: Mark a true boss encounter as alive and seal the exit.
    // Human: Designed boss floors and boss health bar requirement.
    // AI: Helped distinguish true bosses from elite boss-kind enemies.
    public void RegisterBossSpawned(Enemy boss)
    {
        // Called only for true boss encounters, not for later elite versions of the same boss types.
        // This keeps the bottom boss HP bar and exit lock tied to boss floors only.
        bossAliveThisFloor = true;
        activeBoss = boss;
        objective = "Floor " + currentFloor + ": Boss encounter. Get the key and defeat the boss.";
        OnStateChanged?.Invoke();
    }

    // What: Clear boss state after a true boss is defeated.
    // Human: Chose boss defeat as the condition that unseals boss-floor exits.
    // AI: Helped update objective text based on whether the key is already collected.
    public void RegisterBossDefeated()
    {
        if (!bossAliveThisFloor) return;

        // Clearing activeBoss hides the HUD boss bar on the next repaint. The normal enemy defeat
        // path still awards XP separately, so boss defeat stays compatible with level-up rewards.
        bossAliveThisFloor = false;
        activeBoss = null;
        objective = hasKey
            ? "Floor " + currentFloor + ": Boss defeated. Enter the exit."
            : "Floor " + currentFloor + ": Boss defeated. Collect the key.";
        OnStateChanged?.Invoke();
    }

    public void RegisterFloorObjectivePositions(Vector3 keyPosition, Vector3 exitPosition)
    {
        keyObjectivePosition = keyPosition;
        exitObjectivePosition = exitPosition;
        objectivePositionsReady = true;
    }

    public bool TryGetCurrentObjectivePosition(out Vector3 target)
    {
        target = Vector3.zero;
        if (!objectivePositionsReady) return false;

        if (!hasKey)
        {
            target = keyObjectivePosition;
            return true;
        }

        if (bossAliveThisFloor && activeBoss != null && !activeBoss.IsDead)
        {
            target = activeBoss.transform.position;
            return true;
        }

        target = exitObjectivePosition;
        return true;
    }

    // What: Add XP and pause for a weapon upgrade if the next threshold is reached.
    // Human: Designed mid-floor weapon level-ups.
    // AI: Helped support chained level-ups safely after upgrade selection.
    public void AddXP(int amount)
    {
        if (phase != Phase.Playing || amount <= 0) return;

        currentXP += amount;
        // One level-up is processed at a time. If currentXP still exceeds the new threshold after
        // choosing an upgrade, ChooseUpgrade immediately opens another level-up screen.
        // Level-ups interrupt gameplay so the upgrade UI can be read and selected safely.
        if (currentXP >= xpToNextLevel) BeginLevelUp();
        else OnStateChanged?.Invoke();
    }

    // What: Apply the selected weapon upgrade and resume gameplay or open another level-up.
    // Human: Chose one weapon card per level-up.
    // AI: Helped handle leftover XP that can trigger another level immediately.
    public void ChooseUpgrade(WeaponUpgradeKind upgrade)
    {
        if (phase != Phase.LevelUp) return;

        RegisterWeaponUpgrade(upgrade);
        ApplyWeaponUpgrade(upgrade);
        objective = "Weapon upgrade chosen: " + UpgradeDisplayName(upgrade) + ". Keep pushing through floor " + currentFloor + ".";

        if (currentXP >= xpToNextLevel)
        {
            BeginLevelUp();
            return;
        }

        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke();
    }

    // What: Build a short HUD/end-screen summary of selected weapon upgrades.
    // Human: Chose summary labels and which upgrades to show.
    // AI: Helped use a reusable AppendUpgradeSummary helper.
    public string GetWeaponBuildSummary()
    {
        string summary = AppendUpgradeSummary("", "Multi", extraProjectileLevel);
        summary = AppendUpgradeSummary(summary, "Rapid", rapidFireLevel);
        summary = AppendUpgradeSummary(summary, "Damage", damageUpLevel);
        summary = AppendUpgradeSummary(summary, "Pierce", piercingShotLevel);
        summary = AppendUpgradeSummary(summary, "Burn", burnShotLevel);
        summary = AppendUpgradeSummary(summary, "Slow", slowShotLevel);
        summary = AppendUpgradeSummary(summary, "Blast", explosiveShotLevel);
        return string.IsNullOrEmpty(summary) ? "Basic Shot" : summary;
    }

    // What: Apply the selected passive upgrade and advance to the next floor.
    // Human: Designed passive rewards as between-floor choices.
    // AI: Helped sequence register/apply/start-next-floor consistently.
    public void ChoosePassiveUpgrade(PassiveUpgradeKind upgrade)
    {
        if (phase != Phase.PassiveUpgrade) return;

        passiveUpgradesChosen++;
        RegisterPassiveUpgrade(upgrade);
        ApplyPassiveUpgrade(upgrade);
        objective = "Passive upgrade chosen: " + PassiveUpgradeDisplayName(upgrade) + ".";
        StartNextFloor();
    }

    // What: Build a short HUD/end-screen summary of selected passive upgrades.
    // Human: Chose passive summary labels.
    // AI: Helped reuse the summary helper.
    public string GetPassiveBuildSummary()
    {
        string summary = AppendUpgradeSummary("", "Life", maxLivesUpLevel);
        summary = AppendUpgradeSummary(summary, "Move", moveSpeedUpLevel);
        summary = AppendUpgradeSummary(summary, "Fire", fireCooldownBonusLevel);
        summary = AppendUpgradeSummary(summary, "XP", xpBonusLevel);
        return string.IsNullOrEmpty(summary) ? "None" : summary;
    }

    // What: Combine run progress into one score for endless-mode record chasing.
    // Human: Chose score as the main long-term goal for infinite runs.
    // AI: Helped weight floor progress above enemy farming.
    public int CurrentScore
    {
        get
        {
            int survivalSeconds = Mathf.FloorToInt(elapsed);
            return Mathf.Max(0, floorsCleared * 10000 +
                enemiesDefeated * 100 +
                playerLevel * 250 +
                survivalSeconds);
        }
    }

    public int BestScore { get { return PlayerPrefs.GetInt(BestScoreKey, 0); } }
    public int BestFloor { get { return PlayerPrefs.GetInt(BestFloorKey, 0); } }

    public string GetBestRecordSummary()
    {
        int bestScore = BestScore;
        if (bestScore <= 0) return "Best Score: none yet";

        return "Best Score: " + bestScore +
            "   Best Floor: " + BestFloor +
            "   Time: " + PlayerPrefs.GetInt(BestTimeKey, 0) + "s";
    }

    public string GetLeaderboardSummary()
    {
        string summary = "";
        for (int i = 0; i < LeaderboardSize; i++)
        {
            int score = PlayerPrefs.GetInt(LeaderboardScoreKey + i, 0);
            if (score <= 0) continue;

            string line = (i + 1) + ". Score " + score +
                " | Floor " + PlayerPrefs.GetInt(LeaderboardFloorKey + i, 0) +
                " | Level " + PlayerPrefs.GetInt(LeaderboardLevelKey + i, 0) +
                " | " + PlayerPrefs.GetInt(LeaderboardTimeKey + i, 0) + "s";
            summary = string.IsNullOrEmpty(summary) ? line : summary + "\n" + line;
        }

        return string.IsNullOrEmpty(summary) ? "No recorded runs yet." : summary;
    }

    // What: Return the player-facing passive upgrade name for UI cards.
    // Human: Wrote final passive names.
    // AI: Helped expose a public wrapper for UI code.
    public string GetPassiveUpgradeDisplayName(PassiveUpgradeKind upgrade)
    {
        return PassiveUpgradeDisplayName(upgrade);
    }

    // What: Return the passive upgrade description shown on cards.
    // Human: Wrote final player-facing descriptions.
    // AI: Helped keep descriptions centralized.
    public string GetPassiveUpgradeDescription(PassiveUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case PassiveUpgradeKind.MaxLivesUp:
                return "Increase max lives and heal by 1.";
            case PassiveUpgradeKind.MoveSpeedUp:
                return "Move faster for the rest of this run.";
            case PassiveUpgradeKind.FireCooldownBonus:
                return "Shoot faster for the rest of this run.";
            case PassiveUpgradeKind.XPBonus:
                return "Gain more XP from future kills.";
            default:
                return "Improves your run.";
        }
    }

    // What: Return the current level count for one passive upgrade.
    // Human: Chose to show current level on upgrade cards.
    // AI: Helped route UI level text through GameManager.
    public int GetPassiveUpgradeLevel(PassiveUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case PassiveUpgradeKind.MaxLivesUp: return maxLivesUpLevel;
            case PassiveUpgradeKind.MoveSpeedUp: return moveSpeedUpLevel;
            case PassiveUpgradeKind.FireCooldownBonus: return fireCooldownBonusLevel;
            case PassiveUpgradeKind.XPBonus: return xpBonusLevel;
            default: return 0;
        }
    }

    // What: Pause active gameplay and freeze time.
    // Human: Chose pause behaviour.
    // AI: Helped keep phase/timeScale changes together.
    public void Pause()
    {
        if (phase != Phase.Playing) return;
        phase = Phase.Paused;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke();
    }

    // What: Resume gameplay from the pause phase.
    // Human: Chose resume flow from the pause menu.
    // AI: Helped restore timeScale and notify UI listeners.
    public void Resume()
    {
        if (phase != Phase.Paused) return;
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke();
    }

    // What: Return to the title menu and clear run progress.
    // Human: Chose home-menu reset behaviour.
    // AI: Helped reuse ResetRunProgress for predictable cleanup.
    public void ReturnToMenu()
    {
        phase = Phase.Menu;
        Time.timeScale = 1f;
        ResetRunProgress();
        OnStateChanged?.Invoke();
    }

    // What: Clear every run-scoped counter, upgrade level, cache, boss flag, and XP value.
    // Human: Defined which values reset between runs.
    // AI: Helped list all state fields so restart is deterministic.
    void ResetRunProgress()
    {
        // These values are intentionally stored in GameManager rather than scattered through UI,
        // player, and enemy scripts. That makes the HUD and upgrade screens read from one source of
        // truth and makes a restart predictable.
        startLives = Mathf.Max(1, baseStartLives);
        playerLevel = Mathf.Max(1, startingLevel);
        currentFloor = 1;
        currentXP = 0;
        xpToNextLevel = Mathf.Max(1, startingXPToNextLevel);
        enemiesDefeated = 0;
        floorsCleared = 0;
        passiveUpgradesChosen = 0;
        extraProjectileLevel = 0;
        rapidFireLevel = 0;
        damageUpLevel = 0;
        piercingShotLevel = 0;
        burnShotLevel = 0;
        slowShotLevel = 0;
        explosiveShotLevel = 0;
        maxLivesUpLevel = 0;
        moveSpeedUpLevel = 0;
        fireCooldownBonusLevel = 0;
        xpBonusLevel = 0;
        finalRunScore = 0;
        lastRunWasNewRecord = false;
        objectivePositionsReady = false;
        bossAliveThisFloor = false;
        activeBoss = null;
        playerCombat = null;
        player = null;
    }

    // What: Persist the finished run into best-record and local leaderboard storage.
    // Human: Wanted each run to answer whether the player broke their record.
    // AI: Helped keep record storage local and simple with PlayerPrefs.
    void RecordRunResult()
    {
        finalRunScore = CurrentScore;
        int finalFloor = Mathf.Max(1, currentFloor);
        int finalEnemies = Mathf.Max(0, enemiesDefeated);
        int finalTime = Mathf.FloorToInt(elapsed);
        int finalLevel = Mathf.Max(1, playerLevel);
        int oldBestScore = BestScore;
        int oldBestFloor = BestFloor;

        lastRunWasNewRecord = finalRunScore > oldBestScore ||
            (finalRunScore == oldBestScore && finalFloor > oldBestFloor);

        if (lastRunWasNewRecord)
        {
            PlayerPrefs.SetInt(BestScoreKey, finalRunScore);
            PlayerPrefs.SetInt(BestFloorKey, finalFloor);
            PlayerPrefs.SetInt(BestEnemiesKey, finalEnemies);
            PlayerPrefs.SetInt(BestTimeKey, finalTime);
            PlayerPrefs.SetInt(BestLevelKey, finalLevel);
        }

        InsertLeaderboardEntry(finalRunScore, finalFloor, finalEnemies, finalTime, finalLevel);
        PlayerPrefs.Save();
    }

    void InsertLeaderboardEntry(int score, int floor, int enemies, int time, int level)
    {
        int insertIndex = LeaderboardSize;
        for (int i = 0; i < LeaderboardSize; i++)
        {
            int savedScore = PlayerPrefs.GetInt(LeaderboardScoreKey + i, 0);
            int savedFloor = PlayerPrefs.GetInt(LeaderboardFloorKey + i, 0);
            if (score > savedScore || (score == savedScore && floor > savedFloor))
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex >= LeaderboardSize) return;

        for (int i = LeaderboardSize - 1; i > insertIndex; i--)
        {
            CopyLeaderboardEntry(i - 1, i);
        }

        PlayerPrefs.SetInt(LeaderboardScoreKey + insertIndex, score);
        PlayerPrefs.SetInt(LeaderboardFloorKey + insertIndex, floor);
        PlayerPrefs.SetInt(LeaderboardEnemiesKey + insertIndex, enemies);
        PlayerPrefs.SetInt(LeaderboardTimeKey + insertIndex, time);
        PlayerPrefs.SetInt(LeaderboardLevelKey + insertIndex, level);
    }

    void CopyLeaderboardEntry(int from, int to)
    {
        PlayerPrefs.SetInt(LeaderboardScoreKey + to, PlayerPrefs.GetInt(LeaderboardScoreKey + from, 0));
        PlayerPrefs.SetInt(LeaderboardFloorKey + to, PlayerPrefs.GetInt(LeaderboardFloorKey + from, 0));
        PlayerPrefs.SetInt(LeaderboardEnemiesKey + to, PlayerPrefs.GetInt(LeaderboardEnemiesKey + from, 0));
        PlayerPrefs.SetInt(LeaderboardTimeKey + to, PlayerPrefs.GetInt(LeaderboardTimeKey + from, 0));
        PlayerPrefs.SetInt(LeaderboardLevelKey + to, PlayerPrefs.GetInt(LeaderboardLevelKey + from, 0));
    }

    // What: Spend the current XP threshold and open the weapon upgrade UI.
    // Human: Designed weapon upgrades as paused choices.
    // AI: Helped calculate the next threshold immediately after leveling.
    void BeginLevelUp()
    {
        // Weapon upgrades are XP-based and can happen mid-floor. Gameplay pauses so the player can
        // make a deliberate choice instead of being hit while reading the cards.
        currentXP -= xpToNextLevel;
        playerLevel++;
        xpToNextLevel = CalculateXPToNextLevel(playerLevel);
        objective = "Level up! Choose a weapon upgrade.";
        phase = Phase.LevelUp;
        Time.timeScale = 0f;
        OnLevelUpAvailable?.Invoke();
        OnStateChanged?.Invoke();
    }

    // What: Mark the floor cleared and open the passive upgrade UI.
    // Human: Designed passive upgrades as floor-clear rewards.
    // AI: Helped fire floor-clear events before showing the passive cards.
    void BeginPassiveUpgrade()
    {
        // Passive upgrades are floor-clear rewards. This separates moment-to-moment combat growth
        // from longer run survivability choices such as lives, speed, fire rate, and XP gain.
        floorsCleared++;
        RecordTutorialSuccessIfNeeded();
        objective = "Floor " + currentFloor + " cleared! Choose a passive upgrade.";
        phase = Phase.PassiveUpgrade;
        Time.timeScale = 0f;
        OnFloorCleared?.Invoke();
        OnPassiveUpgradeAvailable?.Invoke();
        OnStateChanged?.Invoke();
    }

    // What: Advance the floor number and notify the scene builder to generate the next room.
    // Human: Designed endless floor progression.
    // AI: Helped reset key and boss state during floor transition.
    void StartNextFloor()
    {
        // A new floor keeps run upgrades and lives, but resets the floor key objective.
        currentFloor++;
        hasKey = false;
        objectivePositionsReady = false;
        bossAliveThisFloor = false;
        activeBoss = null;
        objective = FloorObjective();
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnFloorStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    // What: Build the objective text for the current floor.
    // Human: Wrote player-facing objective messages.
    // AI: Helped keep boss-floor wording tied to BossFloorInterval.
    string FloorObjective()
    {
        if (currentFloor <= 1) return "Floor 1: Find the key, then enter the exit.";
        if (currentFloor % BossFloorInterval == 0) return "Floor " + currentFloor + ": Boss floor. Find the key and defeat the boss.";
        return "Floor " + currentFloor + ": Find the key, survive, and keep going.";
    }

    // What: Decide whether tutorial prompts should still be shown for this player profile.
    // Human: Chose to stop prompts after a few successful first-floor clears.
    // AI: Helped store the count in PlayerPrefs.
    public bool ShouldShowTutorialPrompts()
    {
        return PlayerPrefs.GetInt(TutorialSuccessfulRunsKey, 0) < 3;
    }

    // What: Count one successful tutorial run when the player clears floor 1.
    // Human: Defined floor 1 clear as tutorial success.
    // AI: Helped cap and persist the counter safely.
    void RecordTutorialSuccessIfNeeded()
    {
        if (countedTutorialSuccessThisRun || currentFloor != 1) return;

        countedTutorialSuccessThisRun = true;
        int successfulRuns = PlayerPrefs.GetInt(TutorialSuccessfulRunsKey, 0);
        PlayerPrefs.SetInt(TutorialSuccessfulRunsKey, Mathf.Min(3, successfulRuns + 1));
        PlayerPrefs.Save();
    }

    // What: Calculate the XP threshold for the next player level.
    // Human: Tuned XP growth values for combat pacing.
    // AI: Helped combine multiplicative and flat growth.
    int CalculateXPToNextLevel(int level)
    {
        float scaled = startingXPToNextLevel * Mathf.Pow(Mathf.Max(1f, xpGrowthMultiplier), Mathf.Max(0, level - startingLevel));
        int flatBonus = Mathf.Max(0, level - startingLevel) * xpGrowthFlatBonus;
        return Mathf.Max(1, Mathf.RoundToInt(scaled) + flatBonus);
    }

    // What: Return the player-facing weapon upgrade name for UI cards.
    // Human: Wrote final weapon names.
    // AI: Helped expose display names through GameManager.
    public string GetWeaponUpgradeDisplayName(WeaponUpgradeKind upgrade)
    {
        return UpgradeDisplayName(upgrade);
    }

    // What: Return the weapon upgrade description shown on cards.
    // Human: Wrote final upgrade descriptions.
    // AI: Helped keep UI copy centralized.
    public string GetWeaponUpgradeDescription(WeaponUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeKind.ExtraProjectile:
                return "Adds more shots to each volley.";
            case WeaponUpgradeKind.RapidFire:
                return "Reduces time between shots.";
            case WeaponUpgradeKind.DamageUp:
                return "Increases bullet damage.";
            case WeaponUpgradeKind.PiercingShot:
                return "Bullets pass through more enemies.";
            case WeaponUpgradeKind.BurnShot:
                return "Hits apply damage over time.";
            case WeaponUpgradeKind.SlowShot:
                return "Hits briefly slow enemies.";
            case WeaponUpgradeKind.ExplosiveShot:
                return "Hits damage nearby enemies.";
            default:
                return "Improves your weapon.";
        }
    }

    // What: Return the current level count for one weapon upgrade.
    // Human: Chose to show current weapon level on cards.
    // AI: Helped avoid UI reading PlayerCombat internals.
    public int GetWeaponUpgradeLevel(WeaponUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeKind.ExtraProjectile: return extraProjectileLevel;
            case WeaponUpgradeKind.RapidFire: return rapidFireLevel;
            case WeaponUpgradeKind.DamageUp: return damageUpLevel;
            case WeaponUpgradeKind.PiercingShot: return piercingShotLevel;
            case WeaponUpgradeKind.BurnShot: return burnShotLevel;
            case WeaponUpgradeKind.SlowShot: return slowShotLevel;
            case WeaponUpgradeKind.ExplosiveShot: return explosiveShotLevel;
            default: return 0;
        }
    }

    // What: Convert a weapon enum value into display text.
    // Human: Chose the names players see.
    // AI: Helped keep enum-to-text conversion in one switch.
    string UpgradeDisplayName(WeaponUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeKind.ExtraProjectile: return "Extra Projectile";
            case WeaponUpgradeKind.RapidFire: return "Rapid Fire";
            case WeaponUpgradeKind.DamageUp: return "Damage Up";
            case WeaponUpgradeKind.PiercingShot: return "Piercing Shot";
            case WeaponUpgradeKind.BurnShot: return "Burn Shot";
            case WeaponUpgradeKind.SlowShot: return "Slow Shot";
            case WeaponUpgradeKind.ExplosiveShot: return "Explosive Shot";
            default: return upgrade.ToString();
        }
    }

    // What: Increment the stored level counter for one chosen weapon upgrade.
    // Human: Decided upgrade levels should appear in HUD/end summary.
    // AI: Helped separate counters from the actual PlayerCombat stat changes.
    void RegisterWeaponUpgrade(WeaponUpgradeKind upgrade)
    {
        // These counters drive HUD/build summaries and card "current level" text. PlayerCombat owns
        // the actual weapon stat changes.
        switch (upgrade)
        {
            case WeaponUpgradeKind.ExtraProjectile:
                extraProjectileLevel++;
                break;
            case WeaponUpgradeKind.RapidFire:
                rapidFireLevel++;
                break;
            case WeaponUpgradeKind.DamageUp:
                damageUpLevel++;
                break;
            case WeaponUpgradeKind.PiercingShot:
                piercingShotLevel++;
                break;
            case WeaponUpgradeKind.BurnShot:
                burnShotLevel++;
                break;
            case WeaponUpgradeKind.SlowShot:
                slowShotLevel++;
                break;
            case WeaponUpgradeKind.ExplosiveShot:
                explosiveShotLevel++;
                break;
        }
    }

    // What: Find PlayerCombat and tell it to apply the selected weapon upgrade.
    // Human: Designed combat upgrades to live on PlayerCombat.
    // AI: Helped make lookup lazy because objects are created at runtime.
    void ApplyWeaponUpgrade(WeaponUpgradeKind upgrade)
    {
        // PlayerCombat may not be cached yet because the runtime scene is generated in Awake/Start
        // order. GetPlayerCombat handles the lookup lazily.
        PlayerCombat combat = GetPlayerCombat();
        if (combat != null) combat.ApplyUpgrade(upgrade);
    }

    // What: Find and cache the active PlayerCombat component.
    // Human: Chose a single player combat component.
    // AI: Helped cache the lookup while allowing runtime object creation.
    PlayerCombat GetPlayerCombat()
    {
        if (playerCombat != null) return playerCombat;

        playerCombat = FindFirstObjectByType<PlayerCombat>();
        return playerCombat;
    }

    // What: Find and cache the active Player component.
    // Human: Chose a single player controller.
    // AI: Helped use lazy lookup for generated scene order.
    Player GetPlayer()
    {
        if (player != null) return player;

        player = FindFirstObjectByType<Player>();
        return player;
    }

    // What: Increment the stored level counter for one chosen passive upgrade.
    // Human: Decided passive levels should appear in HUD/end summary.
    // AI: Helped keep counters parallel to weapon upgrade counters.
    void RegisterPassiveUpgrade(PassiveUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case PassiveUpgradeKind.MaxLivesUp:
                maxLivesUpLevel++;
                break;
            case PassiveUpgradeKind.MoveSpeedUp:
                moveSpeedUpLevel++;
                break;
            case PassiveUpgradeKind.FireCooldownBonus:
                fireCooldownBonusLevel++;
                break;
            case PassiveUpgradeKind.XPBonus:
                xpBonusLevel++;
                break;
        }
    }

    // What: Apply the gameplay effect for one passive upgrade.
    // Human: Designed passive effects and balance values.
    // AI: Helped split effects across GameManager, Player, and PlayerCombat.
    void ApplyPassiveUpgrade(PassiveUpgradeKind upgrade)
    {
        // Passive upgrades are split between GameManager state and player/combat components. This
        // keeps permanent run stats such as lives here while movement/fire-rate changes stay on the
        // components that use them.
        switch (upgrade)
        {
            case PassiveUpgradeKind.MaxLivesUp:
                startLives++;
                lives++;
                break;
            case PassiveUpgradeKind.MoveSpeedUp:
                Player runPlayer = GetPlayer();
                if (runPlayer != null) runPlayer.ApplyMoveSpeedBonus(0.42f);
                break;
            case PassiveUpgradeKind.FireCooldownBonus:
                PlayerCombat combat = GetPlayerCombat();
                if (combat != null) combat.ApplyFireCooldownBonus(0.91f);
                break;
            case PassiveUpgradeKind.XPBonus:
                break;
        }
    }

    // What: Convert XP bonus passive level into a reward multiplier.
    // Human: Tuned XP bonus strength.
    // AI: Helped isolate the formula in one helper.
    float XPBonusMultiplier()
    {
        return 1f + xpBonusLevel * 0.25f;
    }

    // What: Append one leveled upgrade label to a slash-separated summary string.
    // Human: Chose compact build summaries for HUD/end screens.
    // AI: Helped avoid repeated string-building logic.
    string AppendUpgradeSummary(string summary, string label, int level)
    {
        if (level <= 0) return summary;

        string item = label + " Lv." + level;
        return string.IsNullOrEmpty(summary) ? item : summary + " / " + item;
    }

    // What: Convert a passive enum value into display text.
    // Human: Chose the passive names players see.
    // AI: Helped keep enum-to-text conversion in one switch.
    string PassiveUpgradeDisplayName(PassiveUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case PassiveUpgradeKind.MaxLivesUp: return "Max Lives Up";
            case PassiveUpgradeKind.MoveSpeedUp: return "Move Speed Up";
            case PassiveUpgradeKind.FireCooldownBonus: return "Fire Cooldown Bonus";
            case PassiveUpgradeKind.XPBonus: return "XP Bonus";
            default: return upgrade.ToString();
        }
    }

}

/// Run-scoped weapon upgrade choices offered when the player levels up.
///
/// Authorship note:
/// - Student-owned design: upgrade categories, names, and how they change the combat build.
/// - AI-assisted support: enum organization and comments so UI, GameManager, and PlayerCombat use
///   the same strongly typed upgrade list.
public enum WeaponUpgradeKind
{
    ExtraProjectile,
    RapidFire,
    DamageUp,
    PiercingShot,
    BurnShot,
    SlowShot,
    ExplosiveShot
}

/// Run-scoped passive upgrades awarded after clearing a small floor.
///
/// Authorship note:
/// - Student-owned design: passive reward categories and floor-clear timing.
/// - AI-assisted support: enum organization and comments so the passive UI and GameManager share
///   one source of truth.
public enum PassiveUpgradeKind
{
    MaxLivesUp,
    MoveSpeedUp,
    FireCooldownBonus,
    XPBonus
}
