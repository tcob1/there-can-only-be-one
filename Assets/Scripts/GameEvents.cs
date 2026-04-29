using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameEvent
{
    public string id;
    // reflects Timehub time
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
        new GameEvent { id = "guard_drops_key", triggerTime = 5000007L },
    };

    // create event handler
    public static event EventHandler<GameEventArgs> OnGameEvent;

    public static GameEvents Instance;

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