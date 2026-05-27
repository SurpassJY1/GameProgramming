using System;
using UnityEngine;

/// Central state for the Dungeon Key Run vertical slice.
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum Phase { Menu, Playing, Paused, Won, GameOver }
    public Phase phase = Phase.Menu;

    [Header("Tuning")]
    public int startLives = 3;

    public int lives;
    public bool hasKey;
    public float elapsed;
    public string objective = "Find the gold key.";

    public event Action OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnRunEnded;

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
        hasKey = false;
        elapsed = 0f;
        objective = "Find the gold key, then reach the blue exit.";
        phase = Phase.Playing;
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void CollectKey()
    {
        if (phase != Phase.Playing || hasKey) return;

        hasKey = true;
        objective = "Key collected. The blue exit is unlocked.";
        OnStateChanged?.Invoke();
    }

    public void TryExit()
    {
        if (phase != Phase.Playing) return;

        if (!hasKey)
        {
            objective = "The exit is locked. Collect the gold key first.";
            OnStateChanged?.Invoke();
            return;
        }

        phase = Phase.Won;
        objective = "Dungeon cleared.";
        Time.timeScale = 0f;
        OnRunEnded?.Invoke();
        OnStateChanged?.Invoke();
    }

    public void PlayerHit()
    {
        if (phase != Phase.Playing) return;

        lives = Mathf.Max(0, lives - 1);
        objective = lives > 0
            ? "Careful. Avoid the guards and find the exit."
            : "You were caught by the dungeon guards.";

        if (lives == 0)
        {
            phase = Phase.GameOver;
            Time.timeScale = 0f;
            OnRunEnded?.Invoke();
        }

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

    public void ReturnToMenu()
    {
        phase = Phase.Menu;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke();
    }
}
