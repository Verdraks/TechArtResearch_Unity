using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Blob.Runtime
{
	public class BlobPass : ScriptableRenderPass, IDisposable
	{
		private static class ShaderProperties
		{
			internal static readonly int BlobBuffer = Shader.PropertyToID("_BlobBuffer");
			internal static readonly int BlobCount = Shader.PropertyToID("_BlobCount");
			internal static readonly int blitScaleBias = Shader.PropertyToID("_BlitScaleBias");
		}

		private class PassData
		{
			public Material Material;
			public BufferHandle BlobBuffer;
			public int BlobCount;
		}

		private Material _material = null;
		private GraphicsBuffer _bufferBlob = null;
		private const string PASS_NAME = "Blob Pass";

		public BlobPass(Shader shader)
		{
			renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
			
			_material = CoreUtils.CreateEngineMaterial(shader);
			_bufferBlob = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1024, Marshal.SizeOf<BlobData>());
		}

		public ScriptableRenderPassInput GetRequiredInput()
		{
			return ScriptableRenderPassInput.Depth;
		}
		
		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalResourceData ressource = frameData.Get<UniversalResourceData>();

			if (ressource.isActiveTargetBackBuffer)
			{
				return;
			}

			using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(PASS_NAME, out PassData passData))
			{
				passData.Material = _material;

				BlobData[] blobsData = UnityEngine.Object.FindObjectsByType<BlobRenderer>(FindObjectsSortMode.None).Select(o => o.BlobData).ToArray();
				_bufferBlob.SetData(blobsData);

				BufferHandle blobBuffer = renderGraph.ImportBuffer(_bufferBlob);
				builder.UseBuffer(blobBuffer, AccessFlags.Read);
				builder.UseTexture(ressource.activeDepthTexture, AccessFlags.Read);

				passData.BlobBuffer = blobBuffer;
				passData.BlobCount = blobsData.Length;

				builder.SetRenderAttachment(ressource.activeColorTexture, 0, AccessFlags.Write);
				builder.AllowPassCulling(false);
				builder.SetRenderFunc((PassData data, RasterGraphContext ctx) => ExecutePass(data, ctx));
			}
		}

		private static void ExecutePass(PassData data, RasterGraphContext context)
		{
			MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();

			propertyBlock.SetBuffer(ShaderProperties.BlobBuffer, data.BlobBuffer);
			propertyBlock.SetInteger(ShaderProperties.BlobCount, data.BlobCount);

			CoreUtils.DrawFullScreen(context.cmd, data.Material, propertyBlock);
		}

		public void Dispose()
		{
			_bufferBlob?.Dispose();
		}
	}
}