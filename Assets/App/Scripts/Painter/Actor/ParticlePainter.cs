using System.Collections.Generic;
using UnityEngine;


public class ParticlePainter : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private Color m_Color = Color.white;
    [SerializeField] private float m_Radius = 0.1f;
    [SerializeField] private float m_Hardness = 0.5f;
    [SerializeField] private float m_Strength = 1f;

    [Header("References")]
    [SerializeField] private ParticleSystem m_ParticleSystem;

    private readonly List<ParticleCollisionEvent> m_CollisionEvents = new();
    private void OnParticleCollision(GameObject other)
    {
        if (!other.TryGetComponent(out Paintable paintable)) return;
        
        int particleCollisionCount = m_ParticleSystem.GetCollisionEvents(other, m_CollisionEvents);
        
        for (int i = 0; i < particleCollisionCount; i++)
        {
            Vector3 posHit = m_CollisionEvents[i].intersection;
            PainterManager.PainterSettings settings = new()
            {
                Color = m_Color,
                Position = posHit,
                Radius = m_Radius,
                Hardness = m_Hardness,
                Strength = m_Strength
            };
                
            PainterManager.Instance?.Paint(paintable.GetData(), settings);
        }
    }
}