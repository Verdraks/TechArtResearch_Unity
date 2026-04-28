using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class PathTracerPass : ScriptableRenderPass, IDisposable
{
    #region Intern class

    private static class ShaderProperties
    {
        public static readonly int VIEW_PARAM_SHADER_ID = Shader.PropertyToID("_ViewParams");
        public static readonly int MAX_DEPTH_SHADER_ID = Shader.PropertyToID("_MaxDepth");
        public static readonly int SPHERE_BUFFER_SHADER_ID = Shader.PropertyToID("_SpheresBuffer");
        public static readonly int SPHERE_COUNT_SHADER_ID = Shader.PropertyToID("_SpheresCount");
        public static readonly int RAYS_PER_PIXEL_SHADER_ID = Shader.PropertyToID("_RaysPerPixel");
        public static readonly int FRAME_INDEX_SHADER_ID =  Shader.PropertyToID("_FrameIndex");
    }

    private class PassData
    {
        public Material Material;
        public Vector4 ViewParams;
        public int MaxDepth;
        public int RayPerPixel;
        public int FrameIndex;
        public int SpheresCount;
        public BufferHandle SphereBuffer;
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

    #endregion

    #region Constant

    private const string PASS_NAME = "Path Tracer Pass";
    private const int SPHERE_COUNT = 10;

    #endregion

    #region Fiels

    private readonly PathTracerRenderFeature.Settings m_Settings;
    private readonly Material m_Material;
    private readonly GraphicsBuffer m_SphereBuffer;

    private readonly TextureHandle m_TargetRender;

    private int m_ExecutionCount;
    
    #endregion

    #region Methods

    public PathTracerPass(PathTracerRenderFeature.Settings settings)
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        m_Settings = settings;
        m_Material = CoreUtils.CreateEngineMaterial(settings.pathTracerShader);
        m_SphereBuffer =
            new GraphicsBuffer(GraphicsBuffer.Target.Structured, SPHERE_COUNT, Marshal.SizeOf(typeof(Sphere)));

        Sphere[] spheres = new Sphere[SPHERE_COUNT];
        spheres[0] = new Sphere
        {
            center = new Vector3(0, -100.5f, 0),
            radius = 100f,
            material = new Mat { color = Color.grey }
        };

        for (int i = 1; i < SPHERE_COUNT; i++)
        {
            Sphere sphere = new Sphere
            {
                center = new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(0, 1.0f),
                    UnityEngine.Random.Range(-5.0f, 5.0f)),
                radius = UnityEngine.Random.Range(0.5f, 2f),
                material = new Mat { color = UnityEngine.Random.ColorHSV() }
            };
            spheres[i] = sphere;
        }

        m_SphereBuffer.SetData(spheres);
    }
    
    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();
        
        propertyBlock.SetVector(ShaderProperties.VIEW_PARAM_SHADER_ID, data.ViewParams);
        propertyBlock.SetInteger(ShaderProperties.MAX_DEPTH_SHADER_ID, data.MaxDepth);
        propertyBlock.SetInteger(ShaderProperties.RAYS_PER_PIXEL_SHADER_ID, data.RayPerPixel);
        propertyBlock.SetInteger(ShaderProperties.SPHERE_COUNT_SHADER_ID, data.SpheresCount);
        propertyBlock.SetBuffer(ShaderProperties.SPHERE_BUFFER_SHADER_ID, data.SphereBuffer);
        propertyBlock.SetInteger(ShaderProperties.FRAME_INDEX_SHADER_ID, data.FrameIndex);
        
        CoreUtils.DrawFullScreen(context.cmd, data.Material, propertyBlock);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PASS_NAME, out var passData))
        {
            passData.Material = m_Material;
            passData.MaxDepth = m_Settings.maxDepth;
            passData.RayPerPixel  = m_Settings.rayPerPixel;
            passData.FrameIndex = m_ExecutionCount;

            Camera currentCamera = cameraData.camera;

            float planeHeight = currentCamera.nearClipPlane *
                                Mathf.Tan(currentCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2;
            float planeWidth = planeHeight * currentCamera.aspect;
            passData.ViewParams = new Vector4(planeWidth, planeHeight, currentCamera.nearClipPlane, 0.0f);

            BufferHandle sphereBufferHandle = renderGraph.ImportBuffer(m_SphereBuffer);
            passData.SpheresCount = SPHERE_COUNT;
            passData.SphereBuffer = sphereBufferHandle;

            builder.AllowPassCulling(false);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
        
        m_ExecutionCount++;
    }

    public void Dispose()
    {
        m_SphereBuffer?.Release();
    }

    #endregion
}