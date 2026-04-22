using System;
using System.Runtime.InteropServices;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PathTracer : ScriptableRendererFeature
{
	#region Constants
	private const string PASS_ACCUMULATION_TRACER_NAME = "Accumulation Pass";
	private const string PASS_PATH_TRACER_NAME = "Path Tracer Pass";
	#endregion Constants

	#region Fields
	[SerializeField] private PathTracerSettings m_Settings;
	private PathTracerPass m_ScriptablePass;
	#endregion Fields

	#region Methods
	public override void Create()
	{
		m_ScriptablePass = new PathTracerPass(m_Settings)
		{
			renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing,
			requiresIntermediateTexture = true
		};
	}

	protected override void Dispose(bool disposing)
	{
		m_ScriptablePass?.Dispose();
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_ScriptablePass);
	}
	#endregion Methods

	#region Intern class
	private static class ShaderProperties
	{
		public static readonly int VIEW_PARAM_SHADER_ID = Shader.PropertyToID("_ViewParams");
		public static readonly int MAX_DEPTH_SHADER_ID = Shader.PropertyToID("_MaxDepth");
		public static readonly int SPHERE_BUFFER_SHADER_ID = Shader.PropertyToID("_SpheresBuffer");
		public static readonly int SPHERE_COUNT_SHADER_ID = Shader.PropertyToID("_SpheresCount");
		public static readonly int RAYS_PER_PIXEL_SHADER_ID = Shader.PropertyToID("_RaysPerPixel");
	}

	[Serializable]
	private class PathTracerSettings
	{
		public Shader pathTracerShader;
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

	class PathTracerPass : ScriptableRenderPass, IDisposable
	{
		private readonly PathTracerSettings m_Settings;
		private readonly Material m_Material;
		private readonly GraphicsBuffer m_SphereBuffer;
		private readonly int m_SphereCount = 10;

		public PathTracerPass(PathTracerSettings settings)
		{
			m_Settings = settings;
			m_Material = CoreUtils.CreateEngineMaterial(settings.pathTracerShader);
			m_SphereBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_SphereCount, Marshal.SizeOf(typeof(Sphere)));

			Sphere[] spheres = new Sphere[m_SphereCount];
			spheres[0] = new Sphere
			{
				center = new Vector3(0, -100.5f, 0),
				radius = 100f,
				material = new Mat { color = Color.green }
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

		private class PassRasterData
		{
			public ProfilerMarker Marker;
			public Material BlitMaterial;
			public MaterialPropertyBlock PropertyBlock;
		}

		static void ExecutePass(PassRasterData data, RasterGraphContext context)
		{
			CoreUtils.DrawFullScreen(context.cmd, data.BlitMaterial, data.PropertyBlock);
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{

			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

			using (var builder = renderGraph.AddRasterRenderPass<PassRasterData>(PASS_PATH_TRACER_NAME, out var passData))
			{
				passData.BlitMaterial = m_Material;
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

				builder.SetRenderFunc((PassRasterData data, RasterGraphContext context) => ExecutePass(data, context));
			}





			//RenderGraphUtils.BlitMaterialParameters blitParam = new RenderGraphUtils.BlitMaterialParameters()
			//{
			//	destination = resourceData.activeColorTexture,
			//};

			//using (var builder = renderGraph.AddBlitPass(blitParam, passName: PASS_ACCUMULATION_TRACER_NAME))
			//{
			
			//}

		}

		public void Dispose()
		{
			m_SphereBuffer?.Release();
		}
	}
}
