using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private InputHandler inputHandler;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    private Vector3 _velocity;
    private bool _isGrounded;

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        _isGrounded = controller.isGrounded;
        
        float speed = moveSpeed * (inputHandler.isSprinting ?  sprintMultiplier : 1);

        Vector3 move = transform.right * inputHandler.moveInput.x + transform.forward * inputHandler.moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        if (_isGrounded && inputHandler.jumpPressed)
        {
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        inputHandler.ResetJump();
    }

    private void ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);
    }
}