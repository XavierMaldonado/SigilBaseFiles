using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
public class RigidbodyJumpHandler : MonoBehaviour
{
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;
    public bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        CheckGroundStatus();
        HandleJump();
    }

    private void CheckGroundStatus()
    {
        // Check if the character is grounded by using a sphere cast
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleJump()
    {
        if (inputHandler.jumpInput && isGrounded)
        {
            // Apply jump force
            Debug.Log("jump");
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = 0; // Reset current vertical velocity
            rb.linearVelocity = jumpVelocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // Call the method to reset jump input
            inputHandler.ResetJumpInput();
        }
    }
}
