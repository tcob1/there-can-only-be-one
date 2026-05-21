using UnityEngine;
using System.Collections.Generic;

public class PlayerHistory : MonoBehaviour
{
    private class PlayerMovementHistoryEntry
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion cameraRotation;

        public PlayerMovementHistoryEntry(Vector3 position, Quaternion rotation, Quaternion cameraRotation)
        {
            this.position = position;
            this.rotation = rotation;
            this.cameraRotation = cameraRotation;
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

    public Transform lookTransform;
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;
    public GameObject playerCamera;

    private List<PlayerMovementHistoryEntry> movementHistory;
    private List<PlayerActionHistoryEntry> actionHistory;
    private long currentTime;
    private bool isActivePlayer = true;
    private bool isOutsideOfTimeRange = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start called!");
        movementHistory = new();
        actionHistory = new();
        currentTime = 0;
        TimeHub.onTimeChange += OnTimeTravel;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isActivePlayer)
        {
            // record the player's position and rotation in the history
            movementHistory.Add(new PlayerMovementHistoryEntry(transform.position, transform.rotation, lookTransform.rotation));
        }
        else
        {
            // Player movement is currently being replayed
            // Find the current time in the history and set the player's
            // position and rotation to match.
            // If the current time is past the end of the history, the player
            // time travelled back at this point, so despawn them.
            // If the current time is before the start of the history, the
            // player has not yet time traveled back to this point, so despawn
            // them. They will become active in later loops.
            if (currentTime > movementHistory.Count - 1)
            {
                isOutsideOfTimeRange = true;
                Despawn();
            }
            else if (currentTime >= 0)
            {
                if (isOutsideOfTimeRange)
                {
                    // Player has just entered the replay zone and should be spawned in
                    Spawn();
                }
                PlayerMovementHistoryEntry entry = movementHistory[(int)currentTime];
                if (entry != null)
                {
                    transform.position = entry.position;
                    transform.rotation = entry.rotation;
                    lookTransform.rotation = entry.cameraRotation;
                }
            }
        }

        currentTime++;
    }

    void RecordAction(string actionName)
    {
        actionHistory.Add(new PlayerActionHistoryEntry(actionName, currentTime));
    }

    void StartReplay()
    {
        isActivePlayer = false;
        mouseLook.isActive = false;
        playerMovement.isActive = false;
    }

    void OnTimeTravel(int delta, long newTime)
    {
        Destroy(playerCamera);
        // if time is in the past, start replaying
        if (delta < 0)
        {
            CreateDuplicate();
            currentTime += delta * (long)(1.0f / Time.fixedDeltaTime);
            StartReplay();
            if (currentTime < 0)
            {
                // before the player spawned in
                isOutsideOfTimeRange = true;
                Despawn();
            }
            else if (isOutsideOfTimeRange && currentTime < movementHistory.Count)
            {
                // the player was past the time range but has moved back into it
                isOutsideOfTimeRange = false;
                Spawn();
            }
        }
    }

    void CreateDuplicate()
    {
        if (isActivePlayer)
        {
            Debug.Log("Creating duplicate player for replay");
            // create a duplicate of the player at the current position and rotation, and set it to replay the history
            GameObject duplicate = Instantiate(gameObject, transform.position, transform.rotation);
            // destroy the camera so it uses the duplicate's camera, which is the new main player
            Destroy(playerCamera);
        }
    }

    void Despawn()
    {
        // Deactivate all children
        // Cannot deactivate self because that would stop this script from running,
        // and this script needs to re-spawn the player.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Spawn()
    {
        // Reactivate children, will add effects later
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
