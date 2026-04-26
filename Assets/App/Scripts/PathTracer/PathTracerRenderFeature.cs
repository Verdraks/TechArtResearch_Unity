using System;
using System.Runtime.InteropServices;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PathTracerRenderFeature : ScriptableRendererFeature
{
	#region Fields
	#region Serialized
	[SerializeField] private Settings m_Settings;
	#endregion Serialized
	#region Private
	private PathTracerPass m_PathTracerPass;
	private AccumulationTracerPath m_AccumulationTracerPass;

	#endregion Private
	#endregion Fields

	#region Methods
	public override void Create()
	{
		m_PathTracerPass = new PathTracerPass(m_Settings);
		m_AccumulationTracerPass = new AccumulationTracerPath(m_Settings);
	}

	protected override void Dispose(bool disposing)
	{
		m_PathTracerPass.Dispose();
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_PathTracerPass);
		renderer.EnqueuePass(m_AccumulationTracerPass);
	}
	#endregion Methods

	#region Intern class
	[Serializable] private class Settings
	{
		public Shader pathTracerShader;
		public Shader accumulationTracerShader;
		public int maxDepth = 10;
		[Min(1)] public int rayPerPixel = 1;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	private struct Sphere
	{
		public Vector3 center;
		public float radius;
		public Mat material;
	}

	private struct Mat
	{
		public Vector4 color;
	}
	#endregion Intern class

	#region Pass
	class PathTracerPass : ScriptableRenderPass, IDisposable
	{
		private static class ShaderProperties
		{
			public static readonly int VIEW_PARAM_SHADER_ID = Shader.PropertyToID("_ViewParams");
			public static readonly int MAX_DEPTH_SHADER_ID = Shader.PropertyToID("_MaxDepth");
			public static readonly int SPHERE_BUFFER_SHADER_ID = Shader.PropertyToID("_SpheresBuffer");
			public static readonly int SPHERE_COUNT_SHADER_ID = Shader.PropertyToID("_SpheresCount");
			public static readonly int RAYS_PER_PIXEL_SHADER_ID = Shader.PropertyToID("_RaysPerPixel");
		}

		private const string PASS_NAME = "Path Tracer Pass";
		
		private readonly Settings m_Settings;
		private readonly Material m_Material;
		private readonly GraphicsBuffer m_SphereBuffer;
		private readonly int m_SphereCount = 10;

		private readonly TextureHandle m_TargetRender;

		public PathTracerPass(Settings settings)
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

			m_Settings = settings;
			m_Material = CoreUtils.CreateEngineMaterial(settings.pathTracerShader);
			m_SphereBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_SphereCount, Marshal.SizeOf(typeof(Sphere)));

			Sphere[] spheres = new Sphere[m_SphereCount];
			spheres[0] = new Sphere
			{
				center = new Vector3(0, -100.5f, 0),
				radius = 100f,
				material = new Mat { color = Color.grey }
			};

			for (int i = 1; i < m_SphereCount; i++)
			{
				Sphere sphere = new Sphere
				{
					center = new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(-5.0f, 5.0f)),
					radius = UnityEngine.Random.Range(0.5f, 2f),
					material = new Mat { color = UnityEngine.Random.ColorHSV() }
				};
				spheres[i] = sphere;

			}
			m_SphereBuffer.SetData(spheres);
		}

		private class PassData
		{
			public ProfilerMarker Marker;
			public Material Material;
			public MaterialPropertyBlock PropertyBlock;
		}

		static void ExecutePass(PassData data, RasterGraphContext context)
		{
			CoreUtils.DrawFullScreen(context.cmd, data.Material, data.PropertyBlock);
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
			
			using (var builder = renderGraph.AddRasterRenderPass<PassData>(PASS_NAME, out var passData))
			{
				passData.Material = m_Material;
				passData.PropertyBlock = new MaterialPropertyBlock();

				Camera currentCamera = cameraData.camera;

				float planeHeight = currentCamera.nearClipPlane * Mathf.Tan(currentCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2;
				float planeWidth = planeHeight * currentCamera.aspect;

				passData.PropertyBlock.SetVector(ShaderProperties.VIEW_PARAM_SHADER_ID, new Vector4(planeWidth, planeHeight, currentCamera.nearClipPlane, 0));
				passData.PropertyBlock.SetInt(ShaderProperties.MAX_DEPTH_SHADER_ID, m_Settings.maxDepth);
				passData.PropertyBlock.SetInt(ShaderProperties.RAYS_PER_PIXEL_SHADER_ID, m_Settings.rayPerPixel);
				passData.PropertyBlock.SetInt(ShaderProperties.SPHERE_COUNT_SHADER_ID, m_SphereCount);
				passData.PropertyBlock.SetBuffer(ShaderProperties.SPHERE_BUFFER_SHADER_ID, m_SphereBuffer);

				builder.AllowPassCulling(false);
				builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

				builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
			}
		}

		public void Dispose()
		{
			m_SphereBuffer?.Release();
		}
	}

	class AccumulationTracerPath : ScriptableRenderPass
	{
		private static class ShaderProperties
		{
			public static readonly int TEXTURE_CURRENT_FRAME_SHADER_ID = Shader.PropertyToID("_CurrentFrame");
			public static readonly int TEXTURE_PREVIOUS_FRAME_SHADER_ID = Shader.PropertyToID("_PreviousFrame");
			public static readonly int FRAME_INDEX_SHADER_ID = Shader.PropertyToID("_FrameIndex");
		}

		private const string PASS_NAME = "Accumulation Tracer Pass";
		private readonly Material m_Material;

		private class PassData
		{
			public Material Material;
			public TextureHandle CurrentFrame;
			public RawColorHistory RawColorHistory;
		}

		public AccumulationTracerPath(Settings settings)
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
			requiresIntermediateTexture = true;

			m_Material = CoreUtils.CreateEngineMaterial(settings.accumulationTracerShader);
		}

		private static void ExecutePass(PassData data, RasterGraphContext context)
		{
			RTHandle previousFrameHandle = data.RawColorHistory.GetPreviousTexture(0);
			
			MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();
			propertyBlock.SetTexture(ShaderProperties.TEXTURE_PREVIOUS_FRAME_SHADER_ID, previousFrameHandle);
			propertyBlock.SetTexture(ShaderProperties.TEXTURE_CURRENT_FRAME_SHADER_ID, data.CurrentFrame);
			propertyBlock.SetInt(ShaderProperties.TEXTURE_PREVIOUS_FRAME_SHADER_ID, Time.renderedFrameCount);
			
			CoreUtils.DrawFullScreen(context.cmd, data.Material, propertyBlock);
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

			//TODO: history manager is always null
			if (resourceData.isActiveTargetBackBuffer || cameraData.historyManager == null)
			{
				return;
			}

			cameraData.historyManager.RequestAccess<RawColorHistory>();

			TextureHandle source = resourceData.activeColorTexture;
			TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
			destinationDesc.name = $"CameraColor-{PASS_NAME}";
			destinationDesc.clearBuffer = true;
			TextureHandle targetPass = renderGraph.CreateTexture(destinationDesc);
			
			using (var builder = renderGraph.AddRasterRenderPass<PassData>(PASS_NAME, out var passData))
			{
				passData.Material = m_Material;
				passData.CurrentFrame = source;
				passData.RawColorHistory = cameraData.historyManager.GetHistoryForWrite<RawColorHistory>();

				builder.AllowPassCulling(false);
				builder.UseTexture(source, AccessFlags.Read);
				builder.SetRenderAttachment(targetPass, 0);

				builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
			}

			resourceData.cameraColor = targetPass;
		}
	}

	#endregion Pass
}
