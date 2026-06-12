using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameEvent
{
    public string id;
    public string description;
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
    new GameEvent { id = "guard_drops_key", description = "Guard drops key", triggerTimeString = "0:0:10" },
    new GameEvent { id = "guard_walk_to_safe", description = "Guard walks to safe", triggerTimeString = "0:0:30" },
    new GameEvent { id = "guard_opens_safe", description = "Guard opens safe", triggerTimeString = "0:0:40" },
    new GameEvent { id = "guard_closes_safe", description = "Guard closes safe", triggerTimeString = "0:0:50" },
};

    public static event EventHandler<GameEventArgs> OnGameEvent;
    public static GameEvents Instance;

    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform safeDoor;
    [SerializeField] private Transform safeInteractPosition;

    // track which guard opened the safe so same one closes it
    private GuardNav safeGuard;
    private GuardNav.GuardState safeGuardPreviousState;
    private Vector3 safeGuardPreviousDestination;

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
            gameEvent.triggerTime = ParseTime(gameEvent.triggerTimeString);
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
                gameEvent.hasTriggered = false;
        }

        foreach (var gameEvent in events)
        {
            if (!gameEvent.hasTriggered && currentTime >= gameEvent.triggerTime)
                TriggerEvent(gameEvent, currentTime);
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
                GuardNav dropGuard = FindAnyObjectByType<GuardNav>();
                if (dropGuard != null && keyPrefab != null)
                {
                    eventPosition = dropGuard.transform.position;
                    Instantiate(keyPrefab, eventPosition, Quaternion.identity);
                }
                else eventSuccess = false;
                break;

            case "guard_walk_to_safe":
                if (safeDoor == null || safeInteractPosition == null)
                {
                    eventSuccess = false;
                    break;
                }

                safeGuard = FindAnyObjectByType<GuardNav>();
                if (safeGuard == null)
                {
                    eventSuccess = false;
                    break;
                }

                safeGuardPreviousState = safeGuard.currentGuardState;
                safeGuardPreviousDestination = safeGuard.transform.position;

                // disable AI, send to safe
                safeGuard.enabled = false;
                safeGuard.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(safeInteractPosition.position);
                eventPosition = safeDoor.position + new Vector3(2, 0, 0);
                break;

            case "guard_opens_safe":
                if (safeDoor == null) { eventSuccess = false; break; }
                safeDoor.gameObject.SetActive(false);
                eventPosition = safeDoor.position;
                break;

            case "guard_closes_safe":
                if (safeDoor == null) { eventSuccess = false; break; }
                safeDoor.gameObject.SetActive(true);

                // re-enable AI and resume patrol
                if (safeGuard != null)
                {
                    safeGuard.enabled = true;
                    safeGuard.currentGuardState = safeGuardPreviousState;
                    safeGuard.GetComponent<UnityEngine.AI.NavMeshAgent>()
                        .SetDestination(safeGuardPreviousDestination);
                    safeGuard = null;
                }
                eventPosition = safeDoor.position + new Vector3(0, 0, 2);
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