using UnityEngine;
using UnityEngine.UI;

/// In-play HUD: subscribes to GameManager events and re-paints text every frame
/// for the live timer / power-up countdowns.
public class HUD : MonoBehaviour
{
    public Text scoreText;
    public Text highScoreText;
    public Text livesText;
    public Text levelText;
    public Text timerText;
    public Text powerText;
    public Text objectiveText;
    public Player player;
    public GameObject root;        // whole HUD canvas — toggled with phase

    void Start()
    {
        var gm = GameManager.I;
        gm.OnStateChanged += Repaint;
        gm.OnGameStarted += () =>
        {
            if (root != null) root.SetActive(true);
            if (objectiveText != null)
                objectiveText.text = "Survive! Shoot the falling enemies. Grab power-ups.";
        };
        gm.OnGameOver += () => { if (root != null) root.SetActive(false); };
        if (root != null) root.SetActive(false);
    }

    void Update()
    {
        if (GameManager.I == null) return;
        if (timerText != null)
            timerText.text = "Time " + Mathf.FloorToInt(GameManager.I.elapsed) + "s";
        if (powerText != null) powerText.text = BuildPowerString();
    }

    void Repaint()
    {
        var gm = GameManager.I;
        if (scoreText != null)     scoreText.text     = "Score " + gm.score;
        if (highScoreText != null) highScoreText.text = "Hi " + gm.highScore;
        if (livesText != null)     livesText.text     = "Lives " + gm.lives;
        if (levelText != null)     levelText.text     = "Level " + gm.level;
    }

    string BuildPowerString()
    {
        if (player == null) return "";
        string s = "";
        if (player.HasRapid)  s += "Rapid " + player.rapidTimer.ToString("F1") + "s  ";
        if (player.HasShield) s += "Shield " + player.shieldTimer.ToString("F1") + "s";
        return s;
    }
}
