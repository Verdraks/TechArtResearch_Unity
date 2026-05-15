using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace RayTracing.Runtime
{
    public class WritePathTracerHistoryPass : ScriptableRenderPass
    {
        #region Constants
        private const string PASS_NAME = "Write PathTracer History";
        #endregion

        #region Constructors
        public WritePathTracerHistoryPass(RayTracingRenderFeature.Settings settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }
        #endregion

        #region Methods
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (AccumulationTracerPass.CanExecutePass(resourceData, cameraData) == false || frameData.Contains<PathTracerFrameData>() == false)
            {
                return;
            }

            cameraData.historyManager.RequestAccess<RayTracingHistory>();
            RayTracingHistory rayTracingHistory = cameraData.historyManager.GetHistoryForWrite<RayTracingHistory>();

            TextureHandle destination = renderGraph.ImportTexture(rayTracingHistory.CurrentFrame);
            
            TextureHandle source = frameData.Get<PathTracerFrameData>().supportTexture;

            if (renderGraph.CanAddCopyPass(source, destination))
            {
                using (IBaseRenderGraphBuilder builder = renderGraph.AddCopyPass(source, destination, PASS_NAME, true))
                {
                    builder.UseTexture(source);
                    builder.AllowPassCulling(false);
                }
            }
        }
        #endregion
    }
}