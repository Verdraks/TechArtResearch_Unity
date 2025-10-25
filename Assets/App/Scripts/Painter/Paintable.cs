using System;
using UnityEngine;
using UnityEngine.UI;

public class Paintable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer m_Renderer;

    private PaintableData m_PaintableData;
    
    private RenderTexture m_MaskRenderTexture;
    private RenderTexture m_SupportRenderTexture;
    
    private const int k_TextureSize = 512;
    private static readonly int s_MaskTextureIdShader = Shader.PropertyToID("_PaintMask");

    private void Start()
    {
        m_MaskRenderTexture = new RenderTexture(k_TextureSize, k_TextureSize, 0)
        {
            filterMode = FilterMode.Bilinear,
            name = $"Mask Render Texture: {gameObject.name}",
            wrapMode = TextureWrapMode.Clamp
        };
        m_MaskRenderTexture.Create();
        
        m_SupportRenderTexture = new RenderTexture(k_TextureSize, k_TextureSize, 0)
        {
            filterMode = FilterMode.Bilinear,
            name = $"Support Render Texture: {gameObject.name}",
            wrapMode = TextureWrapMode.Clamp
        };
        m_SupportRenderTexture.Create();

        m_Renderer.material.SetTexture(s_MaskTextureIdShader, m_MaskRenderTexture);
        
        m_PaintableData = new PaintableData
        {
            Mask = m_MaskRenderTexture,
            Support = m_SupportRenderTexture,
            Renderer = m_Renderer
        };
    }
    
    private void OnDestroy()
    {
        m_MaskRenderTexture.Release();
        m_SupportRenderTexture.Release();
    }

    public PaintableData GetData()
    {
        return m_PaintableData;
    }
    
    public struct PaintableData
    {
        public RenderTexture Mask;
        public RenderTexture Support;
        public Renderer Renderer;
    }
    
}