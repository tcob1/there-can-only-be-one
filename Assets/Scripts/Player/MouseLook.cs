using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private Transform playerBody;

    [Header("Head Bob Settings")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private float bobSpeed = 10f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchCameraOffset = -0.5f;
    [SerializeField] private float crouchLerpSpeed = 10f;

    private float targetBaseY;
    private float xRotation;
    private float defaultY;
    private float baseY;
    private float bobTimer;
    private float currentBobAmount;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        defaultY = cameraTarget.localPosition.y;
        targetBaseY = defaultY;
        baseY = defaultY;
        playerMovement.OnPlayerMove += PlayerMovement_BobWhileMoving;
        playerMovement.OnCrouchToggled += PlayerMovement_OnCrouchToggled;
    }

    void Update()
    {
        HandleMouseLook();
        UpdateCrouchLerp();
    }

    private void HandleMouseLook()
    {
        float delta = Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * delta;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * delta;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void PlayerMovement_BobWhileMoving(object sender, PlayerMovement.OnPlayerMoveEventArgs e)
    {
        if (!e.isGrounded)
        {
            currentBobAmount = Mathf.Lerp(currentBobAmount, 0f, Time.deltaTime * 10f);
        }
        else
        {
            float horizontalSpeed = new Vector2(e.velocity.x, e.velocity.z).magnitude;
            float targetAmount = horizontalSpeed > 0.01f ? bobAmount : 0f;
            currentBobAmount = Mathf.Lerp(currentBobAmount, targetAmount, Time.deltaTime * 10f);
        }

        bobTimer += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTimer) * currentBobAmount;
        SetCameraTargetY(baseY + yOffset);
    }

    private void PlayerMovement_OnCrouchToggled(object sender, PlayerMovement.OnCrouchEventArgs e)
    {
        bobTimer = 0f;
        targetBaseY = e.isCrouching ? defaultY + crouchCameraOffset : defaultY;
    }

    private void UpdateCrouchLerp()
    {
        baseY = Mathf.Lerp(baseY, targetBaseY, Time.deltaTime * crouchLerpSpeed);
    }

    private void SetCameraTargetY(float y)
    {
        Vector3 pos = cameraTarget.localPosition;
        pos.y = y;
        cameraTarget.localPosition = pos;
    }
}