using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerJumping : MonoBehaviour
{
    public float jumpHeight = 2.0f; // Adjust this value for desired jump height
    public float gravity = -9.81f;
    public float jumpStaminaCost = 10f;
    public float maxStamina = 100f;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    private Vector3 velocity;
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
        HandleJumping();
        ApplyGravity();
        controller.Move(velocity * Time.deltaTime); // Apply velocity to controller movement
    }

    private void HandleJumping()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        if (inputHandler.jumpInput && isGrounded && currentStamina >= jumpStaminaCost)
        {
            // Calculate jump velocity based on the desired height
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Apply jump velocity, preserving horizontal momentum
            velocity.y = jumpVelocity;
            currentStamina -= jumpStaminaCost; // Reduce stamina for jump
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            // Ensure a smooth landing
            velocity.y = Mathf.Max(velocity.y, -2f);
        }
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public void SetCurrentStamina(float stamina)
    {
        currentStamina = stamina;
    }
}
