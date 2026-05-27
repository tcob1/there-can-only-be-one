using System.Collections.Generic;
using UnityEngine;

public class EventLogger : MonoBehaviour
{
    public static EventLogger Instance { get; private set; }

    public List<LoggedEvent> loggedEvents = new List<LoggedEvent>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool HasBeenLogged(string id)
    {
        foreach (LoggedEvent e in loggedEvents)
        {
            if (e.eventId == id) return true;
        }
        return false;
    }

    public void Log(LoggedEvent loggedEvent)
    {
        loggedEvents.Add(loggedEvent);
        //Debug.Log($"Logged: [{loggedEvent.gameTime}] {loggedEvent.description}");
    }
}