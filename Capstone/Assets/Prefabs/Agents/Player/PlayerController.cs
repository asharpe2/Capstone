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

        // ✅ Manually disable UI-related actions
        InputAction navigateAction = playerInput.actions["Navigate"];
        if (navigateAction != null) navigateAction.Disable();
    }

    public void EnableUIControls()
    {
        playerInput.SwitchCurrentActionMap("UI"); // ✅ Switch to "UI" Action Map
        playerInput.currentActionMap.Disable(); // ✅ Fully disable Player actions
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

    // Define move mappings (Input Action Name → (Move Name, Stamina Cost))
    private Dictionary<string, (string moveName, float staminaCost)> moveMap = new Dictionary<string, (string, float)>
    {
    { "Jab", ("Jab", 10f) },
    { "Straight", ("Straight", 15f) },
    { "Left_Hook", ("Left_Hook", 25f) },
    { "Right_Hook", ("Right_Hook", 25f) }
    };

    private void HandleInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "movementAction")
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.action.name == "Block")
        {
            HandleBlocking(context.performed);
        }
        else if (context.performed)
        {
            if (moveMap.TryGetValue(context.action.name, out var moveData))
            {
                ThrowPunch(moveData.moveName, moveData.staminaCost);
            }
            else
            {
                Debug.LogWarning($"Move '{context.action.name}' not found in moveMap!");
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