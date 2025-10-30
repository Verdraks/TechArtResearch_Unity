using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// VFXPainter: écoute les événements de sortie d'un VisualEffect (Visual Effect Graph) et déclenche
/// l'action de peinture en appelant le RuntimeScriptableEvent `RSE_Paint` avec les mêmes paramètres
/// que le système de `ParticlePainter`.
///
/// Comportement et hypothèses :
/// - Le VFX doit émettre un event output contenant un attribut Vector3 nommé (par défaut) "position".
///   Si l'attribut n'existe pas ou n'est pas récupérable, la position du VisualEffect est utilisée en fallback.
/// - Le script cherche un `Paintable` autour de la position (OverlapSphere) et appelle `m_Paint.Call(...)`.
/// - Vous pouvez filtrer les cibles avec `m_TargetLayerMask` et régler le rayon de détection.
///</summary>
public class VFXPainter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color m_Color = Color.white;
    [SerializeField] private float m_Radius = 0.1f;
    [SerializeField] private float m_Hardness = 0.5f;
    [SerializeField] private float m_Strength = 1f;

    [Header("VFX Input")]
    [SerializeField] private VisualEffect m_VisualEffect;
    [Tooltip("Nom de l'attribut Vector3 envoyé par le VFX (par défaut: 'position')")]
    [SerializeField] private string m_PositionAttributeName = "position";

    [Header("Detection")]
    [SerializeField] private LayerMask m_TargetLayerMask = ~0;
    [SerializeField] private float m_SearchRadius = 0.1f;
    [SerializeField][Tooltip("Nombre maximum d'objets recherchés par événement (utilisé pour le buffer non-allouant)")]
    private int m_MaxHits = 8;

    [Header("Output")]
    [SerializeField] private RSE_Paint m_Paint;

    // Buffer non-allouant pour éviter les allocations GC lors des événements fréquents
    private Collider[] m_ColliderBuffer;

    private void OnEnable()
    {
        // Pré-allouer le buffer de colliders
        if (m_ColliderBuffer == null || m_ColliderBuffer.Length != m_MaxHits)
            m_ColliderBuffer = new Collider[m_MaxHits];

        if (m_VisualEffect != null)
            m_VisualEffect.outputEventReceived += OnVFXOutputEvent;
    }

    private void OnDisable()
    {
        if (m_VisualEffect != null)
            m_VisualEffect.outputEventReceived -= OnVFXOutputEvent;
    }

    private void OnVFXOutputEvent(VFXOutputEventArgs evt)
    {
        if (m_Paint == null) return;

        Vector3 pos;
        // Essayer de lire l'attribut Vector3 du VFX, si absent fallback sur la position du VFX
        try
        {
            pos = evt.eventAttribute.GetVector3(m_PositionAttributeName);
        }
        catch
        {
            pos = m_VisualEffect != null ? m_VisualEffect.transform.position : Vector3.zero;
        }

        // Chercher un Paintable proche de la position reçue en utilisant la version NonAlloc
        int hitCount = Physics.OverlapSphereNonAlloc(pos, m_SearchRadius, m_ColliderBuffer, m_TargetLayerMask);
        for (int i = 0; i < hitCount; i++)
        {
            var hit = m_ColliderBuffer[i];
            if (hit == null) continue;
            if (hit.TryGetComponent<Paintable>(out var paintable))
            {
                PainterManager.PainterSettings settings = new PainterManager.PainterSettings
                {
                    Color = m_Color,
                    Position = pos,
                    Radius = m_Radius,
                    Hardness = m_Hardness,
                    Strength = m_Strength
                };

                m_Paint.Call(paintable.GetData(), settings);
                // Un seul paintable ciblé par événement
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 pos = Vector3.zero;
        if (m_VisualEffect != null) pos = m_VisualEffect.transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pos, m_SearchRadius);
    }
}
