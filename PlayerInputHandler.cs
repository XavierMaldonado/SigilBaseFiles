using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;
    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool crouchInput { get; private set; }
    public bool sprintInput { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Jump.canceled += OnJump;
        inputActions.Player.Crouch.performed += OnCrouch;
        inputActions.Player.Crouch.canceled += OnCrouch;
        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Jump.canceled -= OnJump;
        inputActions.Player.Crouch.performed -= OnCrouch;
        inputActions.Player.Crouch.canceled -= OnCrouch;
        inputActions.Player.Sprint.performed -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;
        inputActions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        crouchInput = context.ReadValueAsButton();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        sprintInput = context.ReadValueAsButton();
    }

    public void ResetJumpInput()
    {
        jumpInput = false;
    }
}
