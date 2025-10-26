using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [FormerlySerializedAs("controller")]
    [Header("References")]
    [SerializeField] private CharacterController m_Controller;
    [FormerlySerializedAs("inputHandler")] [SerializeField] private InputHandler m_InputHandler;

    [FormerlySerializedAs("moveSpeed")]
    [Header("Settings")]
    [SerializeField] private float m_MoveSpeed = 5f;
    [FormerlySerializedAs("sprintMultiplier")] [SerializeField] private float m_SprintMultiplier = 1.5f;
    [FormerlySerializedAs("jumpForce")] [SerializeField] private float m_JumpForce = 5f;
    [FormerlySerializedAs("gravity")] [SerializeField] private float m_Gravity = -9.81f;

    private Vector3 m_Velocity;
    private bool m_IsGrounded;

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        m_IsGrounded = m_Controller.isGrounded;
        
        float speed = m_MoveSpeed * (m_InputHandler.IsSprinting ?  m_SprintMultiplier : 1);

        Vector3 move = transform.right * m_InputHandler.MoveInput.x + transform.forward * m_InputHandler.MoveInput.y;
        m_Controller.Move(move * (speed * Time.deltaTime));

        if (m_IsGrounded && m_InputHandler.JumpPressed)
        {
            m_Velocity.y = Mathf.Sqrt(m_JumpForce * -2f * m_Gravity);
        }

        m_InputHandler.ResetJump();
    }

    private void ApplyGravity()
    {
        if (m_IsGrounded && m_Velocity.y < 0)
            m_Velocity.y = -2f;

        m_Velocity.y += m_Gravity * Time.deltaTime;
        m_Controller.Move(m_Velocity * Time.deltaTime);
    }
}