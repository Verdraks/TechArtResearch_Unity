using UnityEngine;

namespace Controller.AutoFocusController
{
    public class OrbitalFocusCameraController : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Target à orbiter. Peut être null au départ.")] [SerializeField]
        private Transform m_Target;

        [Header("Orbit Settings")]
        [Tooltip("Distance entre la caméra et le focus (target + focusOffset)")] [SerializeField] [Min(0.01f)]
        private float m_OrbitDistance = 5f;

        [Tooltip("Vitesse de rotation autour de l'axe Y (degrés par seconde)")] [SerializeField] [Range(0f, 360f)]
        private float m_RotationSpeed = 30f;

        [Tooltip("Temps de lissage pour la position (valeur Small => plus réactif)")] [SerializeField] [Min(0f)]
        private float m_SmoothTime = 0.2f;

        [Header("Focus Offset")]
        [Tooltip("Offset appliqué au point de focus relatif à la target (en local world units)")] [SerializeField]
        private Vector3 m_FocusOffset = Vector3.zero;

        [Header("Initial Angle")]
        [Tooltip(
            "Angle initial en degrés autour de la target. Si enableRandomStartAngle est true, cette valeur est ignorée.")]
        [SerializeField]
        [Range(0f, 360f)]
        private float m_StartAngle;

        [Tooltip("Si true, la caméra commence à un angle aléatoire autour de la target")] [SerializeField]
        private bool m_EnableRandomStartAngle;

        private float m_CurrentAngle;

        private Vector3 m_Velocity = Vector3.zero;

        private void Start()
        {
            m_CurrentAngle = m_EnableRandomStartAngle ? Random.Range(0f, 360f) : m_StartAngle;

            if (m_Target == null) return;
            Vector3 targetPos = m_Target.position + m_FocusOffset;
            Quaternion rot = Quaternion.Euler(0f, m_CurrentAngle, 0f);
            transform.position = targetPos + rot * (Vector3.forward * m_OrbitDistance);
            transform.rotation = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);
        }

        private void LateUpdate()
        {
            if (!m_Target) return;

            m_CurrentAngle += m_RotationSpeed * Time.deltaTime;
            if (m_CurrentAngle >= 360f) m_CurrentAngle -= 360f;
            else if (m_CurrentAngle < 0f) m_CurrentAngle += 360f;

            Vector3 targetPos = m_Target.position + m_FocusOffset;

            Quaternion rotation = Quaternion.Euler(0f, m_CurrentAngle, 0f);
            Vector3 desiredPosition = targetPos + rotation * (Vector3.forward * m_OrbitDistance);

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref m_Velocity,
                Mathf.Max(0.0001f, m_SmoothTime));

            Quaternion desiredRot = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);
            float rotFactor = m_SmoothTime > 0f ? Time.deltaTime / m_SmoothTime : 1f;
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Mathf.Clamp01(rotFactor));
        }

    
        private void OnDrawGizmosSelected()
        {
            if (m_Target == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(m_Target.position + m_FocusOffset, Mathf.Max(0.01f, m_OrbitDistance));
        }
    }
}