using UnityEngine;

public class MousePainter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool m_EnableContinuousPainting = true;
    
    [SerializeField] private Color m_Color = Color.red;
    [SerializeField, Range(0.01f, 1f)] private float m_Radius = 0.1f;
    [SerializeField, Range(0f, 1f)] private float m_Hardness = 0.5f;
    [SerializeField, Range(0f, 1f)] private float m_Strength = 1f;
    
    [Header("Output")]
    [SerializeField] private RSE_Paint m_Paint;
    
    private void Update()
    {
        if (CanPaint())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                
                if (hit.collider.TryGetComponent(out Paintable paintable))
                {
                    PainterManager.PainterSettings settings = new ()
                    {
                        Color = m_Color,
                        Position = hit.point,
                        Radius = m_Radius,
                        Hardness = m_Hardness,
                        Strength = m_Strength
                    };
                    
                    m_Paint.Call(paintable.GetData(), settings);
                }
            }
        }
    }

    private bool CanPaint()
    {
        return m_EnableContinuousPainting ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
    }
}
