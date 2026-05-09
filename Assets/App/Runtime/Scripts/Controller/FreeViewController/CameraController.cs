using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Controller.FreeViewController
{
    public class CameraController : MonoBehaviour
    {
        // Movement
        [Header("Movement")]
        public float MoveSpeed = 5f;            // base movement speed (units / second)
        public float BoostMultiplier = 2.5f;    // multiplier when boost key is held

        // Mouse look
        [Header("Mouse Look")]
        public float MouseSensitivity = 3.5f;   // mouse sensitivity multiplier
        public float MaxPitch = 89f;            // clamp for looking up/down

        [Header("Input System")]
        [SerializeField] private InputActionAsset m_InputActions;
        [SerializeField] private string m_ActionMap = "Player";
        [SerializeField] private string m_MoveActionName = "Move";   // Vector2 (x=right, y=forward)
        [SerializeField] private string m_LookActionName = "Look";   // Vector2 (x=mouseX, y=mouseY)
        [SerializeField] private string m_UpActionName = "Up";       // Button
        [SerializeField] private string m_DownActionName = "Down";   // Button
        [SerializeField] private string m_BoostActionName = "Boost"; // Button

        // Smoothing
        [Header("Smoothing")]
        [Tooltip("Time (in seconds) for movement smoothing. Small = snappy, larger = smoother/slower to respond.")]
        public float MoveSmoothTime = 0.08f;
        [Tooltip("Time (in seconds) for rotation smoothing. Small = snappy, larger = smoother/slower to respond.")]
        public float RotationSmoothTime = 0.05f;

        // runtime action refs
        private InputAction m_MoveAction;
        private InputAction m_LookAction;
        private InputAction m_UpAction;
        private InputAction m_DownAction;
        private InputAction m_BoostAction;

        // Internal state
        private float m_Yaw;   // rotation around Y (target)
        private float m_Pitch; // rotation around X (target)
        private bool m_IsCursorLocked;

        // Smoothed state
        private float m_SmoothYaw;    // current smoothed yaw
        private float m_SmoothPitch;  // current smoothed pitch
        private float m_YawVelocity;  // used by SmoothDampAngle
        private float m_PitchVelocity;// used by SmoothDampAngle

        private Vector3 m_CurrentVelocity;   // current world-space movement velocity
        private Vector3 m_VelocityRef;       // ref for SmoothDamp

        void Start()
        {
            // initialize rotation from current transform
            Vector3 e = transform.eulerAngles;
            m_Yaw = e.y;
            m_Pitch = e.x;

            // initialize smoothed values to current so there's no pop
            m_SmoothYaw = m_Yaw;
            m_SmoothPitch = m_Pitch;
            m_CurrentVelocity = Vector3.zero;
            m_VelocityRef = Vector3.zero;
        }

        void OnEnable()
        {
            InputActionMap map = m_InputActions.FindActionMap(m_ActionMap, true);
            m_MoveAction = map.FindAction(m_MoveActionName, true);
            m_LookAction = map.FindAction(m_LookActionName, true);
            m_UpAction = map.FindAction(m_UpActionName, true);
            m_DownAction = map.FindAction(m_DownActionName, true);
            m_BoostAction = map.FindAction(m_BoostActionName, true);

            m_MoveAction?.Enable();
            m_LookAction?.Enable();
            m_UpAction?.Enable();
            m_DownAction?.Enable();
            m_BoostAction?.Enable();

            LockCursor(true);
        }

        void OnDisable()
        {
            
            LockCursor(false);

            m_MoveAction?.Disable();
            m_LookAction?.Disable();
            m_UpAction?.Disable();
            m_DownAction?.Disable();
            m_BoostAction?.Disable();
        }

        void LateUpdate()
        {
            if (m_InputActions)
            {
                // Read move (Vector2)
                Vector2 move = Vector2.zero;
                if (m_MoveAction != null)
                    move = m_MoveAction.ReadValue<Vector2>();

                // Read look (Vector2)
                Vector2 look = Vector2.zero;
                if (m_LookAction != null && m_IsCursorLocked)
                    look = m_LookAction.ReadValue<Vector2>();

                // Vertical motion
                float vertical = 0f;
                if (m_UpAction != null && m_UpAction.ReadValue<float>() > 0.5f) vertical += 1f;
                if (m_DownAction != null && m_DownAction.ReadValue<float>() > 0.5f) vertical -= 1f;

                bool boost = m_BoostAction != null && m_BoostAction.ReadValue<float>() > 0.5f;

                // Apply look first so movement uses the (smoothed) forward/right
                ApplyLook(look);
                ApplyMovement(move, vertical, boost);
            }
        }

        void ApplyLook(Vector2 look)
        {
            // Update target angles immediately (these are the target orientation values)
            if (look != Vector2.zero)
            {
                m_Yaw += look.x * MouseSensitivity;
                m_Pitch -= look.y * MouseSensitivity; // invert Y so moving mouse up looks up
                m_Pitch = Mathf.Clamp(m_Pitch, -MaxPitch, MaxPitch);
            }

            // Smooth yaw and pitch towards the target angles
            if (RotationSmoothTime > 0f)
            {
                m_SmoothYaw = Mathf.SmoothDampAngle(m_SmoothYaw, m_Yaw, ref m_YawVelocity, RotationSmoothTime);
                m_SmoothPitch = Mathf.SmoothDampAngle(m_SmoothPitch, m_Pitch, ref m_PitchVelocity, RotationSmoothTime);
            }
            else
            {
                m_SmoothYaw = m_Yaw;
                m_SmoothPitch = m_Pitch;
            }

            transform.rotation = Quaternion.Euler(m_SmoothPitch, m_SmoothYaw, 0f);
        }

        void ApplyMovement(Vector2 move, float vertical, bool boost)
        {
            // move.x = right, move.y = forward
            Vector3 dir = Vector3.zero;
            dir += transform.forward * move.y;
            dir += transform.right * move.x;
            dir += transform.up * vertical;

            float speed = MoveSpeed * (boost ? BoostMultiplier : 1f);

            // If there is input, targetVelocity is dir.normalized * speed
            Vector3 targetVelocity = Vector3.zero;
            if (dir.sqrMagnitude > 0f)
            {
                Vector3 dirNormalized = dir.normalized;
                targetVelocity = dirNormalized * speed;
            }

            if (MoveSmoothTime > 0f)
            {
                // Smoothly adjust current velocity towards targetVelocity
                m_CurrentVelocity = Vector3.SmoothDamp(m_CurrentVelocity, targetVelocity, ref m_VelocityRef, MoveSmoothTime);
                transform.position += m_CurrentVelocity * Time.deltaTime;
            }
            else
            {
                // No smoothing, immediate movement
                if (targetVelocity.sqrMagnitude > 0f)
                    transform.position += targetVelocity * Time.deltaTime;
            }
        }
        

        void LockCursor(bool locked)
        {
            m_IsCursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
