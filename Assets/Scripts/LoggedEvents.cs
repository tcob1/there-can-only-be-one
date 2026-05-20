using UnityEngine;

public class LoggedEvent
{
    // This class is used to store information about game events for later display in the UI.
    public string description;
    public long gameTime;
    public Vector3 position;

    public LoggedEvent(string description, long gameTime, Vector3 position)
    {
        this.description = description;
        this.gameTime = gameTime;
        // doesnt acc matter for now but feel like would be useful
        this.position = position;
    }
}