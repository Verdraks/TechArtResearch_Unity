using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace PathTracer
{
    public class WritePathTracerHistoryPass : ScriptableRenderPass
    {
        private const string PASS_NAME = "Write PathTracer History";
        private readonly PathTracerRenderFeature.Settings m_Settings;

        public WritePathTracerHistoryPass(PathTracerRenderFeature.Settings settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = true;
            m_Settings = settings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (m_Settings.useAccumulation == false || cameraData.historyManager == null)
            {
                return;
            }
        
            cameraData.historyManager.RequestAccess<RawColorHistory>();
            PathTracerHistory pathTracerHistory = cameraData.historyManager.GetHistoryForWrite<PathTracerHistory>();

            TextureHandle destination = renderGraph.ImportTexture(pathTracerHistory.CurrentFrame);

            RenderTextureDescriptor historyDesc = cameraData.cameraTargetDescriptor;
            historyDesc.depthBufferBits = 0;
            historyDesc.msaaSamples = 1;
            pathTracerHistory.Update(historyDesc);

            if (renderGraph.CanAddCopyPass(resourceData.activeColorTexture, destination))
            {
                using (IBaseRenderGraphBuilder builder = renderGraph.AddCopyPass(resourceData.activeColorTexture, destination, PASS_NAME, true))
                {
                    builder.AllowPassCulling(false);
                }
            }
        
       
        }
    }
}