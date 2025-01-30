using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    private Vector2 moveInput;
    private Vector2 blockDirection;
    private bool isBlocking;

    public float moveSpeed = 5f;

    private float maxHealth = 100;
    public float health;

    private float maxStamina = 100;
    public float stamina;
    private bool isRegeneratingStamina = false;

    public Transform blockIndicator; // Optional visual indicator for direction
    public GameObject blockLarge; // Assign "Block_Large" object in Inspector
    public GameObject blockSmall; // Assign "Block_Small" object in Inspector

    public float blockAngleThreshold = 0.1f; // Minimum input for MoveBlock

    private void Start()
    {
        health = maxHealth;
        stamina = maxStamina;
    }


    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Block.performed += ctx => StartBlocking();
        controls.Player.Block.canceled += ctx => StopBlocking();

        controls.Player.MoveBlock.performed += ctx => blockDirection = ctx.ReadValue<Vector2>();
        controls.Player.MoveBlock.canceled += ctx => blockDirection = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        Move();
        //UpdateBlockDirection();
    }

    private void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    private void StartBlocking()
    {
        StopCoroutine(nameof(RegenerateStamina));

        if (stamina <= 0)
        {
            return;
        }
        isBlocking = true;
        blockLarge.SetActive(true);
        blockSmall.SetActive(false);
    }

    private void StopBlocking()
    {
        isBlocking = false;
        blockLarge.SetActive(false);
        blockSmall.SetActive(false);

        StartCoroutine(RegenerateStamina());
    }

    public void TakeHealthDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeStaminaDamage(int damage)
    {
        stamina -= damage;
        if (stamina <= 0)
        {
            StopBlocking();
        }
    }

    private IEnumerator RegenerateStamina(float waitTime = 2f)
    {
        Debug.Log("Starting Regen");
        // Wait for 2 seconds before regeneration starts
        yield return new WaitForSeconds(waitTime);

        isRegeneratingStamina = true;

        // Regenerate stamina at a rate of 1 per frame
        while (stamina < maxStamina)
        {
            stamina += 0.1f;
            Debug.Log($"Regenerating Stamina: {stamina}/{maxStamina}");
            yield return null; // Wait for the next frame
        }

        isRegeneratingStamina = false;
    }
}
