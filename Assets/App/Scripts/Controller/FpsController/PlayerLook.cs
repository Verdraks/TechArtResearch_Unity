using UnityEngine;
using UnityEngine.Serialization;

namespace Controller.FpsController
{
    public class PlayerCameraFollow : MonoBehaviour
    {
        [FormerlySerializedAs("playerBody")]
        [Header("References")]
        [SerializeField] private Transform m_PlayerBody; 
        [FormerlySerializedAs("input")] [SerializeField] private InputHandler m_Input;

        [FormerlySerializedAs("sensitivity")]
        [Header("Settings")]
        [SerializeField] private float m_Sensitivity = 100f;

        // Smoothing settings
        [Header("Smoothing")]
        [Tooltip("Activer le lissage des mouvements de la caméra (adoucit le regard)")]
        [SerializeField] private bool m_EnableSmoothing = true;

        [Tooltip("Temps de lissage pour SmoothDamp (valeur plus petite = plus réactif)")]
        [SerializeField, Min(0f)] private float m_SmoothTime = 0.05f;

        // Internal smoothing state
        private Vector2 m_CurrentLook = Vector2.zero;
        private Vector2 m_CurrentLookVelocity = Vector2.zero;

        private float m_XRotation;

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
            Vector2 look = m_Input.LookInput;

            // calcul de la cible (delta * sensibilité * dt)
            float deltaMultiplier = m_Sensitivity * Time.deltaTime;
            Vector2 targetDelta = look * deltaMultiplier;

            // appliquer le lissage si activé
            Vector2 appliedDelta;
            if (m_EnableSmoothing)
            {
                m_CurrentLook = Vector2.SmoothDamp(m_CurrentLook, targetDelta, ref m_CurrentLookVelocity, Mathf.Max(0.0001f, m_SmoothTime));
                appliedDelta = m_CurrentLook;
            }
            else
            {
                // pas de smoothing -> appliquer directement
                appliedDelta = targetDelta;
                // garder l'état interne propre si on réactive plus tard
                m_CurrentLook = appliedDelta;
                m_CurrentLookVelocity = Vector2.zero;
            }

            float mouseX = appliedDelta.x;
            float mouseY = appliedDelta.y;

            m_XRotation -= mouseY;
            m_XRotation = Mathf.Clamp(m_XRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(m_XRotation, 0f, 0f);
            m_PlayerBody.Rotate(Vector3.up * mouseX);
        }
    }
}