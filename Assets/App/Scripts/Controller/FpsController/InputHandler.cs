using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller.FpsController
{
    public class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }

        public bool AttackPressed { get; private set; }
    
        public bool IsSprinting { get; private set; }

        private InputSystem_Actions m_InputSystemActions;

        private void Awake()
        {
            m_InputSystemActions = new InputSystem_Actions();

            m_InputSystemActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            m_InputSystemActions.Player.Move.canceled += _ => MoveInput = Vector2.zero;

            m_InputSystemActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            m_InputSystemActions.Player.Look.canceled += _ => LookInput = Vector2.zero;

            m_InputSystemActions.Player.Jump.performed += OnJumpOnperformed;
        
            m_InputSystemActions.Player.Sprint.performed += _ => IsSprinting = true;
            m_InputSystemActions.Player.Sprint.canceled += _ => IsSprinting = false;
        
            m_InputSystemActions.Player.Attack.performed += _ => AttackPressed = true;
            m_InputSystemActions.Player.Attack.canceled += _ => AttackPressed = false;
        }

        private void OnJumpOnperformed(InputAction.CallbackContext _)
        {
            JumpPressed = true;
        }

        private void OnEnable() => m_InputSystemActions.Enable();
        private void OnDisable() => m_InputSystemActions.Disable();

        public void ResetJump() => JumpPressed = false;
    }
}