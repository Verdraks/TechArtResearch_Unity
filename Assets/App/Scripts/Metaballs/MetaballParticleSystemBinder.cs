using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteAlways]
public class MetaballParticleSystemBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer m_TargetRenderer;
    [SerializeField] private ParticleSystem m_ParticleSystem;
    
    private GraphicsBuffer m_Buffer;
    private ParticleSystem.Particle[] m_Particles;
    private MetaballDataConstant.MetaballData[] m_DataCopy;
    private MaterialPropertyBlock m_PropertyBlock;

    private void OnValidate()
    {
        OnDisable();
        OnEnable();
    }

    private void OnEnable()
    {
        if (!IsValid()) return;
        
        int count = m_ParticleSystem.main.maxParticles;
        
        m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, Marshal.SizeOf(typeof(MetaballDataConstant.MetaballData)));

        m_DataCopy = new MetaballDataConstant.MetaballData[count];
        m_Particles = new ParticleSystem.Particle[count];

        m_PropertyBlock = new MaterialPropertyBlock();
        m_PropertyBlock.SetBuffer(MetaballDataConstant.S_MetaballDataBufferMat, m_Buffer);
        m_PropertyBlock.SetInt(MetaballDataConstant.S_MetaballCountMat, 0);
        
        m_TargetRenderer?.SetPropertyBlock(m_PropertyBlock);
    }

    private bool IsValid()
    {
        return m_ParticleSystem && m_TargetRenderer;
    }

    private void Update()
    {
        if (!IsValid()) return;
        
        m_ParticleSystem.GetParticles(m_Particles);
        for (int i = 0; i < m_ParticleSystem.particleCount; i++)
        {
            ParticleSystem.Particle particle = m_Particles[i];
            m_DataCopy[i].Position = particle.position;
            m_DataCopy[i].Radius = particle.GetCurrentSize(m_ParticleSystem);
        }
        
        m_Buffer.SetData(m_DataCopy);
        m_PropertyBlock.SetInt(MetaballDataConstant.S_MetaballCountMat, m_ParticleSystem.particleCount);
        m_TargetRenderer?.SetPropertyBlock(m_PropertyBlock);
    }

    private void OnDisable()
    {
        m_Buffer?.Release();
        m_PropertyBlock?.Clear();
        m_TargetRenderer?.SetPropertyBlock(null);
    }
}