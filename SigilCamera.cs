using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public float rotationSmoothTime = 0.05f;

    private PlayerInputHandler inputHandler;
    private float xRotation = 0f;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    void Start()
    {
        inputHandler = playerBody.GetComponent<PlayerInputHandler>();
        if (inputHandler == null)
        {
            Debug.LogError("PlayerInputHandler not found on player body.");
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        Vector2 lookInput = inputHandler.lookInput;
        Vector2 targetMouseDelta = new Vector2(lookInput.x, lookInput.y) * mouseSensitivity * Time.deltaTime;

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, rotationSmoothTime);

        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * currentMouseDelta.x);
    }
}