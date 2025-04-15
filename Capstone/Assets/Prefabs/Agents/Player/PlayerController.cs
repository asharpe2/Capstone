using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class PlayerController : Agent
{
    private bool inputLocked = true; // Lock input initially

    private PlayerInput playerInput;
    private Vector2 moveInput;
    public Vector3 initialPosition;

    [Header("Player Settings")]
    public float moveSpeed = 2f;

    public float fade;

    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
        // Lock input until explicitly unlocked after transition.
        inputLocked = true;
    }

    public void ResetPlayer()
    {
        health = maxHealth;
        stamina = maxStamina;

        Rigidbody rb1 = GetComponent<Rigidbody>();
        // Zero out velocities to ensure no residual movement
        rb1.velocity = Vector3.zero;
        rb1.angularVelocity = Vector3.zero;

        // Set the transform's position instead of the rigidbody's position property.
        transform.position = initialPosition;
    }


    public void EnableGameplayControls()
    {
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void EnableUIControls()
    {
        playerInput.SwitchCurrentActionMap("UI"); // ✅ Switch to "UI" Action Map
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += HandleInput;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= HandleInput;
    }

    private void FixedUpdate()
    {
        Move(moveInput, moveSpeed);
    }

    // Call this after the transition delay to allow input.
    public void UnlockInput()
    {
        inputLocked = false;
        playerInput.SwitchCurrentActionMap("Player");
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        // Early exit if input is locked.
        if (inputLocked)
            return;

        if (context.action.name == "movementAction")
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.action.name == "Block")
        {
            if (context.started)
            {
                HandleBlocking(true);
            }
            else if (context.canceled)
            {
                HandleBlocking(false);
            }
        }
        else if (context.performed)
        {
            if (context.action.name == "Jab")
            {
                ThrowPunch("Jab", 5f);
            }
            else if (context.action.name == "Straight")
            {
                ThrowPunch("Straight", 10f);
            }
            else if (context.action.name == "Left_Hook")
            {
                Debug.Log("Throwing Left Hook");
                ThrowPunch("Left_Hook", 15f);
            }
            else if (context.action.name == "Right_Hook")
            {
                ThrowPunch("Right_Hook", 15f);
            }
        }
    }

    protected override void OnDeath()
    {
        if (targetTransform.position.x < transform.position.x) StartScreenManager.Instance.HandleGameOver(true);
        else StartScreenManager.Instance.HandleGameOver(false);
    }
}