using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpPressed { get; private set; }

    public bool isSprinting { get; private set; }

    private InputSystem_Actions _inputSystemActions;

    private void Awake()
    {
        _inputSystemActions = new InputSystem_Actions();

        _inputSystemActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        _inputSystemActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        _inputSystemActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        _inputSystemActions.Player.Look.canceled += _ => lookInput = Vector2.zero;

        _inputSystemActions.Player.Jump.performed += _ => jumpPressed = true;
        
        _inputSystemActions.Player.Sprint.performed += _ => isSprinting = true;
        _inputSystemActions.Player.Sprint.canceled += _ => isSprinting = false;
    }

    private void OnEnable() => _inputSystemActions.Enable();
    private void OnDisable() => _inputSystemActions.Disable();

    public void ResetJump() => jumpPressed = false;
}
