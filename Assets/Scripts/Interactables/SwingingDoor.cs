using System.Collections;
using UnityEngine;

public class SwingingDoor : MonoBehaviour
{
    [SerializeField] private float rotationDuration = 1f;

    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closedAngle = 0f;

    private Door door;

    private Coroutine currentRotation;

    void Awake()
    {
        door = GetComponent<Door>();
        door.OnInitialized.AddListener(Initialize);
        door.OnOpen.AddListener(SwingOpen);
        door.OnClose.AddListener(SwingClosed);
    }

    void Initialize()
    {
        if (door.State == Door.DoorState.Open)
        {
            transform.rotation = Quaternion.Euler(0f, openAngle, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, closedAngle, 0f);
        }
    }

    void SwingOpen()
    {
        RotateDoor(transform.rotation.eulerAngles.y, openAngle);
    }

    void SwingClosed()
    {
        RotateDoor(transform.rotation.eulerAngles.y, closedAngle);
    }

    public void RotateDoor(float startYRotation, float targetYRotation)
    {
        // Stop any existing rotation
        if (currentRotation != null)
        {
            StopCoroutine(currentRotation);
        }

        currentRotation = StartCoroutine(RotateDoorCoroutine(startYRotation, targetYRotation));
    }

    private IEnumerator RotateDoorCoroutine(float startY, float targetY)
    {
        // Calculate shortest path
        float delta = Mathf.DeltaAngle(startY, targetY);
        float actualTarget = startY + delta;

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            // Smooth step for ease in/out
            t = t * t * (3f - 2f * t);

            float currentY = Mathf.Lerp(startY, actualTarget, t);
            transform.rotation = Quaternion.Euler(0f, currentY, 0f);

            yield return null;
        }

        // Ensure we end exactly at target rotation
        transform.rotation = Quaternion.Euler(0f, targetY, 0f);
        currentRotation = null;
    }
}
