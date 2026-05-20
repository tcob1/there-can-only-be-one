using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnGameWin;
    public event Action OnRespawn;
    public event Action OnPause;
    public event Action OnResume;

    public bool IsPaused { get; private set; }

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnGameStart = null;
        OnGameOver = null;
        OnGameWin = null;
        OnRespawn = null;

        IsGameRunning = false;

        if (scene.name == "Main")
        {
            StartGame();
        }
    }

    private void Start()
    {

    }

    public void StartGame()
    {
        if (IsGameRunning) return;
        IsGameRunning = true;
        OnGameStart?.Invoke();
        SFXManager.Instance.StopLoopingMusic();
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

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        OnPause?.Invoke();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        OnResume?.Invoke();
    }

}
