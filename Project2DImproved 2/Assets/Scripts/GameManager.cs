using System;
using UnityEngine;

/// Central state for the Dungeon Key Run vertical slice.
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum Phase { Menu, Playing, Paused, LevelUp, PassiveUpgrade, Won, GameOver }
    public Phase phase = Phase.Menu;

    [Header("Tuning")]
    public int startLives = 3;
    public int startingLevel = 1;
    public int startingXPToNextLevel = 20;
    public float xpGrowthMultiplier = 1.35f;
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

    int baseStartLives;
    PlayerCombat playerCombat;
    Player player;

    public event Action OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnRunEnded;
    public event Action OnLevelUpAvailable;
    public event Action OnPassiveUpgradeAvailable;
    public event Action OnFloorStarted;
    public event Action OnFloorCleared;

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

    void Update()
    {
        if (phase == Phase.LevelUp)
        {
            HandleWeaponUpgradeDebugInput();
            return;
        }

        if (phase == Phase.PassiveUpgrade)
        {
            HandlePassiveUpgradeDebugInput();
            return;
        }

        if (phase != Phase.Playing) return;

        elapsed += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Escape)) Pause();
        OnStateChanged?.Invoke();
    }

    public void StartGame()
    {
        hasKey = false;
        elapsed = 0f;
        ResetRunProgress();
        lives = startLives;
        objective = FloorObjective();
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
        OnFloorStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void CollectKey()
    {
        if (phase != Phase.Playing || hasKey) return;

        hasKey = true;
        objective = "Key collected. Enter the exit to choose a passive upgrade.";
        OnStateChanged?.Invoke();
    }

    public void TryExit()
    {
        if (phase != Phase.Playing) return;

        if (!hasKey)
        {
            objective = "The exit is locked. Collect the floor key first.";
            OnStateChanged?.Invoke();
            return;
        }

        BeginPassiveUpgrade();
    }

    public void PlayerHit()
    {
        if (phase != Phase.Playing) return;

        lives = Mathf.Max(0, lives - 1);
        objective = lives > 0
            ? "Careful. Avoid the guards and find the exit."
            : "You were caught by the dungeon guards.";

        if (lives == 0)
        {
            phase = Phase.GameOver;
            Time.timeScale = 0f;
            OnRunEnded?.Invoke();
        }

        OnStateChanged?.Invoke();
    }

    public void RegisterEnemyDefeated(int xpReward)
    {
        if (phase != Phase.Playing) return;

        enemiesDefeated++;
        AddXP(Mathf.RoundToInt(xpReward * XPBonusMultiplier()));
    }

    public void AddXP(int amount)
    {
        if (phase != Phase.Playing || amount <= 0) return;

        currentXP += amount;
        if (currentXP >= xpToNextLevel) BeginLevelUp();
        else OnStateChanged?.Invoke();
    }

    public void ChooseUpgrade(WeaponUpgradeKind upgrade)
    {
        if (phase != Phase.LevelUp) return;

        RegisterWeaponUpgrade(upgrade);
        ApplyWeaponUpgrade(upgrade);
        objective = "Weapon upgrade chosen: " + UpgradeDisplayName(upgrade) + ". Keep pushing deeper.";

        if (currentXP >= xpToNextLevel)
        {
            BeginLevelUp();
            return;
        }

        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke();
    }

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

    public void ChoosePassiveUpgrade(PassiveUpgradeKind upgrade)
    {
        if (phase != Phase.PassiveUpgrade) return;

        passiveUpgradesChosen++;
        RegisterPassiveUpgrade(upgrade);
        ApplyPassiveUpgrade(upgrade);
        objective = "Passive upgrade chosen: " + PassiveUpgradeDisplayName(upgrade) + ".";
        StartNextFloor();
    }

    public string GetPassiveBuildSummary()
    {
        string summary = AppendUpgradeSummary("", "Life", maxLivesUpLevel);
        summary = AppendUpgradeSummary(summary, "Move", moveSpeedUpLevel);
        summary = AppendUpgradeSummary(summary, "Fire", fireCooldownBonusLevel);
        summary = AppendUpgradeSummary(summary, "XP", xpBonusLevel);
        return string.IsNullOrEmpty(summary) ? "None" : summary;
    }

    public string GetPassiveUpgradeDisplayName(PassiveUpgradeKind upgrade)
    {
        return PassiveUpgradeDisplayName(upgrade);
    }

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

    public void Pause()
    {
        if (phase != Phase.Playing) return;
        phase = Phase.Paused;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke();
    }

    public void Resume()
    {
        if (phase != Phase.Paused) return;
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke();
    }

    public void ReturnToMenu()
    {
        phase = Phase.Menu;
        Time.timeScale = 1f;
        ResetRunProgress();
        OnStateChanged?.Invoke();
    }

    void ResetRunProgress()
    {
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
        playerCombat = null;
        player = null;
    }

    void BeginLevelUp()
    {
        currentXP -= xpToNextLevel;
        playerLevel++;
        xpToNextLevel = CalculateXPToNextLevel(playerLevel);
        objective = "Level up! Choose a weapon upgrade.";
        phase = Phase.LevelUp;
        Time.timeScale = 0f;
        OnLevelUpAvailable?.Invoke();
        OnStateChanged?.Invoke();
    }

    void BeginPassiveUpgrade()
    {
        floorsCleared++;
        objective = "Floor cleared! Choose a passive upgrade: 1 Life, 2 Move, 3 Fire, 4 XP.";
        phase = Phase.PassiveUpgrade;
        Time.timeScale = 0f;
        OnFloorCleared?.Invoke();
        OnPassiveUpgradeAvailable?.Invoke();
        OnStateChanged?.Invoke();
    }

    void StartNextFloor()
    {
        currentFloor++;
        hasKey = false;
        objective = FloorObjective();
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnFloorStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    string FloorObjective()
    {
        if (currentFloor <= 1) return "Floor 1: Find the key, then enter the exit.";
        return "Floor " + currentFloor + ": Find the key, survive, and keep going.";
    }

    int CalculateXPToNextLevel(int level)
    {
        float scaled = startingXPToNextLevel * Mathf.Pow(Mathf.Max(1f, xpGrowthMultiplier), Mathf.Max(0, level - startingLevel));
        int flatBonus = Mathf.Max(0, level - startingLevel) * xpGrowthFlatBonus;
        return Mathf.Max(1, Mathf.RoundToInt(scaled) + flatBonus);
    }

    public string GetWeaponUpgradeDisplayName(WeaponUpgradeKind upgrade)
    {
        return UpgradeDisplayName(upgrade);
    }

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

    void RegisterWeaponUpgrade(WeaponUpgradeKind upgrade)
    {
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

    void ApplyWeaponUpgrade(WeaponUpgradeKind upgrade)
    {
        PlayerCombat combat = GetPlayerCombat();
        if (combat != null) combat.ApplyUpgrade(upgrade);
    }

    PlayerCombat GetPlayerCombat()
    {
        if (playerCombat != null) return playerCombat;

        playerCombat = FindFirstObjectByType<PlayerCombat>();
        return playerCombat;
    }

    Player GetPlayer()
    {
        if (player != null) return player;

        player = FindFirstObjectByType<Player>();
        return player;
    }

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

    void ApplyPassiveUpgrade(PassiveUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case PassiveUpgradeKind.MaxLivesUp:
                startLives++;
                lives++;
                break;
            case PassiveUpgradeKind.MoveSpeedUp:
                Player runPlayer = GetPlayer();
                if (runPlayer != null) runPlayer.ApplyMoveSpeedBonus(0.35f);
                break;
            case PassiveUpgradeKind.FireCooldownBonus:
                PlayerCombat combat = GetPlayerCombat();
                if (combat != null) combat.ApplyFireCooldownBonus(0.9f);
                break;
            case PassiveUpgradeKind.XPBonus:
                break;
        }
    }

    float XPBonusMultiplier()
    {
        return 1f + xpBonusLevel * 0.2f;
    }

    string AppendUpgradeSummary(string summary, string label, int level)
    {
        if (level <= 0) return summary;

        string item = label + " Lv." + level;
        return string.IsNullOrEmpty(summary) ? item : summary + " / " + item;
    }

    void HandleWeaponUpgradeDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseUpgrade(WeaponUpgradeKind.ExtraProjectile);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseUpgrade(WeaponUpgradeKind.RapidFire);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChooseUpgrade(WeaponUpgradeKind.DamageUp);
    }

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

    void HandlePassiveUpgradeDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChoosePassiveUpgrade(PassiveUpgradeKind.MaxLivesUp);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChoosePassiveUpgrade(PassiveUpgradeKind.MoveSpeedUp);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChoosePassiveUpgrade(PassiveUpgradeKind.FireCooldownBonus);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) ChoosePassiveUpgrade(PassiveUpgradeKind.XPBonus);
    }
}

/// Run-scoped weapon upgrade choices offered when the player levels up.
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
public enum PassiveUpgradeKind
{
    MaxLivesUp,
    MoveSpeedUp,
    FireCooldownBonus,
    XPBonus
}
