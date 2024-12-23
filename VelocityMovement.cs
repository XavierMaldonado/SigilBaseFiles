using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class VelocityMovement : MonoBehaviour
{
    public Transform playerCamera;
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float gravity = -9.81f;
    public float momentumSmoothing = 0.1f; // Smoothing factor for direction changes
    public float airControlFactor = 0.2f; // Factor to control air maneuvering

    public float maxStamina = 100f;
    public float sprintStaminaCost = 10f;
    public float staminaRegenRate = 5f;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private Vector3 horizontalVelocity; // Separate horizontal velocity
    private float currentStamina;
    private bool isGrounded = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        currentStamina = maxStamina;
    }

    void Update()
    {
        isGrounded = controller.isGrounded; // Update ground check
        HandleMovement();
        ApplyGravity();
        RegenerateStamina();
    }

    void HandleMovement()
    {
        Vector2 moveInput = inputHandler.moveInput;
        Vector3 desiredMoveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Determine speed based on sprinting states
        float targetSpeed = walkSpeed;
        if (inputHandler.sprintInput && currentStamina > 0)
        {
            targetSpeed = sprintSpeed;
            currentStamina -= sprintStaminaCost * Time.deltaTime;
        }

        // Calculate desired horizontal movement direction
        Vector3 targetHorizontalVelocity = desiredMoveDirection.normalized * targetSpeed;

        // Smoothly update horizontal velocity
        if (isGrounded)
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, Time.deltaTime / momentumSmoothing);
        }
        else
        {
            // Maintain horizontal velocity while in the air
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity * airControlFactor, Time.deltaTime / momentumSmoothing);
        }

        // Apply movement including forward velocity while in the air
        moveDirection = horizontalVelocity;
        Vector3 move = moveDirection * Time.deltaTime;
        controller.Move(move + new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }
    void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Apply gravity to vertical movement only
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
    }

    void RegenerateStamina()
    {
        if (!inputHandler.sprintInput)
        {
            currentStamina = Mathf.Clamp(currentStamina + staminaRegenRate * Time.deltaTime, 0, maxStamina);
        }
    }
}
