using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ---------- Events ----------
    // Fired when the game starts
    public event Action OnGameStart;

    // Fired when the game ends or player dies
    public event Action OnGameOver;


    // ---------- Properties ----------
    public bool IsGameRunning { get; private set; }

    [SerializeField] public Transform playerAttackPosition;


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
    }

    public void EndGame()
    {
        if (!IsGameRunning) return;

        IsGameRunning = false;
        OnGameOver?.Invoke();
    }

}
