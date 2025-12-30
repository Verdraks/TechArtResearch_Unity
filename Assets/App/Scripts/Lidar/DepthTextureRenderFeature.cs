using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class DepthTextureRenderFeature : ScriptableRendererFeature
{
    
    private ScriptableRenderPass m_DepthTexturePass;
    
    public override void Create()
    {
        m_DepthTexturePass = new DepthTextureCopyRenderPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_DepthTexturePass);
    }
    
    private class DepthTextureCopyRenderPass : ScriptableRenderPass
    {
        public DepthTextureCopyRenderPass()
        {
            
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            base.RecordRenderGraph(renderGraph, frameData);

            
            // using (var builder = renderGraph.AddRenderPass("Depth Texture Copy",out var passDescriptor))
            // {
            //     
            // }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }
    }
}
