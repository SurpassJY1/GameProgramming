using UnityEngine;
using UnityEngine.UI;

/// In-play HUD for the dungeon slice: lives, key state, timer, and objective.
public class HUD : MonoBehaviour
{
    public Text titleText;
    public Text livesText;
    public Text keyText;
    public Text timerText;
    public Text objectiveText;
    public Player player;
    public GameObject root;

    void Start()
    {
        var gm = GameManager.I;
        gm.OnStateChanged += Repaint;
        gm.OnGameStarted += () =>
        {
            if (root != null) root.SetActive(true);
            Repaint();
        };
        gm.OnGameOver += () => { if (root != null) root.SetActive(false); };
        gm.OnVictory += () => { if (root != null) root.SetActive(false); };
        if (root != null) root.SetActive(false);
    }

    void Update()
    {
        if (GameManager.I == null) return;
        if (timerText != null)
            timerText.text = "Time " + Mathf.FloorToInt(GameManager.I.elapsed) + "s";
    }

    void Repaint()
    {
        var gm = GameManager.I;
        if (titleText != null) titleText.text = "Dungeon Key Run";
        if (livesText != null) livesText.text = "Lives " + gm.lives;
        if (keyText != null) keyText.text = gm.hasKey ? "Key Yes" : "Key No";
        if (objectiveText != null) objectiveText.text = gm.objective;
    }
}
