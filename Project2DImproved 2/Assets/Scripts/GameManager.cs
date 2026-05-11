using System;
using UnityEngine;

/// Central game state. One-and-only instance is created by GameBootstrap.
/// Other scripts read/mutate state through GameManager.I and subscribe to
/// the events for HUD updates.
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum Phase { Menu, Playing, Paused, GameOver }
    public Phase phase = Phase.Menu;

    [Header("Tuning")]
    public int startLives = 3;
    public float secondsPerLevel = 25f;     // auto-advance difficulty every N seconds

    // Live values
    public int score;
    public int highScore;
    public int lives;
    public int level = 1;
    public float elapsed;

    // Events for HUD subscribers
    public event Action OnStateChanged;     // score / lives / level / phase changes
    public event Action OnGameStarted;
    public event Action OnGameOver;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (phase != Phase.Playing) return;

        elapsed += Time.deltaTime;
        int newLevel = 1 + Mathf.FloorToInt(elapsed / secondsPerLevel);
        if (newLevel != level)
        {
            level = newLevel;
            OnStateChanged?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) Pause();
    }

    public void StartGame()
    {
        score = 0;
        lives = startLives;
        level = 1;
        elapsed = 0f;
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void AddScore(int delta)
    {
        score += delta;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        OnStateChanged?.Invoke();
    }

    public void LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        OnStateChanged?.Invoke();
        if (lives == 0) GameOver();
    }

    public void GainLife()
    {
        lives++;
        OnStateChanged?.Invoke();
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

    public void GameOver()
    {
        phase = Phase.GameOver;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void ReturnToMenu()
    {
        phase = Phase.Menu;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke();
    }
}
