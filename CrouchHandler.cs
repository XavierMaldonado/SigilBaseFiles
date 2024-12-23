using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CrouchHandler : MonoBehaviour
{
    public float crouchHeight = 1.0f;  // Height when crouching
    public float standingHeight = 2.0f;  // Height when standing
    public float transitionSpeed = 5f;  // Speed of transition
    public float crouchCameraHeightOffset = 0.5f;  // Offset for camera when crouching
    public float standingCameraHeightOffset = 1.0f;  // Offset for camera when standing
    public Transform cameraTransform;  // Reference to the player's camera transform

    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private InputAction crouchAction;
    private bool isCrouching = false;  // State of crouch
    private Coroutine crouchCoroutine;  // To manage transitions

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        // Initialize PlayerInputActions and bind actions
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        crouchAction = inputActions.Player.Crouch;
    }

    void OnEnable()
    {
        crouchAction.started += OnCrouch;
    }

    void OnDisable()
    {
        crouchAction.started -= OnCrouch;
    }

    void OnCrouch(InputAction.CallbackContext context)
    {
        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
        }

        if (isCrouching)
        {
            crouchCoroutine = StartCoroutine(CrouchTransition(standingHeight, standingCameraHeightOffset));
        }
        else
        {
            crouchCoroutine = StartCoroutine(CrouchTransition(crouchHeight, crouchCameraHeightOffset));
        }

        isCrouching = !isCrouching;
    }

    IEnumerator CrouchTransition(float targetHeight, float targetCameraHeightOffset)
    {
        float startHeight = characterController.height;
        Vector3 startCenter = characterController.center;
        float startCameraY = cameraTransform.localPosition.y;

        float endCameraY = (targetHeight / 2) + targetCameraHeightOffset;  // Target camera height based on crouch or stand state
        Vector3 endCenter = new Vector3(0, targetHeight / 2, 0);

        float elapsedTime = 0f;

        while (elapsedTime < transitionSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionSpeed;

            // Smooth transition for character controller
            characterController.height = Mathf.Lerp(startHeight, targetHeight, t);
            characterController.center = Vector3.Lerp(startCenter, endCenter, t);

            // Smooth transition for camera
            cameraTransform.localPosition = new Vector3(
                cameraTransform.localPosition.x,
                Mathf.Lerp(startCameraY, endCameraY, t),
                cameraTransform.localPosition.z
            );

            yield return null;
        }

        // Ensure final values are set
        characterController.height = targetHeight;
        characterController.center = endCenter;
        cameraTransform.localPosition = new Vector3(
            cameraTransform.localPosition.x,
            endCameraY,
            cameraTransform.localPosition.z
        );
    }
}
