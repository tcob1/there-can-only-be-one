using UnityEngine;

public class LoggedEvent
{
    public string description;
    public long gameTime;
    public Vector3 position;

    public LoggedEvent(string description, long gameTime, Vector3 position)
    {
        this.description = description;
        this.gameTime = gameTime;
        this.position = position;
    }
}