using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Blob.Runtime
{
    public class BlobPass : ScriptableRenderPass, IDisposable
    {
        private static class ShaderProperties
        {
            internal static readonly int BlobBuffer = Shader.PropertyToID("_BlobBuffer");
            internal static readonly int BlobCount = Shader.PropertyToID("_BlobCount");
            internal static readonly int BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        }

        private class PassData
        {
            public MeshRenderer BlobRenderer;
            public BufferHandle BlobBuffer;
            public int BlobCount;
        }

        private readonly Material _material = null;
        private GraphicsBuffer _bufferBlob = null;
        private GameObject _hook = null;
        private const string PASS_NAME = "Blob Pass";

        public BlobPass(Shader shader)
        {
            _material = CoreUtils.CreateEngineMaterial(shader);
            BlobPass_Internal();
        }

        public BlobPass(Material material)
        {
            _material = material;
            BlobPass_Internal();
        }

        private void BlobPass_Internal()
        {
            renderPassEvent = RenderPassEvent.BeforeRendering;

            _bufferBlob = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1024, Marshal.SizeOf<BlobData>());

            CreateHook();
        }

        private void CreateHook()
        {
            _hook = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _hook.name = PASS_NAME;
            _hook.hideFlags = HideFlags.HideAndDontSave;
            
            MeshRenderer renderer = _hook.GetComponent<MeshRenderer>();
            
            renderer.receiveShadows = false;
            renderer.sharedMaterial = _material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (IUnsafeRenderGraphBuilder builder = renderGraph.AddUnsafePass(PASS_NAME, out PassData passData))
            {
                builder.AllowPassCulling(false);
                
                BlobData[] blobsData = Object.FindObjectsByType<BlobRenderer>(FindObjectsInactive.Exclude ,FindObjectsSortMode.None)
                    .Select(o => o.BlobData).ToArray();
                _bufferBlob.SetData(blobsData);
                
                BufferHandle blobBuffer = renderGraph.ImportBuffer(_bufferBlob);
                builder.UseBuffer(blobBuffer, AccessFlags.Read);
                
                passData.BlobBuffer = blobBuffer;
                passData.BlobCount = blobsData.Length;
                passData.BlobRenderer = _hook.GetComponent<MeshRenderer>();

                Bounds bound = new Bounds();
                    
                for (int i = 0; i < blobsData.Length; i++)
                {
                    bound.Encapsulate(blobsData[i].position - Vector3 .one * blobsData[i].radius);
                    bound.Encapsulate(blobsData[i].position + Vector3.one * blobsData[i].radius);
                }
                
                _hook.transform.position = bound.center;
                _hook.transform.localScale = bound.size;
                
                builder.SetRenderFunc(static (PassData data, UnsafeGraphContext ctx) => ExecutePass(data, ctx));
            }
        }

        private static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();
            propertyBlock.SetBuffer(ShaderProperties.BlobBuffer, data.BlobBuffer);
            propertyBlock.SetInteger(ShaderProperties.BlobCount, data.BlobCount);
            data.BlobRenderer.SetPropertyBlock(propertyBlock);
        }

        public void Dispose()
        {
            _bufferBlob?.Dispose();
#if UNITY_EDITOR
            Object.DestroyImmediate(_hook);
#else
            Object.Destroy(hook);
#endif
        }
    }
}