using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private Transform playerBody;

    [Header("Head Bob Settings")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private float bobSpeed = 10f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchCameraOffset = -0.5f;
    [SerializeField] private float crouchLerpSpeed = 10f;
    private float targetBaseY;


    private float xRotation;
    //starting Y position of the camera
    private float defaultY;
    // current base Y position of the camera
    private float baseY;
    private float bobTimer;
    private float currentBobAmount;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        defaultY = cameraTransform.localPosition.y;
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

    //Mouse look
    private void HandleMouseLook()
    {
        float delta = Time.deltaTime;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * delta;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * delta;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    //Bobbin
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
        SetCameraY(baseY + yOffset);
    }

    //replaced with currentBobAmount = Mathf.Lerp(currentBobAmount, 0f, Time.deltaTime * 10f); a smooth lerp back to height
    //private void ResetBob()
    //{
    //    bobTimer = 0f;
    //    //lowkey setting Y for all funcs right here
    //    SetCameraY(baseY);
    //}

    //Crouch
    private void PlayerMovement_OnCrouchToggled(object sender, PlayerMovement.OnCrouchEventArgs e)
    {
        bobTimer = 0f;

        targetBaseY = e.isCrouching ? defaultY + crouchCameraOffset : defaultY;
    }


    private void UpdateCrouchLerp()
    {
        baseY = Mathf.Lerp(baseY, targetBaseY, Time.deltaTime * crouchLerpSpeed);
    }


    //Camera support func
    private void SetCameraY(float y)
    {
        Vector3 pos = cameraTransform.localPosition;
        pos.y = y;
        cameraTransform.localPosition = pos;
    }
}
