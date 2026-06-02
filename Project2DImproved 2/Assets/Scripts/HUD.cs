using UnityEngine;
using UnityEngine.UI;

/// In-play HUD for lives, key state, timer and current objective.
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
    }
}
