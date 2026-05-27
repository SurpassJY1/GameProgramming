using UnityEngine;
using UnityEngine.UI;

/// All menu panels live on one Canvas and toggle visibility through this script.
/// Pages: Main, Instructions, Credits, Pause, End.
public class Menus : MonoBehaviour
{
    public GameObject mainPage;
    public GameObject instructionsPage;
    public GameObject creditsPage;
    public GameObject pausePage;
    public GameObject gameOverPage;
    public Text gameOverScoreText;
    public AudioClip clickClip;

    AudioSource audioSrc;

    void Awake()
    {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    void Start()
    {
        var gm = GameManager.I;
        gm.OnStateChanged += SyncToPhase;
        gm.OnVictory += () => SetEndText("Victory!", "You collected the key and escaped the dungeon.");
        gm.OnGameOver += () => SetEndText("Game Over", "You ran out of lives. Try a safer route next time.");
        SyncToPhase();
    }

    void SyncToPhase()
    {
        var phase = GameManager.I.phase;
        bool inMenu = phase == GameManager.Phase.Menu;
        if (mainPage != null) mainPage.SetActive(inMenu);
        if (pausePage != null) pausePage.SetActive(phase == GameManager.Phase.Paused);
        if (gameOverPage != null)
            gameOverPage.SetActive(phase == GameManager.Phase.GameOver || phase == GameManager.Phase.Victory);
        // Sub-pages always start hidden when phase changes.
        if (!inMenu)
        {
            if (instructionsPage != null) instructionsPage.SetActive(false);
            if (creditsPage != null) creditsPage.SetActive(false);
        }
    }

    public void Click() { if (clickClip != null) audioSrc.PlayOneShot(clickClip, 0.6f); }

    // Wired by buttons:
    public void OnStart()        { Click(); GameManager.I.StartGame(); }
    public void OnInstructions() { Click(); ShowOnly(instructionsPage); }
    public void OnCredits()      { Click(); ShowOnly(creditsPage); }
    public void OnBackToMenu()   { Click(); ShowOnly(mainPage); }
    public void OnQuit()
    {
        Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnResume()       { Click(); GameManager.I.Resume(); }
    public void OnRestart()      { Click(); CleanupRunActors(); GameManager.I.StartGame(); }
    public void OnReturnHome()   { Click(); CleanupRunActors(); GameManager.I.ReturnToMenu(); }

    void SetEndText(string title, string body)
    {
        if (gameOverScoreText == null) return;
        gameOverScoreText.text = title + "\n" + body + "\nTime: "
            + Mathf.FloorToInt(GameManager.I.elapsed) + "s";
    }

    void ShowOnly(GameObject page)
    {
        if (mainPage != null) mainPage.SetActive(page == mainPage);
        if (instructionsPage != null) instructionsPage.SetActive(page == instructionsPage);
        if (creditsPage != null) creditsPage.SetActive(page == creditsPage);
    }

    /// Reset runtime pickups and player position so a restart starts clean.
    void CleanupRunActors()
    {
        foreach (var b in Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            Destroy(b.gameObject);
        foreach (var e in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            e.ResetEnemy();
        foreach (var k in Object.FindObjectsByType<KeyPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            k.gameObject.SetActive(true);
    }
}
