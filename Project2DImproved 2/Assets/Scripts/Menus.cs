using UnityEngine;
using UnityEngine.UI;

/// Menu controller for main, instructions, credits, pause, win and fail pages.
public class Menus : MonoBehaviour
{
    public GameObject mainPage;
    public GameObject instructionsPage;
    public GameObject creditsPage;
    public GameObject pausePage;
    public GameObject winPage;
    public GameObject gameOverPage;
    public Text winText;
    public Text gameOverText;
    public AudioClip clickClip;

    AudioSource audioSrc;

    void Awake()
    {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    void Start()
    {
        GameManager.I.OnStateChanged += SyncToPhase;
        GameManager.I.OnRunEnded += UpdateEndText;
        SyncToPhase();
    }

    void SyncToPhase()
    {
        GameManager.Phase phase = GameManager.I.phase;
        bool inMenu = phase == GameManager.Phase.Menu;
        if (mainPage != null) mainPage.SetActive(inMenu);
        if (pausePage != null) pausePage.SetActive(phase == GameManager.Phase.Paused);
        if (winPage != null) winPage.SetActive(phase == GameManager.Phase.Won);
        if (gameOverPage != null) gameOverPage.SetActive(phase == GameManager.Phase.GameOver);

        if (!inMenu)
        {
            if (instructionsPage != null) instructionsPage.SetActive(false);
            if (creditsPage != null) creditsPage.SetActive(false);
        }
    }

    void UpdateEndText()
    {
        GameManager gm = GameManager.I;
        string time = Mathf.FloorToInt(gm.elapsed) + " seconds";
        string runSummary =
            "Final Floor: " + gm.currentFloor + "\n" +
            "Final Level: " + gm.playerLevel + "\n" +
            "Enemies Defeated: " + gm.enemiesDefeated + "\n" +
            "Survival Time: " + time + "\n" +
            "Weapon Build: " + gm.GetWeaponBuildSummary() + "\n" +
            "Passive Build: " + gm.GetPassiveBuildSummary();

        if (winText != null) winText.text = "Run Complete\n\n" + runSummary;
        if (gameOverText != null)
            gameOverText.text = "Infinite Run Ended\n\n" + runSummary;
    }

    public void Click()
    {
        if (clickClip != null) audioSrc.PlayOneShot(clickClip, 0.6f);
    }

    public void OnStart() { Click(); GameManager.I.StartGame(); }
    public void OnInstructions() { Click(); ShowOnly(instructionsPage); }
    public void OnCredits() { Click(); ShowOnly(creditsPage); }
    public void OnBackToMenu() { Click(); ShowOnly(mainPage); }
    public void OnResume() { Click(); GameManager.I.Resume(); }
    public void OnRestart() { Click(); GameManager.I.StartGame(); }
    public void OnReturnHome() { Click(); GameManager.I.ReturnToMenu(); }

    public void OnQuit()
    {
        Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ShowOnly(GameObject page)
    {
        if (mainPage != null) mainPage.SetActive(page == mainPage);
        if (instructionsPage != null) instructionsPage.SetActive(page == instructionsPage);
        if (creditsPage != null) creditsPage.SetActive(page == creditsPage);
    }
}
