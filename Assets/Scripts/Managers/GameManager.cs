using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ---------- Events ----------
    // Fired when the game starts
    public event Action OnGameStart;

    // Fired when the game ends or player dies
    public event Action OnGameOver;
    public event Action OnGameWin;

    public event Action OnRespawn;


    // ---------- Properties ----------
    public bool IsGameRunning { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (IsGameRunning) return;

        IsGameRunning = true;
        OnGameStart?.Invoke();

        SFXManager.Instance.PlayLoopingMusic("AnotherMe", 0f, 0.3f);
    }

    public void EndGame()
    {
        if (!IsGameRunning) return;
        IsGameRunning = false;
        OnGameOver?.Invoke();

    }

    public void WinGame()
    {
        if (!IsGameRunning) return;
        IsGameRunning = false;
        OnGameWin?.Invoke();
        Debug.Log("Game Won!");
        // temporary
        SFXManager.Instance.StopLoopingMusic();
        SceneManager.LoadScene("MainMenu");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        IsGameRunning = true;
        OnRespawn?.Invoke();
    }

}
