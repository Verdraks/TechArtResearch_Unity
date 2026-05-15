using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RayTracing.Runtime
{
	public class PathTracerPass : ScriptableRenderPass, IDisposable
	{
		#region Intern Class
		private static class ShaderProperties
		{
			public static readonly int VIEW_PARAM_SHADER_ID = Shader.PropertyToID("_ViewParams");
			public static readonly int MAX_DEPTH_SHADER_ID = Shader.PropertyToID("_MaxDepth");
			public static readonly int RAYS_PER_PIXEL_SHADER_ID = Shader.PropertyToID("_RaysPerPixel");

			public static readonly int SPHERE_BUFFER_SHADER_ID = Shader.PropertyToID("_SpheresBuffer");
			public static readonly int SPHERE_COUNT_SHADER_ID = Shader.PropertyToID("_SpheresCount");

			public static readonly int TRIANGLE_BUFFER_SHADER_ID = Shader.PropertyToID("_TrianglesBuffer");
			public static readonly int MESH_BUFFER_SHADER_ID = Shader.PropertyToID("_MeshesBuffer");
			public static readonly int MESH_COUNT_SHADER_ID = Shader.PropertyToID("_MeshesCount");

			public static readonly int FRAME_INDEX_SHADER_ID = Shader.PropertyToID("_FrameIndex");

			public static readonly int DEFOCUS_STRENGTH_SHADER_ID = Shader.PropertyToID("_DefocusStrength");
			public static readonly int DIVERGE_STRENGTH_SHADER_ID = Shader.PropertyToID("_DivergeStrength");
		}

		private class PassData
		{
			public Material material;

			public Vector4 viewParams;

			public float defocusStrength;
			public float divergeStrength;

			public int maxDepth;
			public int rayPerPixel;

			public int frameIndex;

			public int spheresCount;
			public BufferHandle sphereBuffer;

			public BufferHandle meshBuffer;
			public BufferHandle triangleBuffer;
			public int meshesCount;
		}
		#endregion

		#region Constant
		private const string PASS_NAME = "Path Tracer Pass";
		private const string PATH_TRACER_TEXTURE_NAME = "PathTracerOutput";
		private const int MAX_BUFFER_SIZE = 1024;
		#endregion

		#region Fiels
		private readonly RayTracingRenderFeature.Settings m_Settings;
		private readonly Material m_Material;

		private readonly GraphicsBuffer m_SphereBuffer;
		private readonly GraphicsBuffer m_TriangleBuffer;
		private readonly GraphicsBuffer m_MeshBuffer;
		#endregion

		#region Constructors
		public PathTracerPass(RayTracingRenderFeature.Settings settings)
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
			requiresIntermediateTexture = true;

			m_Settings = settings;

			m_Material = CoreUtils.CreateEngineMaterial(settings.pathTracerShader);

			m_SphereBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_BUFFER_SIZE,
				Marshal.SizeOf(typeof(Sphere)));
			m_TriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_BUFFER_SIZE,
				Marshal.SizeOf(typeof(Triangle)));
			m_MeshBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_BUFFER_SIZE,
				Marshal.SizeOf(typeof(MeshInfo)));
		}
		#endregion Constructors

		#region Methods
		#region Static Methods

		private static bool CanExecutePass(UniversalResourceData resourceData, UniversalCameraData cameraData)
		{
			if (cameraData.cameraType == CameraType.Game || cameraData.cameraType == CameraType.SceneView)
			{
				return true;
			}
			return false;
		}

		public static RenderTextureDescriptor GetRenderTextureFormatDescription(UniversalCameraData cameraData)
		{
			RenderTextureDescriptor historyDesc = cameraData.cameraTargetDescriptor;
			historyDesc.depthBufferBits = 0;
			historyDesc.msaaSamples = 1;
			historyDesc.colorFormat = RenderTextureFormat.ARGBFloat;
			return historyDesc;
		}

		public static TextureDesc GetTextureFormatDescription(RenderGraph renderGraph, UniversalResourceData resourceData)
		{
			TextureDesc support = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
			support.name = PATH_TRACER_TEXTURE_NAME;
			support.depthBufferBits = 0;
			support.msaaSamples =  MSAASamples.None;
			support.format = GraphicsFormat.R32G32B32A32_SFloat;
			support.clearBuffer = false;
			 return support;
		}
		
		private static Vector4 GetViewParams(Camera camera)
		{
			float planeHeight = camera.nearClipPlane *
								Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2;
			float planeWidth = planeHeight * camera.aspect;

			Vector4 viewParams = new Vector4(planeWidth, planeHeight, camera.nearClipPlane, camera.farClipPlane);
			return viewParams;
		}

		private static void FetchBufferMesh(GraphicsBuffer meshBuffer, GraphicsBuffer triangleBuffer,
			out int elementCount)
		{
			MeshInfo[] meshesInfos = new MeshInfo[RayTracingMeshRenderer.Elements.Count];
			Triangle[] triangles =
				new Triangle[RayTracingMeshRenderer.Elements.Sum(o => o.MeshInfo.trianglesCount)];
			for (int i = 0; i < meshesInfos.Length; i++)
			{
				meshesInfos[i] = RayTracingMeshRenderer.Elements[i].MeshInfo;
				meshesInfos[i].firstTriangleIndex = i <= 0 ? 0 : meshesInfos[i - 1].firstTriangleIndex + meshesInfos[i-1].trianglesCount;

				Triangle[] meshTriangles = RayTracingMeshRenderer.Elements[i].Triangles;

				for (int j = 0; j < meshesInfos[i].trianglesCount; j++)
				{
					triangles[j + meshesInfos[i].firstTriangleIndex] = meshTriangles[j];
				}
			}

			triangleBuffer.SetData(triangles);
			meshBuffer.SetData(meshesInfos);
			elementCount = meshesInfos.Length;
		}

		private static void FetchBufferSphere(GraphicsBuffer sphereBuffer, out int elementCount)
		{
			sphereBuffer.SetData(RayTracingSphereRenderer.ELEMENTS.Select(o => o.Data).ToArray());
			elementCount = RayTracingSphereRenderer.ELEMENTS.Count;
		}

		private static void ExecutePass(PassData data, RasterGraphContext context)
		{
			MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();

			propertyBlock.SetVector(ShaderProperties.VIEW_PARAM_SHADER_ID, data.viewParams);
			propertyBlock.SetInteger(ShaderProperties.FRAME_INDEX_SHADER_ID, data.frameIndex);

			propertyBlock.SetInteger(ShaderProperties.SPHERE_COUNT_SHADER_ID, data.spheresCount);
			propertyBlock.SetBuffer(ShaderProperties.SPHERE_BUFFER_SHADER_ID, data.sphereBuffer);

			propertyBlock.SetInteger(ShaderProperties.MESH_COUNT_SHADER_ID, data.meshesCount);
			propertyBlock.SetBuffer(ShaderProperties.MESH_BUFFER_SHADER_ID, data.meshBuffer);
			propertyBlock.SetBuffer(ShaderProperties.TRIANGLE_BUFFER_SHADER_ID, data.triangleBuffer);

			propertyBlock.SetInteger(ShaderProperties.MAX_DEPTH_SHADER_ID, data.maxDepth);
			propertyBlock.SetInteger(ShaderProperties.RAYS_PER_PIXEL_SHADER_ID, data.rayPerPixel);
			propertyBlock.SetFloat(ShaderProperties.DEFOCUS_STRENGTH_SHADER_ID, data.defocusStrength);
			propertyBlock.SetFloat(ShaderProperties.DIVERGE_STRENGTH_SHADER_ID, data.divergeStrength);

			CoreUtils.DrawFullScreen(context.cmd, data.material, propertyBlock);
		}
		#endregion Static Methods

		#region Override Methods
		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
			UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

			if (CanExecutePass(resourceData, cameraData) == false)
			{
				return;
			}

			PathTracerFrameData pathTracerData = frameData.GetOrCreate<PathTracerFrameData>();
			RayTracingHistory history = null;

			if (cameraData.historyManager != null)
			{
				cameraData.historyManager.RequestAccess<RayTracingHistory>();
				history = cameraData.historyManager.GetHistoryForWrite<RayTracingHistory>();
			}
			
			history?.Update(GetRenderTextureFormatDescription(cameraData));
			int frameIndex = history?.FrameIndex ?? 0;

			TextureDesc desc = GetTextureFormatDescription(renderGraph, resourceData);
			TextureHandle pathTracerTexture = renderGraph.CreateTexture(desc);
			
			pathTracerData.frameIndex = frameIndex;
			pathTracerData.pathTracerTexture = pathTracerTexture;

			using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(PASS_NAME, out PassData passData))
			{
				passData.material = m_Material;
				passData.maxDepth = m_Settings.maxDepth;
				passData.rayPerPixel = m_Settings.rayPerPixel;
				passData.defocusStrength = m_Settings.defocusStrength;
				passData.divergeStrength = m_Settings.divergeStrength;

				passData.frameIndex = frameIndex;

				passData.viewParams = GetViewParams(cameraData.camera);

				FetchBufferSphere(m_SphereBuffer, out int sphereCount);
				BufferHandle sphereBufferHandle = renderGraph.ImportBuffer(m_SphereBuffer);
				passData.spheresCount = sphereCount;
				passData.sphereBuffer = sphereBufferHandle;

				FetchBufferMesh(m_MeshBuffer, m_TriangleBuffer, out int meshesCount);
				BufferHandle triangleBufferHandle = renderGraph.ImportBuffer(m_TriangleBuffer);
				BufferHandle meshBufferHandle = renderGraph.ImportBuffer(m_MeshBuffer);
				passData.triangleBuffer = triangleBufferHandle;
				passData.meshBuffer = meshBufferHandle;
				passData.meshesCount = meshesCount;

				builder.AllowPassCulling(false);
				builder.SetRenderAttachment(pathTracerTexture, 0);

				builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
			}
		}
		#endregion Override Methods

		#region Public Methods
		public void Dispose()
		{
			m_SphereBuffer?.Dispose();
			m_MeshBuffer?.Dispose();
			m_TriangleBuffer?.Dispose();
		}
		#endregion
		#endregion Methods
	}
}