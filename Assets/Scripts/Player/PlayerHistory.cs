using UnityEngine;
using System.Collections.Generic;

public class PlayerHistory : MonoBehaviour
{
    private class PlayerMovementHistoryEntry
    {
        public Vector3 position;
        public Quaternion rotation;

        public PlayerMovementHistoryEntry(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    private class PlayerActionHistoryEntry
    {
        public string actionName;
        public float timestamp;

        public PlayerActionHistoryEntry(string actionName, float timestamp)
        {
            this.actionName = actionName;
            this.timestamp = timestamp;
        }
    }

    private List<PlayerMovementHistoryEntry> movementHistory;
    private List<PlayerActionHistoryEntry> actionHistory;
    private long currentTime;

    public bool isReplaying { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movementHistory = new();
        actionHistory = new();
        currentTime = 0;
        TimeHub.onTimeChange += OnTimeTravel;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isReplaying)
        {
            // find the current time in the history and set the player's position and rotation to match
            // if the current time is past the end of the history, stop replaying
            // if the current time is before the start of the history, do nothing (TODO: should deactivate player)
            if (currentTime > movementHistory.Count - 1)
            {
                StopReplay();
            }
            else if (currentTime >= 0)
            {
                PlayerMovementHistoryEntry entry = movementHistory[(int)currentTime];
                if (entry != null)
                {
                    transform.position = entry.position;
                    transform.rotation = entry.rotation;
                }
            }
        }
        else
        {
            // record the player's position and rotation in the history
            movementHistory.Add(new PlayerMovementHistoryEntry(transform.position, transform.rotation));
        }

        currentTime++;
    }

    void RecordAction(string actionName)
    {
        actionHistory.Add(new PlayerActionHistoryEntry(actionName, TimeHub.Instance.getTime()));
    }

    void StartReplay()
    {
        Debug.Log("Starting replay at time " + currentTime);
        isReplaying = true;
    }

    void StopReplay()
    {
        isReplaying = false;
    }

    void OnTimeTravel(int delta, long newTime)
    {
        // if time is in the past, start replaying
        if (delta < 0)
        {
            Debug.Log("Time traveled backwards by " + (-delta) + " seconds to time " + newTime);
            Debug.Log("Current time: " + currentTime);
            currentTime += delta * (long)(1.0f / Time.fixedDeltaTime);
            Debug.Log("New current time: " + currentTime);
            StartReplay();
        }
        // TODO: is this it???
    }
}
