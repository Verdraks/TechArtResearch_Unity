using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody; 
    [SerializeField] private InputHandler input;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 100f;

    private float _xRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        HandleLook();
    }

    private void HandleLook()
    {
        Vector2 look = input.LookInput;
        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}