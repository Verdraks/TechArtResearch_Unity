using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Blob.Runtime
{
    public class BlobPass : ScriptableRenderPass
    {
        private static class ShaderProperties
        {
            public static readonly int BlobBuffer = Shader.PropertyToID("_BlobBuffer");
            public static readonly int BlobCount = Shader.PropertyToID("_BlobCount");
        }
        
        private class PassData
        {
            public Material Material;
            public BufferHandle BlobBuffer;
            public int BlobCount;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // var ressource = frameData.Get<UniversalRendererResources>()
            
            // using (var builder = renderGraph.AddRasterRenderPass("Blob", out PassData passData))
            // {
            //     builder.SetInputAttachment(frameData.);
            //     builder.SetRenderFunc((PassData data, RasterGraphContext ctx) => ExecutePass(data, ctx));
            // }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();

            propertyBlock.SetBuffer(ShaderProperties.BlobBuffer, data.BlobBuffer);
            propertyBlock.SetInteger(ShaderProperties.BlobCount, data.BlobCount);

            CoreUtils.DrawFullScreen(context.cmd, data.Material, propertyBlock);
        }
    }
}