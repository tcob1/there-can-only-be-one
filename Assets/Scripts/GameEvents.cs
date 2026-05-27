using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public class GameEvent
{
    public string id;
    public string description; // human readable, set in Inspector
    // enter something like "0:1:30:0" in Inspector
    public string triggerTimeString;
    public long triggerTime;
    public bool hasTriggered;
}

public class GameEventArgs : EventArgs
{
    public GameEvent Event;
    public long CurrentTime;
    public Vector3 Position;
}

public class GameEvents : MonoBehaviour
{
    [Header("Events")]
    [SerializeField]
    private List<GameEvent> events = new List<GameEvent>
    {
        new GameEvent { id = "guard_drops_key", description = "Guard drops key", triggerTime = 10007L },
    };

    public static event EventHandler<GameEventArgs> OnGameEvent;
    public static GameEvents Instance;

    [SerializeField] private GameObject keyPrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        TimeHub.onSecond += OnSecondTick;
    }

    void OnDisable()
    {
        TimeHub.onSecond -= OnSecondTick;
    }

    void Start()
    {
        foreach (var gameEvent in events)
        {
            gameEvent.triggerTime = ParseTime(gameEvent.triggerTimeString);
        }
    }

    public static long ParseTime(string time)
    {
        string[] parts = time.Split(':');

        long day = parts.Length > 3 ? long.Parse(parts[parts.Length - 4]) : 0;
        long hour = parts.Length > 2 ? long.Parse(parts[parts.Length - 3]) : 0;
        long minute = parts.Length > 1 ? long.Parse(parts[parts.Length - 2]) : 0;
        long second = long.Parse(parts[parts.Length - 1]);

        return (day * 86400) + (hour * 3600) + (minute * 60) + second;
    }

    private void OnSecondTick()
    {
        long currentTime = TimeHub.Instance.getTime();

        foreach (var gameEvent in events)
        {
            if (gameEvent.hasTriggered && gameEvent.triggerTime > currentTime)
            {
                gameEvent.hasTriggered = false;
                Debug.Log($"(GameEvents) '{gameEvent.id}' has been reset");
            }
        }

        foreach (var gameEvent in events)
        {
            if (!gameEvent.hasTriggered && currentTime >= gameEvent.triggerTime)
            {
                TriggerEvent(gameEvent, currentTime);
            }
        }
    }

    void TriggerEvent(GameEvent gameEvent, long currentTime)
    {
        gameEvent.hasTriggered = true;
        Vector3 eventPosition = Vector3.zero;
        bool eventSuccess = true;

        switch (gameEvent.id)
        {
            case "guard_drops_key":
                GuardNav guard = FindAnyObjectByType<GuardNav>();
                if (guard != null && keyPrefab != null)
                {
                    eventPosition = guard.transform.position;
                    Instantiate(keyPrefab, eventPosition, Quaternion.identity);
                }
                else
                {
                    eventSuccess = false;
                }
                break;
        }

        if (eventSuccess)
        {
            OnGameEvent?.Invoke(this, new GameEventArgs
            {
                Event = gameEvent,
                CurrentTime = currentTime,
                Position = eventPosition
            });
        }
    }

    public void RegisterEvent(GameEvent newEvent)
    {
        long currentTime = TimeHub.Instance.getTime();
        newEvent.hasTriggered = currentTime >= newEvent.triggerTime;
        events.Add(newEvent);
    }
}