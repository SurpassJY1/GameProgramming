using UnityEngine;
using UnityEngine.UI;

/// In-play HUD for lives, key state, timer and current objective.
public class HUD : MonoBehaviour
{
    public Text livesText;
    public Text keyText;
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
        if (keyText != null) keyText.text = gm.hasKey ? "Key: Collected" : "Key: Missing";
        if (timerText != null) timerText.text = "Time: " + Mathf.FloorToInt(gm.elapsed) + "s";
        if (objectiveText != null) objectiveText.text = gm.objective;
    }
}
