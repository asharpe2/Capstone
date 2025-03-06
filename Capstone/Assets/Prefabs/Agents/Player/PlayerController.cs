using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class PlayerController : Agent
{
    private PlayerInput playerInput;
    private Vector2 moveInput;

    [Header("Player Settings")]
    public float moveSpeed = 2f;

    public float fade;

    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
        EnableGameplayControls();
        targetTransform = GameObject.FindWithTag("Enemy").transform;
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

    private void Update()
    {
        base.Update();
        HandleMovement();
        EnableGameplayControls();
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "movementAction")
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.action.name == "Block")
        {
            if (context.performed)
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
                ThrowPunch("Jab", 10f);
            }
            else if (context.action.name == "Straight")
            {
                ThrowPunch("Straight", 15f);
            }
            else if (context.action.name == "Left_Hook")
            {
                ThrowPunch("Left_Hook", 25f);
            }
            else if (context.action.name == "Right_Hook")
            {
                ThrowPunch("Right_Hook", 25f);
            }
        }
    }


    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize(); // normalize only if needed

        Vector3 localDirection = transform.TransformDirection(inputDirection);

        Move(localDirection, moveSpeed);
    }

    protected override void OnDeath()
    {
        GameManager.Instance.HandleGameOver(false);
    }
}