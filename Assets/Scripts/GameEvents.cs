using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameEvent
{
    public string id;
    public float triggerTime;
    public bool hasTriggered;
}

public class GameEventArgs : EventArgs
{
    public GameEvent Event;
    public float CurrentTime;
}

public class GameEvents : MonoBehaviour
{
    [Header("Time")]
    public float currentTime = 0f;
    public bool isRunning = true;

    [Header("Events")]
    //list events that happen here
    [SerializeField]
    private List<GameEvent> events = new List<GameEvent>
    {
        new GameEvent { id = "guard_drops_key", triggerTime = 30f  },
    };

    // Subscribe to this from any MonoBehaviour to react to events
    public static event EventHandler<GameEventArgs> OnGameEvent;

    void Update()
    {
        if (!isRunning) return;

        currentTime += Time.deltaTime;

        foreach (var gameEvent in events)
        {
            if (!gameEvent.hasTriggered && currentTime >= gameEvent.triggerTime)
            {
                TriggerEvent(gameEvent);
            }
        }
    }

    void TriggerEvent(GameEvent gameEvent)
    {
        gameEvent.hasTriggered = true;

        Debug.Log($"[GameEvents] ▶ '{gameEvent.id}' fired at t={currentTime:F1}s");

        OnGameEvent?.Invoke(this, new GameEventArgs
        {
            Event = gameEvent,
            CurrentTime = currentTime
        });
    }

    public void RewindTo(float targetTime)
    {
        currentTime = targetTime;
        foreach (var gameEvent in events)
            if (gameEvent.triggerTime >= targetTime)
                gameEvent.hasTriggered = false;

        Debug.Log($"[GameEvents] Rewound to t={targetTime}s");
    }

    public void SetPaused(bool paused) => isRunning = !paused;

    public void RegisterEvent(GameEvent newEvent)
    {
        newEvent.hasTriggered = currentTime >= newEvent.triggerTime;
        events.Add(newEvent);
    }
}