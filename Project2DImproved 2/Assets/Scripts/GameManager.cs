using System;
using UnityEngine;

/// Central state for Dungeon Key Run. The scene is assembled by GameBootstrap,
/// while this class owns the rules: lives, key state, pause, victory, and fail.
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum Phase { Menu, Playing, Paused, Victory, GameOver }
    public Phase phase = Phase.Menu;

    [Header("Tuning")]
    public int startLives = 3;

    public int lives;
    public int score;
    public int highScore;
    public int level = 1;
    public bool hasKey;
    public float elapsed;
    public string objective = "Find the key, avoid enemies, then reach the exit.";

    public event Action OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action OnVictory;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    void Update()
    {
        if (phase != Phase.Playing) return;

        elapsed += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Escape)) Pause();
        OnStateChanged?.Invoke();
    }

    public void StartGame()
    {
        lives = startLives;
        score = 0;
        level = 1;
        hasKey = false;
        elapsed = 0f;
        objective = "Explore the dungeon and collect the gold key.";
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void CollectKey()
    {
        if (hasKey) return;
        hasKey = true;
        objective = "Key collected. Reach the blue exit door.";
        OnStateChanged?.Invoke();
    }

    public void LoseLife()
    {
        if (phase != Phase.Playing) return;

        lives = Mathf.Max(0, lives - 1);
        objective = lives > 0
            ? "You were hit. Keep moving and reach the exit."
            : "You were defeated by the dungeon guards.";
        OnStateChanged?.Invoke();

        if (lives == 0) GameOver();
    }

    public void GainLife()
    {
        lives++;
        objective = "Life restored.";
        OnStateChanged?.Invoke();
    }

    public void AddScore(int delta)
    {
        score += delta;
        if (score > highScore) highScore = score;
        OnStateChanged?.Invoke();
    }

    public void TryExit()
    {
        if (phase != Phase.Playing) return;

        if (!hasKey)
        {
            objective = "The exit is locked. Find the gold key first.";
            OnStateChanged?.Invoke();
            return;
        }

        Victory();
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

    public void Victory()
    {
        phase = Phase.Victory;
        Time.timeScale = 0f;
        OnVictory?.Invoke();
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
