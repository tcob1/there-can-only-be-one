using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Events
    public event EventHandler<OnPlayerMoveEventArgs> OnPlayerMove;
    public class OnPlayerMoveEventArgs : EventArgs
    {
        public Vector3 velocity;
        public bool isGrounded;
    }

    public event EventHandler<OnCrouchEventArgs> OnCrouchToggled;
    public class OnCrouchEventArgs : EventArgs
    {
        public bool isCrouching;
    }

    [SerializeField] private CharacterController controller;
    [SerializeField] private float baseSpeed = 12f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1f;

    //Ground checking
    [SerializeField] private float capsuleCastRadius = 0.3f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    private bool isCrouching;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;

    // Cached values
    private float currentSpeed;
    private float sqrtJumpValue;
    private Transform cachedTransform;

    // Input caching
    private bool jumpPressed;
    private bool crouchPressed;
    private bool crouchReleased;

    private void Awake()
    {
        cachedTransform = transform;
        currentSpeed = baseSpeed;
        sqrtJumpValue = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void Update()
    {
        // Cache input at start of frame
        jumpPressed = Input.GetButtonDown("Jump");
        crouchPressed = Input.GetKeyDown(KeyCode.LeftControl);
        crouchReleased = Input.GetKeyUp(KeyCode.LeftControl);

        //Ground check
        wasGrounded = isGrounded;

        RaycastHit hit;

        // bottom and top of the capsule in world space
        Vector3 capsuleBottom = groundCheck.position;
        Vector3 capsuleTop = groundCheck.position + Vector3.up * 0.1f; // tiny height above the bottom

        //spawns capsult at origin then casts to find floor
        isGrounded = Physics.CapsuleCast(
            capsuleBottom,
            capsuleTop,
            capsuleCastRadius,
            Vector3.down,
            out hit,
            groundDistance,
            groundMask
        );


        // Sound for land/jump
        //if (isGrounded != wasGrounded)
        //{
        //    if (isGrounded)
        //        SFXManager.Instance.PlaySFX("land");
        //    else
        //        SFXManager.Instance.PlaySFX("jump");
        //}

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        //Movement input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Calculate movement direction (reuse transform cache)
        Vector3 move = cachedTransform.right * x + cachedTransform.forward * z;
        //restrict diagonal speed boost-i saw this in a yt short
        move = Vector3.ClampMagnitude(move, 1f);

        // Apply horizontal movement
        controller.Move(move * (currentSpeed * Time.deltaTime));

        // Jump
        if (jumpPressed && isGrounded)
        {
            velocity.y = sqrtJumpValue;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical movement
        controller.Move(velocity * Time.deltaTime);

        // Handle crouch input
        if (crouchPressed)
        {
            StartCrouch();
        }
        else if (crouchReleased)
        {
            StopCrouch();
        }

        // Fire event (calculate total velocity once)
        OnPlayerMove?.Invoke(this, new OnPlayerMoveEventArgs
        {
            velocity = new Vector3(move.x * currentSpeed, velocity.y, move.z * currentSpeed),
            isGrounded = isGrounded
        });
    }

    private void StartCrouch()
    {
        if (isCrouching) return;

        isCrouching = true;
        currentSpeed = baseSpeed * crouchSpeedMultiplier;

        OnCrouchToggled?.Invoke(this, new OnCrouchEventArgs { isCrouching = true });
    }

    private void StopCrouch()
    {
        if (!isCrouching) return;

        isCrouching = false;
        currentSpeed = baseSpeed;

        OnCrouchToggled?.Invoke(this, new OnCrouchEventArgs { isCrouching = false });
    }
}