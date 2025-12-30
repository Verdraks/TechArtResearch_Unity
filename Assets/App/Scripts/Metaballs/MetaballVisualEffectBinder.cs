using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class MetaballVisualEffectBinder : MonoBehaviour
{
    private static readonly int s_MetaballDataBufferVfxProp = Shader.PropertyToID("MetaballsDataBuffer");
    
    [Header("Settings")]
    [SerializeField] private int m_MaxParticles = 10;
    
    [Header("References")]
    [SerializeField] private Renderer m_TargetRenderer;
    [SerializeField] private Material m_MatMetaball;
    [SerializeField] private VisualEffect m_VFXMetaball;

    private GraphicsBuffer m_Buffer;
    private Material m_MaterialInstance;

    private void OnValidate()
    {
        OnDisable();
        OnEnable();
    }

    private void OnEnable()
    {
        m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_MaxParticles, Marshal.SizeOf(typeof(MetaballDataConstant.MetaballData)));
        if (m_TargetRenderer && m_MatMetaball)
        {
            m_MaterialInstance = new Material(m_MatMetaball)
            {
                name = m_MatMetaball.name + " (Instance)"
            };
            m_TargetRenderer.material = m_MaterialInstance;
            
            m_MaterialInstance.SetBuffer(MetaballDataConstant.S_MetaballDataBufferMat, m_Buffer);
            m_MaterialInstance.SetInt(MetaballDataConstant.S_MetaballCountMat, m_MaxParticles);
        }
        if (m_VFXMetaball)
        {
            m_VFXMetaball.SetGraphicsBuffer(s_MetaballDataBufferVfxProp, m_Buffer);
        }
    }

    private void OnDisable()
    {
        if(m_Buffer != null && m_Buffer.IsValid()) m_Buffer.Release();
        if (m_MaterialInstance)
        {
            #if UNITY_EDITOR
            DestroyImmediate(m_MaterialInstance);
            #else
            Destroy(m_MaterialInstance);
            #endif
        }
    }
}
