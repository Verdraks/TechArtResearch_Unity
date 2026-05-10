using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RayTracing.Runtime
{
	public class AccumulationTracerPass : ScriptableRenderPass
	{
		#region Intern Class
		private static class ShaderProperties
		{
			public static readonly int TEXTURE_CURRENT_FRAME_SHADER_ID = Shader.PropertyToID("_CurrentFrame");
			public static readonly int TEXTURE_PREVIOUS_FRAME_SHADER_ID = Shader.PropertyToID("_PreviousFrame");
			public static readonly int FRAME_INDEX_SHADER_ID = Shader.PropertyToID("_FrameIndex");
		}

		private class PassData
		{
			public Material material;
			public TextureHandle currentFrame;
			public TextureHandle previousFrame;
			public int frameIndex;
		}
		#endregion

		#region Constants
		private const string PASS_NAME = "Accumulation Tracer Pass";
		private const string TEXTURE_SUPPORT_NAME = " AccumulationTracer_Support";
		#endregion

		#region Fields
		private readonly Material m_Material;
		#endregion

		#region Constructors
		public AccumulationTracerPass(RayTracingRenderFeature.Settings settings)
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
			requiresIntermediateTexture = true;

			m_Material = CoreUtils.CreateEngineMaterial(settings.accumulationTracerShader);
		}
		#endregion

		#region Static Methods
		public static bool CanExecutePass(UniversalResourceData resourceData, UniversalCameraData cameraData)
		{
			if (resourceData.isActiveTargetBackBuffer)
			{
				return false;
			}

			if (cameraData.cameraType != CameraType.Game)
			{
				return false;
			}

			if (cameraData.historyManager == null)
			{
				return false;
			}

			if (cameraData.historyManager.GetHistoryForRead<RayTracingHistory>() == null)
			{
				return false;
			}

			return true;
		}

		private static void Reset(UniversalCameraData cameraData)
		{
			if (cameraData.historyManager == null)
			{
				return;
			}

			cameraData.historyManager.RequestAccess<RayTracingHistory>();
			RayTracingHistory history = cameraData.historyManager.GetHistoryForWrite<RayTracingHistory>();
			history?.Reset();
		}

		private static void ExecutePass(PassData data, RasterGraphContext context)
		{
			MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();

			propertyBlock.SetTexture(ShaderProperties.TEXTURE_CURRENT_FRAME_SHADER_ID, data.currentFrame);
			propertyBlock.SetTexture(ShaderProperties.TEXTURE_PREVIOUS_FRAME_SHADER_ID, data.previousFrame);
			propertyBlock.SetInteger(ShaderProperties.FRAME_INDEX_SHADER_ID, data.frameIndex);

			CoreUtils.DrawFullScreen(context.cmd, data.material, propertyBlock);
		}
		#endregion Static Methods

		#region Methods
		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

			if (CanExecutePass(resourceData, cameraData) == false)
			{
				if (frameData.Contains<PathTracerFrameData>())
				{
					resourceData.cameraColor = frameData.Get<PathTracerFrameData>().pathTracerTexture;
				}

				Reset(cameraData);
				return;
			}

			cameraData.historyManager.RequestAccess<RayTracingHistory>();
			RayTracingHistory history = cameraData.historyManager.GetHistoryForRead<RayTracingHistory>();

			if (frameData.Contains<PathTracerFrameData>() == false)
			{
				Reset(cameraData);
				return;
			}

			TextureHandle source = frameData.Get<PathTracerFrameData>().pathTracerTexture;
			
			history.Increment();

			TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
			destinationDesc.name = TEXTURE_SUPPORT_NAME;
			destinationDesc.clearBuffer = false;
			TextureHandle support = renderGraph.CreateTexture(destinationDesc);
			frameData.Get<PathTracerFrameData>().supportTexture = support;

			using (IRasterRenderGraphBuilder
					builder = renderGraph.AddRasterRenderPass(PASS_NAME, out PassData passData))
			{
				passData.material = m_Material;
				passData.currentFrame = source;

				passData.previousFrame = renderGraph.ImportTexture(history.PreviousFrame);

				builder.UseTexture(passData.previousFrame, AccessFlags.Read);

				passData.frameIndex = frameData.Get<PathTracerFrameData>().frameIndex;

				builder.UseTexture(source, AccessFlags.Read);

				builder.SetRenderAttachment(support, 0);

				builder.AllowPassCulling(false);

				builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
			}

			//Assign the support texture to the camera color to avoid blit back
			resourceData.cameraColor = support;
		}
		#endregion Methods
	}
}