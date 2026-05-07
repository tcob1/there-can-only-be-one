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
                    lookTransform.rotation = entry.cameraRotation;
                }
            }
        }
        else
        {
            // record the player's position and rotation in the history
            movementHistory.Add(new PlayerMovementHistoryEntry(transform.position, transform.rotation, lookTransform.rotation));
        }

        currentTime++;
    }

    void RecordAction(string actionName)
    {
        actionHistory.Add(new PlayerActionHistoryEntry(actionName, TimeHub.Instance.getTime()));
    }

    void StartReplay()
    {
        isReplaying = true;
        mouseLook.isActive = false;
        playerMovement.isActive = false;
    }

    void StopReplay()
    {
        isReplaying = false;
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
        }
        // TODO: is this it???
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
            isActivePlayer = false;
        }
    }
}
