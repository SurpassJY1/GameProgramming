using UnityEngine;
using UnityEngine.UI;

/// In-play HUD for lives, key state, timer, current objective, and boss health.
///
/// Authorship note:
/// - Student-completed code: required HUD information and the design requirement that a boss
///   health bar appears at the bottom of the screen during boss encounters.
/// - AI-assisted support: code review suggestions, wiring guidance for GameManager.activeBoss, and
///   comments that clarify when the boss bar appears. Final UI behaviour was reviewed by the student.
public class HUD : MonoBehaviour
{
    public Text floorText;
    public Text livesText;
    public Text keyText;
    public Text enemiesText;
    public Text levelText;
    public Text xpText;
    public Image xpBarFill;
    public Text weaponText;
    public Text passiveText;
    public Text timerText;
    public Text objectiveText;
    public GameObject bossBarRoot;
    public Image bossBarFill;
    public Text bossBarText;
    public GameObject root;

    void Start()
    {
        GameManager.I.OnStateChanged += Repaint;
        GameManager.I.OnGameStarted += () =>
        {
            if (root != null) root.SetActive(true);
            Repaint();
        };
        GameManager.I.OnRunEnded += () => { if (root != null) root.SetActive(false); };
        if (root != null) root.SetActive(false);
    }

    void Update()
    {
        if (GameManager.I != null && GameManager.I.phase == GameManager.Phase.Playing) Repaint();
    }

    void Repaint()
    {
        GameManager gm = GameManager.I;
        if (gm == null) return;

        if (livesText != null) livesText.text = "Lives: " + gm.lives;
        if (floorText != null) floorText.text = "Floor: " + gm.currentFloor;
        if (keyText != null) keyText.text = gm.hasKey ? "Key: Collected" : "Key: Missing";
        if (enemiesText != null) enemiesText.text = "Defeated: " + gm.enemiesDefeated;
        if (levelText != null) levelText.text = "Level: " + gm.playerLevel;
        if (xpText != null) xpText.text = "XP: " + gm.currentXP + " / " + gm.xpToNextLevel;
        if (xpBarFill != null) xpBarFill.fillAmount = gm.xpToNextLevel > 0 ? Mathf.Clamp01((float)gm.currentXP / gm.xpToNextLevel) : 0f;
        if (weaponText != null) weaponText.text = "Weapon: " + gm.GetWeaponBuildSummary();
        if (passiveText != null) passiveText.text = "Passives: " + gm.GetPassiveBuildSummary();
        if (timerText != null) timerText.text = "Time: " + Mathf.FloorToInt(gm.elapsed) + "s";
        if (objectiveText != null) objectiveText.text = gm.objective;
        RepaintBossBar(gm);
    }

    void RepaintBossBar(GameManager gm)
    {
        // The boss bar is driven by gameplay state, not by floor number. This means it appears only
        // while a formal boss encounter is alive. Past bosses that return as weaker elite enemies
        // keep boss-style sprites/abilities, but they do not set bossAliveThisFloor and therefore
        // do not take over the HUD.
        bool showBossBar = gm.bossAliveThisFloor && gm.activeBoss != null && !gm.activeBoss.IsDead;
        if (bossBarRoot != null) bossBarRoot.SetActive(showBossBar);

        // The bottom-left weapon/passive summaries share screen space with the boss bar. Hiding
        // them during boss fights keeps the important health bar legible for the short demo.
        if (weaponText != null) weaponText.gameObject.SetActive(!showBossBar);
        if (passiveText != null) passiveText.gameObject.SetActive(!showBossBar);
        if (!showBossBar) return;

        if (bossBarFill != null) bossBarFill.fillAmount = gm.activeBoss.HealthFraction;
        if (bossBarText != null)
        {
            bossBarText.text = gm.activeBoss.displayName + "  " +
                gm.activeBoss.currentHealth + " / " + gm.activeBoss.maxHealth;
        }
    }
}
