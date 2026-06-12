using UnityEngine;
using UnityEngine.UI;

/// Menu controller for main, instructions, credits, pause, win and fail pages.
/// GameBootstrap creates the pages, then these button callbacks route user actions into
/// GameManager so menu state and gameplay state stay synchronized.
///
/// Authorship note:
/// - Student-owned implementation: menu flow, run summary content, button actions, and final UI
///   wording for the playable build.
/// - AI-assisted support: review suggestions and comments that clarify phase-driven page visibility
///   and editor-safe quit behaviour.
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
    public Text mainRecordText;
    public AudioClip clickClip;

    AudioSource audioSrc;

    // What: Create a local AudioSource for menu click sounds.
    // Human: Chose menu sound behaviour and button feedback.
    // AI: Suggested keeping menu audio separate from gameplay event audio.
    void Awake()
    {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    // What: Subscribe menu pages to GameManager state changes and initialize visible pages.
    // Human: Defined the menu, pause, and end-screen flow.
    // AI: Helped connect the UI to phase events instead of polling every frame.
    void Start()
    {
        // Menus do not poll every frame. They listen to GameManager phase changes and repaint only
        // when the run moves between menu, playing, paused, and end states.
        GameManager.I.OnStateChanged += SyncToPhase;
        GameManager.I.OnRunEnded += UpdateEndText;
        SyncToPhase();
    }

    // What: Show exactly the page that matches the current game phase.
    // Human: Decided which phases need visible menu overlays.
    // AI: Helped document the distinction between phase pages and menu subpages.
    void SyncToPhase()
    {
        GameManager.Phase phase = GameManager.I.phase;
        bool inMenu = phase == GameManager.Phase.Menu;
        // Only one root page should be visible for each phase. Instructions and credits are manual
        // subpages of the main menu, so they are hidden whenever gameplay is active.
        if (mainPage != null) mainPage.SetActive(inMenu);
        if (pausePage != null) pausePage.SetActive(phase == GameManager.Phase.Paused);
        if (winPage != null) winPage.SetActive(phase == GameManager.Phase.Won);
        if (gameOverPage != null) gameOverPage.SetActive(phase == GameManager.Phase.GameOver);

        if (!inMenu)
        {
            if (instructionsPage != null) instructionsPage.SetActive(false);
            if (creditsPage != null) creditsPage.SetActive(false);
        }
        else
        {
            UpdateMainRecordText();
        }
    }

    // What: Build the final run summary shown on win and game-over pages.
    // Human: Selected the stats that matter for the project presentation.
    // AI: Helped format the summary consistently for both endings.
    void UpdateEndText()
    {
        GameManager gm = GameManager.I;
        string time = Mathf.FloorToInt(gm.elapsed) + " seconds";
        int score = gm.finalRunScore > 0 ? gm.finalRunScore : gm.CurrentScore;
        string recordLine = gm.lastRunWasNewRecord ? "New Record!\n" : "";
        // The same summary is used for win and game-over pages so both endings show what the player
        // accomplished during the run.
        string runSummary =
            recordLine +
            "Score: " + score + "\n" +
            "Final Floor: " + gm.currentFloor + "\n" +
            "Final Level: " + gm.playerLevel + "\n" +
            "Enemies Defeated: " + gm.enemiesDefeated + "\n" +
            "Survival Time: " + time + "\n" +
            "Weapon Build: " + gm.GetWeaponBuildSummary() + "\n" +
            "Passive Build: " + gm.GetPassiveBuildSummary() + "\n\n" +
            "Leaderboard\n" + gm.GetLeaderboardSummary();

        if (winText != null) winText.text = "Run Complete\n\n" + runSummary;
        if (gameOverText != null)
            gameOverText.text = "Infinite Run Ended\n\n" + runSummary;
    }

    void UpdateMainRecordText()
    {
        if (mainRecordText == null || GameManager.I == null) return;
        mainRecordText.text = GameManager.I.GetBestRecordSummary();
    }

    // What: Play the shared menu click sound for button callbacks.
    // Human: Chose the click sound asset and volume.
    // AI: Suggested centralizing click playback so every button behaves the same way.
    public void Click()
    {
        if (clickClip != null) audioSrc.PlayOneShot(clickClip, 0.6f);
    }

    // What: Start a new run from the main menu.
    // Human: Owned the start button flow.
    // AI: Suggested routing through GameManager so all run state resets together.
    public void OnStart() { Click(); GameManager.I.StartGame(); }

    // What: Open the instructions page without changing gameplay state.
    // Human: Wrote the instructions content and menu structure.
    // AI: Helped keep this as a menu-only page switch.
    public void OnInstructions() { Click(); ShowOnly(instructionsPage); }

    // What: Open the credits page without changing gameplay state.
    // Human: Owned credit content and asset attribution choices.
    // AI: Helped keep credit navigation consistent with other menu buttons.
    public void OnCredits() { Click(); ShowOnly(creditsPage); }

    // What: Return from a menu subpage to the main page.
    // Human: Decided the menu navigation layout.
    // AI: Suggested one shared ShowOnly helper for subpage switches.
    public void OnBackToMenu() { Click(); ShowOnly(mainPage); }

    // What: Resume gameplay from the pause overlay.
    // Human: Chose Escape/pause behaviour.
    // AI: Helped route the button through GameManager.Resume.
    public void OnResume() { Click(); GameManager.I.Resume(); }

    // What: Restart the run from an end or pause page.
    // Human: Required a quick retry flow.
    // AI: Suggested reusing StartGame so restart uses the same reset path as first start.
    public void OnRestart() { Click(); GameManager.I.StartGame(); }

    // What: Leave the run and return to the title menu.
    // Human: Chose to reset run progress when returning home.
    // AI: Helped route the reset through GameManager.ReturnToMenu.
    public void OnReturnHome() { Click(); GameManager.I.ReturnToMenu(); }

    // What: Quit the app in builds, or stop Play Mode inside the Unity Editor.
    // Human: Requested a quit button for the menu.
    // AI: Suggested the editor-safe preprocessor branch.
    public void OnQuit()
    {
        Click();
#if UNITY_EDITOR
        // Application.Quit is ignored in the Unity Editor, so use the editor API when testing.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // What: Show one title-menu subpage and hide the other title-menu pages.
    // Human: Decided which pages exist in the title menu.
    // AI: Helped make a small helper instead of repeating SetActive calls in every button.
    void ShowOnly(GameObject page)
    {
        // Used only by menu subpage buttons. Gameplay phase pages are controlled by SyncToPhase.
        if (mainPage != null) mainPage.SetActive(page == mainPage);
        if (instructionsPage != null) instructionsPage.SetActive(page == instructionsPage);
        if (creditsPage != null) creditsPage.SetActive(page == creditsPage);
    }
}
