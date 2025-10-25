using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class MetaballVisualBinder : MonoBehaviour
{
    private static readonly int s_MetaballDataBufferMatProp = Shader.PropertyToID("_MetaballsDataBuffer");
    private static readonly int s_MetaballDataBufferVfxProp = Shader.PropertyToID("MetaballsDataBuffer");
    private static readonly int s_MetaballsCountMatProp = Shader.PropertyToID("_MetaballsCount");

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
        m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_MaxParticles, Marshal.SizeOf(typeof(MetaballData)));
        if (m_TargetRenderer && m_MatMetaball)
        {
            m_MaterialInstance = new Material(m_MatMetaball)
            {
                name = m_MatMetaball.name + " (Instance)"
            };
            m_TargetRenderer.material = m_MaterialInstance;
            
            m_MaterialInstance.SetBuffer(s_MetaballDataBufferMatProp, m_Buffer);
            m_MaterialInstance.SetInt(s_MetaballsCountMatProp, m_MaxParticles);
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

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer), StructLayout(LayoutKind.Sequential)]
    private struct MetaballData
    {
        public Vector3 Position;
        public float Radius;
    }
}
