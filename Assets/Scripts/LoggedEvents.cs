using UnityEngine;

public class LoggedEvent
{
    public string eventId;
    public string description;
    public long gameTime;
    public Vector3 position;

    public LoggedEvent(string eventId, string description, long gameTime, Vector3 position)
    {
        this.eventId = eventId;
        this.description = description;
        this.gameTime = gameTime;
        this.position = position;
    }
}