using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public class GameEvent
{
    public string id;
    // enter something like "0:1:30:0" in Inspector
    public string triggerTimeString; 
    public long triggerTime;
    public bool hasTriggered;
}

public class GameEventArgs : EventArgs
{
    public GameEvent Event;
    public long CurrentTime;
}

public class GameEvents : MonoBehaviour
{
    // where we can list the events happening throughout day
    [Header("Events")]
    [SerializeField]
    private List<GameEvent> events = new List<GameEvent>
    {
        new GameEvent { id = "guard_drops_key", triggerTime = 10007L },
    };

    // create event handler
    public static event EventHandler<GameEventArgs> OnGameEvent;

    public static GameEvents Instance;

    // events objects
    [SerializeField] private GameObject keyPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
            
    }

    void OnEnable()
    {
        //subscribe to timehub ticks
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

    // Ai generated, for ease of inspector input
    // D:HH:MM:SS to seconds
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
        Debug.Log(currentTime);

        // when going back in time reset events that haven't happened yet
        foreach (var gameEvent in events)
        {
            if (gameEvent.hasTriggered && gameEvent.triggerTime > currentTime)
            {
                gameEvent.hasTriggered = false;
                Debug.Log($"(GameEvents) '{gameEvent.id}' has been reset");
            }
        }

        // Trigger events that should have fired by now
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
        Debug.Log($"(GameEvents) '{gameEvent.id}' triggered at t={currentTime}s");

        // we can set events in this script or sub to OnGameEvent in other scripts to trigger things when certain events happen
        switch (gameEvent.id)
        {
            case "guard_drops_key":
                // not efficient for now
                GuardNav guard = FindAnyObjectByType<GuardNav>();
                if (guard != null && keyPrefab != null)
                {
                    Instantiate(keyPrefab, guard.transform.position, Quaternion.identity);
                }
                break;
        }


        OnGameEvent?.Invoke(this, new GameEventArgs
        {
            Event = gameEvent,
            CurrentTime = currentTime
        });
    }

    // add a new event from other scripts. like when certain events in time only happen under certain conditions
    public void RegisterEvent(GameEvent newEvent)
    {
        long currentTime = TimeHub.Instance.getTime();
        newEvent.hasTriggered = currentTime >= newEvent.triggerTime;
        events.Add(newEvent);
    }
}