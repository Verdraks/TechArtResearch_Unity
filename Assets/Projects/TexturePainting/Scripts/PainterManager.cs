using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PainterManager : RegularSingleton<PainterManager>
{
    
    [Header("Settings")]
    [SerializeField] private bool m_IsVerbose;
    
    [Header("References")]
    [SerializeField] private Shader m_PainterShader;
    
    private Material m_PainterMaterial;
    private CommandBuffer m_CmdPaint;
    private float m_TimeSinceLastExecutionCmd;
    
    private static readonly int s_SupportTextureIdShader = Shader.PropertyToID("_MainTex");
    private static readonly int s_ColorIdShader = Shader.PropertyToID("_PainterColor");
    private static readonly int s_PositionIdShader = Shader.PropertyToID("_PainterPosition");
    private static readonly int s_RadiusIdShader = Shader.PropertyToID("_Radius");
    private static readonly int s_HardnessIdShader = Shader.PropertyToID("_Hardness");
    private static readonly int s_StrengthIdShader = Shader.PropertyToID("_Strength");
    private static readonly int s_UvIslandsTextureIdShader = Shader.PropertyToID("_UvIslandsTex");
    private static readonly int s_PrepareUvIslandsIdShader = Shader.PropertyToID("_PrepareUvIslands");
    private static readonly int s_UvOffsetIdShader = Shader.PropertyToID("_UvOffset");


    protected override void Awake()
    {
        base.Awake();
        m_PainterMaterial = new Material(m_PainterShader){name = m_PainterShader.name + " (Instance)"};
        m_CmdPaint = new CommandBuffer { name = $"Command Buffer : {gameObject.name} " };
    }

    private void OnDestroy()
    {
        Destroy(m_PainterMaterial);
        m_CmdPaint.Release();
    }


    public void SetupPaintable(Paintable.PaintableData data)
    {
        m_PainterMaterial.SetFloat(s_PrepareUvIslandsIdShader, 1);
        m_CmdPaint.SetRenderTarget(data.UvIslands);
        m_CmdPaint.DrawRenderer(data.Renderer,m_PainterMaterial,0,0);
        
        Graphics.ExecuteCommandBuffer(m_CmdPaint);
        m_CmdPaint.Clear();
    }
    
    /// <summary>
    /// Register a paint action to be executed in the command buffer later.
    /// </summary>
    /// <param name="target">Data of the paintable target </param>
    /// <param name="settings">Settings of the painter requested the action</param>
    public void Paint(Paintable.PaintableData target, PainterSettings settings)
    {
        
        if (m_IsVerbose)
            Debug.Log($"[PainterManager] Paint action received at {Time.time} on {target.Renderer.name} with settings: " +
                      $"Position: {settings.Position}, Radius: {settings.Radius}, Hardness: {settings.Hardness}, Strength: {settings.Strength}, Color: {settings.Color}");
        
        m_PainterMaterial.SetTexture(s_SupportTextureIdShader, target.Support);
        m_PainterMaterial.SetTexture(s_UvIslandsTextureIdShader, target.UvIslands);
        m_PainterMaterial.SetColor(s_ColorIdShader, settings.Color);
        m_PainterMaterial.SetVector(s_PositionIdShader, settings.Position);
        m_PainterMaterial.SetFloat(s_RadiusIdShader, settings.Radius);
        m_PainterMaterial.SetFloat(s_HardnessIdShader, settings.Hardness);
        m_PainterMaterial.SetFloat(s_StrengthIdShader,settings.Strength);
        m_PainterMaterial.SetFloat(s_PrepareUvIslandsIdShader, 0);
        
        m_CmdPaint.SetRenderTarget(target.Mask);
        m_CmdPaint.DrawRenderer(target.Renderer, m_PainterMaterial,0,0);
        
        m_CmdPaint.Blit(target.Mask, target.Support);
        
        Graphics.ExecuteCommandBuffer(m_CmdPaint);
        m_CmdPaint.Clear();
    }
    
    public struct PainterSettings
    {
        public Vector3 Position;
        public float Radius;
        public float Hardness;
        public float Strength;
        public Color Color;
    }
}
