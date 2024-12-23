using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class SigilMovement : MonoBehaviour
{
    public Transform playerCamera;
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f;
    public float gravity = -9.81f;
    public float groundMomentumSmoothing = 0.1f; // Smoothing factor for ground movement
    public float airMomentumSmoothing = 0.2f; // Smoothing factor for air movement
    public float airControlFactor = 0.2f; // Factor to control air maneuvering
    public float jumpForce = 10f; // Jump force

    public float maxStamina = 100f;
    public float sprintStaminaCost = 10f;
    public float staminaRegenRate = 5f;

    public float landingImpactHeightReduction = 0.1f; // Amount to reduce height on landing
    public float landingImpactDuration = 0.1f; // Duration of the landing impact effect
    public float landingImpactSmoothing = 0.1f; // Smoothing factor for height transition
    public float landingReductionSmoothing = 0.1f; // Smoothing factor for reduction transition
    public float airTimeThreshold = 0.1f; // Time in the air required to trigger landing effect
    public float movementSmoothing = 0.1f; // Smoothing factor for movement initiation
    public float landingVelocityPenalty = 0.5f; // Penalty to apply to velocity on landing

    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    private Vector3 velocity;
    private Vector3 horizontalVelocity; // Separate horizontal velocity
    private float currentStamina;
    private bool isGrounded = false;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool wasGroundedLastFrame = true;
    private Coroutine landingImpactCoroutine;
    private float airTime = 0f;

    private float originalCameraHeight = 2f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        currentStamina = maxStamina;

        // Set initial camera height
        controller.center = new Vector3(controller.center.x, controller.height / 2, controller.center.z);
    }

    void Update()
    {
        isGrounded = controller.isGrounded; // Update ground check
        HandleMovement();
        ApplyGravity();
        RegenerateStamina();

        if (!isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            airTime = 0f;
        }
    }

    void HandleMovement()
    {
        Vector2 moveInput = inputHandler.moveInput;
        Vector3 desiredMoveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Determine speed based on sprinting states
        float targetSpeed = walkSpeed;
        if (inputHandler.sprintInput && !isCrouching && currentStamina > 0)
        {
            isSprinting = true;
            targetSpeed = sprintSpeed;
            currentStamina -= sprintStaminaCost * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
        }

        if (inputHandler.crouchInput && !isSprinting && currentStamina > 0)
        {
            isCrouching = true;
            targetSpeed = crouchSpeed;
        }
        else
        {
            isCrouching = false;
        }
        
        // Calculate desired horizontal movement direction
        Vector3 targetHorizontalVelocity = desiredMoveDirection.normalized * targetSpeed;

        if (isGrounded)
        {
            // Smoothly update horizontal velocity on the ground
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, Time.deltaTime / groundMomentumSmoothing);
        }
        else
        {
            // Smoothly update horizontal velocity in the air with less control
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity * airControlFactor, Time.deltaTime / airMomentumSmoothing);
        }

        // Apply movement including forward velocity while in the air
        Vector3 move = Vector3.Lerp(Vector3.zero, horizontalVelocity * Time.deltaTime, movementSmoothing);
        controller.Move(move + new Vector3(0, velocity.y, 0) * Time.deltaTime);

        // Handle jumping
        if (inputHandler.jumpInput && isGrounded)
        {
            // Apply jump force
            velocity.y = jumpForce;
            inputHandler.ResetJumpInput(); // Reset jump input
        }

        // Handle slope sliding
        if (isGrounded && OnSteepSlope(out Vector3 slopeDirection))
        {
            // Apply sliding force along the slope direction
            controller.Move(slopeDirection * Time.deltaTime);
        }
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

        // Handle landing impact effect if the player was in the air for more than the threshold
        if (isGrounded && !wasGroundedLastFrame && airTime >= airTimeThreshold)
        {
            if (landingImpactCoroutine != null)
            {
                StopCoroutine(landingImpactCoroutine);
            }
            landingImpactCoroutine = StartCoroutine(LandingImpact());

            // Apply landing velocity penalty
            horizontalVelocity *= landingVelocityPenalty;
        }

        wasGroundedLastFrame = isGrounded;
    }

    void RegenerateStamina()
    {
        if (!inputHandler.sprintInput)
        {
            currentStamina = Mathf.Clamp(currentStamina + staminaRegenRate * Time.deltaTime, 0, maxStamina);
        }
    }

    bool OnSteepSlope(out Vector3 slopeDirection)
    {
        slopeDirection = Vector3.zero;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height / 2 + 0.1f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > controller.slopeLimit)
            {
                slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized * (slopeAngle / 90);
                return true;
            }
        }

        return false;
    }

    IEnumerator LandingImpact()
    {
        float originalHeight = controller.height;
        float airTimeRatio = Mathf.Clamp01(airTime / airTimeThreshold);
        float adjustedReduction = landingImpactHeightReduction * airTimeRatio;
        float reducedHeight = originalHeight - adjustedReduction;
        float reducedCameraHeight = originalCameraHeight - adjustedReduction;

        // Smoothly reduce the height
        float elapsedTime = 0f;
        while (elapsedTime < landingReductionSmoothing)
        {
            controller.height = Mathf.Lerp(originalHeight, reducedHeight, elapsedTime / landingReductionSmoothing);
            controller.center = new Vector3(controller.center.x, controller.height / 2, controller.center.z);
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, Mathf.Lerp(originalCameraHeight, reducedCameraHeight, elapsedTime / landingReductionSmoothing), playerCamera.localPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the height is set to reduced after smoothing
        controller.height = reducedHeight;
        controller.center = new Vector3(controller.center.x, reducedHeight / 2, controller.center.z);
        playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, reducedCameraHeight, playerCamera.localPosition.z);

        // Wait for the duration of the impact
        yield return new WaitForSeconds(landingImpactDuration);

        // Smoothly transition back to the original height
        elapsedTime = 0f;
        while (elapsedTime < landingImpactSmoothing)
        {
            controller.height = Mathf.Lerp(reducedHeight, originalHeight, elapsedTime / landingImpactSmoothing);
            controller.center = new Vector3(controller.center.x, controller.height / 2, controller.center.z);
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, Mathf.Lerp(reducedCameraHeight, originalCameraHeight, elapsedTime / landingImpactSmoothing), playerCamera.localPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the height is set to original after smoothing
        controller.height = originalHeight;
        controller.center = new Vector3(controller.center.x, originalHeight / 2, controller.center.z);
        playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, originalCameraHeight, playerCamera.localPosition.z);
    }
}
