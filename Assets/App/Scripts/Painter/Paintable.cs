using UnityEngine;

public class Paintable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private SSO_PaintableConfig m_Config;
    
    [Header("References")]
    [SerializeField] private Renderer m_Renderer;

    private PaintableData m_PaintableData;
    private MaterialPropertyBlock m_MaterialPropertyBlock;
    
    //Use two different RenderTextures to avoid read/write issues
    private RenderTexture m_MaskRenderTexture;
    private RenderTexture m_SupportRenderTexture;
    
    private RenderTexture m_UvIslandsRenderTexture;
    
    private static readonly int s_MaskTextureIdShader = Shader.PropertyToID("_PaintMask");

    private void OnValidate()
    {
        if (m_Renderer == null) m_Renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        m_MaskRenderTexture = new RenderTexture(m_Config.TextureSize, m_Config.TextureSize, 0)
        {
            filterMode = m_Config.FilterMode,
            name = $"Mask Render Texture: {gameObject.name}",
            wrapMode = m_Config.WrapMode,
            antiAliasing = m_Config.AntiAliasing
        };
        m_MaskRenderTexture.Create();
        
        m_SupportRenderTexture = new RenderTexture(m_Config.TextureSize, m_Config.TextureSize, 0)
        {
            filterMode = m_Config.FilterMode,
            name = $"Support Render Texture: {gameObject.name}",
            wrapMode = m_Config.WrapMode
        };
        m_SupportRenderTexture.Create();

        m_UvIslandsRenderTexture = new RenderTexture(m_Config.TextureSize, m_Config.TextureSize, 0)
        {
            filterMode = m_Config.FilterMode,
            name = $"UV Islands Render Texture: {gameObject.name}",
            wrapMode = m_Config.WrapMode
        };
        m_UvIslandsRenderTexture.Create();
        
        m_MaterialPropertyBlock = new MaterialPropertyBlock();
        m_MaterialPropertyBlock.SetTexture(s_MaskTextureIdShader, m_MaskRenderTexture);
        m_Renderer.SetPropertyBlock(m_MaterialPropertyBlock);

        // m_Renderer.material.SetTexture(s_MaskTextureIdShader, m_MaskRenderTexture);
        
        m_PaintableData = new PaintableData
        {
            Mask = m_MaskRenderTexture,
            Support = m_SupportRenderTexture,
            Renderer = m_Renderer,
            UvIslands = m_UvIslandsRenderTexture
        };
        
        PainterManager.Instance?.SetupPaintable(m_PaintableData);
    }
    
    private void OnDestroy()
    {
        m_MaskRenderTexture.Release();
        m_SupportRenderTexture.Release();
        m_UvIslandsRenderTexture.Release();
    }

    public PaintableData GetData()
    {
        return m_PaintableData;
    }
    
    public class PaintableData
    {
        public RenderTexture Mask;
        public RenderTexture Support;
        public RenderTexture UvIslands;
        public Renderer Renderer;
    }
    
}