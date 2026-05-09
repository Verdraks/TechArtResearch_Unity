using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PathTracer
{
    public class PathTracerPass : ScriptableRenderPass, IDisposable
    {
        #region Intern class

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
            
            public static readonly int FRAME_INDEX_SHADER_ID =  Shader.PropertyToID("_FrameIndex");
            
            public static readonly int DEFOCUS_STRENGTH_SHADER_ID = Shader.PropertyToID("_DefocusStrength");
            public static readonly int DIVERGE_STRENGTH_SHADER_ID = Shader.PropertyToID("_DivergeStrength");
        }

        private class PassData
        {
            public Material Material;
            
            public Vector4 ViewParams;
            
            public float DefocusStrength;
            public float DivergeStrength;
            
            public int MaxDepth;
            public int RayPerPixel;
            
            public int FrameIndex;
            
            public int SpheresCount;
            public BufferHandle SphereBuffer;
            
            public BufferHandle MeshBuffer;
            public BufferHandle TriangleBuffer;
            public int MeshesCount;
        }

        #endregion

        #region Constant

        private const string PASS_NAME = "Path Tracer Pass";
        private const int MAX_BUFFER_SIZE = 1024;

        #endregion

        #region Fiels

        private readonly PathTracerRenderFeature.Settings m_Settings;
        private readonly Material m_Material;
        
        private readonly TextureHandle m_TargetRender;
        
        private GraphicsBuffer m_SphereBuffer;
        private GraphicsBuffer m_TriangleBuffer;
        private GraphicsBuffer m_MeshBuffer;
        
        private int m_ExecutionCount;
    
        #endregion
        
        #region Methods

        public PathTracerPass(PathTracerRenderFeature.Settings settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            m_Settings = settings;
            
            m_Material = CoreUtils.CreateEngineMaterial(settings.pathTracerShader);
            
            m_SphereBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_BUFFER_SIZE, Marshal.SizeOf(typeof(Sphere)));
            m_TriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_BUFFER_SIZE, Marshal.SizeOf(typeof(Triangle)));
            m_MeshBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_BUFFER_SIZE, Marshal.SizeOf(typeof(MeshInfo)));
        }
    
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            MaterialPropertyBlock propertyBlock = context.renderGraphPool.GetTempMaterialPropertyBlock();
        
            propertyBlock.SetVector(ShaderProperties.VIEW_PARAM_SHADER_ID, data.ViewParams);
            propertyBlock.SetInteger(ShaderProperties.FRAME_INDEX_SHADER_ID, data.FrameIndex);
            
            propertyBlock.SetInteger(ShaderProperties.SPHERE_COUNT_SHADER_ID, data.SpheresCount);
            propertyBlock.SetBuffer(ShaderProperties.SPHERE_BUFFER_SHADER_ID, data.SphereBuffer);
            
            propertyBlock.SetInteger(ShaderProperties.MESH_COUNT_SHADER_ID, data.MeshesCount);
            propertyBlock.SetBuffer(ShaderProperties.MESH_BUFFER_SHADER_ID, data.MeshBuffer);
            propertyBlock.SetBuffer(ShaderProperties.TRIANGLE_BUFFER_SHADER_ID, data.TriangleBuffer);
            
            propertyBlock.SetInteger(ShaderProperties.MAX_DEPTH_SHADER_ID, data.MaxDepth);
            propertyBlock.SetInteger(ShaderProperties.RAYS_PER_PIXEL_SHADER_ID, data.RayPerPixel);
            propertyBlock.SetFloat(ShaderProperties.DEFOCUS_STRENGTH_SHADER_ID, data.DefocusStrength);
            propertyBlock.SetFloat(ShaderProperties.DIVERGE_STRENGTH_SHADER_ID, data.DivergeStrength);
        
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
                passData.DefocusStrength = m_Settings.defocusStrength;
                passData.DivergeStrength = m_Settings.divergeStrength;
                if (m_Settings.useAccumulation == false)
                {
                    passData.FrameIndex = 0;
                }
                else
                {
                    passData.FrameIndex = m_ExecutionCount;
                    m_ExecutionCount++;
                }

                Camera currentCamera = cameraData.camera;

                float planeHeight = currentCamera.nearClipPlane *
                                    Mathf.Tan(currentCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2;
                float planeWidth = planeHeight * currentCamera.aspect;
                passData.ViewParams = new Vector4(planeWidth, planeHeight, currentCamera.nearClipPlane, currentCamera.farClipPlane);
                
                m_SphereBuffer.SetData(PathTracingSphereRenderer.Elements.Select(o=> o.Data).ToArray());
                BufferHandle sphereBufferHandle = renderGraph.ImportBuffer(m_SphereBuffer);
                passData.SpheresCount = PathTracingSphereRenderer.Elements.Count;
                passData.SphereBuffer = sphereBufferHandle;
                
                MeshInfo[] meshesInfos = new MeshInfo [PathTracingMeshRenderer.Elements.Count];
                Triangle[] triangles = new Triangle[PathTracingMeshRenderer.Elements.Sum(o => o.MeshInfo.trianglesCount)];
                for (int i = 0; i < meshesInfos.Length; i++)
                {
                    meshesInfos[i] = PathTracingMeshRenderer.Elements[i].MeshInfo;
                    meshesInfos[i].firstTriangleIndex =  i <= 0 ? 0 : meshesInfos[i-1].trianglesCount-1;
                    
                    Triangle[] meshTriangles = PathTracingMeshRenderer.Elements[i].Triangles;
                    
                    for (int j = 0; j < meshesInfos[i].trianglesCount; j++)
                    {
                        triangles[j + meshesInfos[i].firstTriangleIndex] = meshTriangles[j];
                    }
                }
                
                m_TriangleBuffer.SetData(triangles);
                BufferHandle triangleBufferHandle = renderGraph.ImportBuffer(m_TriangleBuffer);
                passData.TriangleBuffer = triangleBufferHandle;
                
                m_MeshBuffer.SetData(meshesInfos);
                BufferHandle meshBufferHandle = renderGraph.ImportBuffer(m_MeshBuffer);
                passData.MeshBuffer = meshBufferHandle;
                passData.MeshesCount = meshesInfos.Length;

                builder.AllowPassCulling(false);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
        #endregion

        public void Dispose()
        {
            m_SphereBuffer?.Dispose();
            m_MeshBuffer?.Dispose();
        }
    }
}