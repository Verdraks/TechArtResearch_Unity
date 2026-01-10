
using System;
using UnityEngine;

public class CollisionPainter : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private Color m_Color = Color.red;
    [SerializeField, Range(0.01f, 1f)] private float m_Radius = 0.1f;
    [SerializeField, Range(0f, 1f)] private float m_Hardness = 0.5f;
    [SerializeField, Range(0f, 1f)] private float m_Strength = 1f;


    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Paintable paintable))
        {
            foreach (ContactPoint contact in other.contacts)
            {
                PainterManager.PainterSettings settings = new()
                {
                    Color = m_Color,
                    Position = contact.point,
                    Radius = m_Radius,
                    Hardness = m_Hardness,
                    Strength = m_Strength
                };
                
                PainterManager.Instance?.Paint(paintable.GetData(), settings);
            }
        }
    }
}