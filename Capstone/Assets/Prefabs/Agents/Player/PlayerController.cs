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
        Move(moveInput, moveSpeed);
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "movementAction")
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.action.name == "Block")
        {
            if (context.started)  // When button is first pressed
            {
                HandleBlocking(true);
            }
            else if (context.canceled)  // When button is released
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
        if (targetTransform.position.x < transform.position.x) GameManager.Instance.HandleGameOver(true);
        else GameManager.Instance.HandleGameOver(false);
    }
}