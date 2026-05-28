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
        isActivePlayer = true;
        isOutsideOfTimeRange = false;
        TimeHub.onTimeChange += OnTimeTravel;
        TimeHub.onTimeTravelForwardEnd += OnForwardTimeTravelEnd;
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
            if (!isOutsideOfTimeRange && currentTime > movementHistory.Count - 1)
            {
                isOutsideOfTimeRange = true;
                Despawn();
            }
            else if (currentTime >= 0 && currentTime <= movementHistory.Count - 1)
            {
                if (isOutsideOfTimeRange)
                {
                    // Player has just entered the replay zone and should be spawned in
                    isOutsideOfTimeRange = false;
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

    void OnTimeTravel(int delta, long newTime)
    {
        if (delta == 0)
        {
            return;
        }

        if (isActivePlayer)
        {
            CreateDuplicate();
            Destroy(playerCamera);
            isActivePlayer = false;
            mouseLook.isActive = false;
            playerMovement.isActive = false;
        }

        if (delta < 0)
        {
            int timestepAdjusted = delta * (int)Mathf.Round((float)(1.0f / Time.fixedDeltaTime));
            currentTime += timestepAdjusted;
            Debug.Log($"Time travel detected! Delta: {delta}, Timestep Adjusted: {timestepAdjusted}, New Time: {currentTime}");
        }
        else if (delta > 0)
        {
            // Forward time travel gets simulated through fixed update so don't add to current time.
            Debug.Log($"Time travel detected! Delta: {delta}");
        }

        if (currentTime < 0 || currentTime > movementHistory.Count - 1)
        {
            // before the player spawned in or after they time travelled out
            isOutsideOfTimeRange = true;
            Despawn();
        }
        else if (isOutsideOfTimeRange)
        {
            // the player was past the time range but has moved back into it
            isOutsideOfTimeRange = false;
            Spawn();
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
        Debug.Log("Player despawned");
    }

    void Spawn()
    {
        // Reactivate children, will add effects later
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        Debug.Log("Player spawned");
    }

    void OnForwardTimeTravelEnd(long newTime)
    {
        // TODO: disable movement while time traveling forward
        // After the player time travels forward, their history must be reset.
        // During the forward time travel, they will have accumulated a lot of
        // history that they were not in existence for, so this is where we
        // remove that.
        if (isActivePlayer)
        {
            movementHistory = new();
            actionHistory = new();
            currentTime = 0;
        }
    }
}
