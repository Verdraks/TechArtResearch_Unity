using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class AccumulationTracerPass : ScriptableRenderPass
{
    private static class ShaderProperties
    {
        public static readonly int TEXTURE_CURRENT_FRAME_SHADER_ID = Shader.PropertyToID("_CurrentFrame");
        public static readonly int TEXTURE_PREVIOUS_FRAME_SHADER_ID = Shader.PropertyToID("_PreviousFrame");
        public static readonly int FRAME_INDEX_SHADER_ID = Shader.PropertyToID("_FrameIndex");
    }

    private const string PASS_NAME = "Accumulation Tracer Pass";
    private readonly Material m_Material;

    private int m_ExecutionCount;

    private class PassData
    {
        public Material Material;
        public TextureHandle CurrentFrame;
        public TextureHandle PreviousFrame;
        public int FrameIndex;
    }

    public AccumulationTracerPass(PathTracerRenderFeature.Settings settings)
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        requiresIntermediateTexture = true;

        m_Material = CoreUtils.CreateEngineMaterial(settings.accumulationTracerShader);
    }

    private static void ExecutePass(PassData data, RasterGraphContext context)
    {
        MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();
        
        propertyBlock.SetTexture(ShaderProperties.TEXTURE_CURRENT_FRAME_SHADER_ID, data.CurrentFrame);
        propertyBlock.SetTexture(ShaderProperties.TEXTURE_PREVIOUS_FRAME_SHADER_ID, data.PreviousFrame);
        propertyBlock.SetInteger(ShaderProperties.FRAME_INDEX_SHADER_ID, data.FrameIndex);

        CoreUtils.DrawFullScreen(context.cmd, data.Material, propertyBlock);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        //History manager is valid only in game view
        if (resourceData.isActiveTargetBackBuffer || cameraData.historyManager == null)
        {
            return;
        }

        cameraData.historyManager.RequestAccess<PathTracerHistory>();
        PathTracerHistory history = cameraData.historyManager.GetHistoryForRead<PathTracerHistory>();

        if (history == null)
        {
            return;
        }
        
        TextureHandle source = resourceData.activeColorTexture;
        TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = $"CameraColor-{PASS_NAME}";
        destinationDesc.clearBuffer = false;
        TextureHandle handlerPass = renderGraph.CreateTexture(destinationDesc);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PASS_NAME, out var passData))
        {
            passData.Material = m_Material;
            passData.CurrentFrame = source;
            TextureHandle previousFrame = renderGraph.ImportTexture(history.PreviousFrame);
            if (previousFrame.IsValid())
            {
                passData.PreviousFrame = previousFrame;
            }
            else
            {
                passData.PreviousFrame = renderGraph.defaultResources.blackTexture;
            }
            passData.FrameIndex = m_ExecutionCount;

            builder.UseTexture(source, AccessFlags.Read);
            builder.UseTexture(passData.PreviousFrame, AccessFlags.Read);
            
            builder.SetRenderAttachment(handlerPass, 0);
            
            builder.AllowPassCulling(false);
            
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
        
        m_ExecutionCount++;
        
        resourceData.cameraColor = handlerPass;
    }
}