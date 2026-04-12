using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GrassSystem
{
    [ExecuteInEditMode]
    public class GrassSystemManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Material m_GrassMaterial;

        [SerializeField] private Mesh m_GrassMesh;
        [SerializeField] private ComputeShader m_GrassComputeShader;

        private const int k_GrassDataBufferSize = 2048;
        private static readonly int s_GrassDataStride = Marshal.SizeOf(typeof(GrassData));
        private static readonly int s_GrassDataComputeBuffer = Shader.PropertyToID("grassDataBuffer");
        private static readonly int s_GrassDataShaderBuffer = Shader.PropertyToID("grassDataBuffer");

        private GraphicsBuffer m_GrassDataBuffer;
        private GraphicsBuffer m_ArgsBuffer;
        private GraphicsBuffer.IndirectDrawIndexedArgs[] m_ArgsBufferData;
        private RenderParams m_RenderParams;

        private int m_GrassDataSpawnKernelID;


        private void OnEnable()
        {
            m_GrassDataSpawnKernelID = m_GrassComputeShader.FindKernel("CSGrassDataSpawn");

            m_GrassDataBuffer =
                new GraphicsBuffer(GraphicsBuffer.Target.Structured, k_GrassDataBufferSize, s_GrassDataStride);

            m_GrassComputeShader.SetBuffer(m_GrassDataSpawnKernelID, s_GrassDataComputeBuffer, m_GrassDataBuffer);
            m_GrassComputeShader.SetInt("grassDataBufferSize", k_GrassDataBufferSize);
            m_GrassComputeShader.SetVector("areaGrassSize", new Vector3(15f, 0f, 15f));
            m_GrassComputeShader.SetVector("areaGrassCenter", Vector3.zero);

            m_ArgsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1,
                GraphicsBuffer.IndirectDrawIndexedArgs.size);

            m_ArgsBufferData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            
            
            // Exécuter le compute shader une seule fois pour initialiser le buffer
            m_GrassComputeShader.Dispatch(m_GrassDataSpawnKernelID, Mathf.CeilToInt(k_GrassDataBufferSize / 64f),
                1, 1);
        }

        private void Update()
        {
            // Graphics.DrawMeshInstancedIndirect(m_GrassMesh, 0, m_GrassMaterial, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)),
            //     m_ArgsBuffer);
            
            m_ArgsBufferData[0] = new GraphicsBuffer.IndirectDrawIndexedArgs
            {
                indexCountPerInstance = m_GrassMesh.GetIndexCount(0),
                instanceCount = k_GrassDataBufferSize
            };
            
            m_ArgsBuffer.SetData(m_ArgsBufferData);

            m_RenderParams = new RenderParams
            {
                matProps = new MaterialPropertyBlock(),
                worldBounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)),
                material = m_GrassMaterial,
            };
            // m_RenderParams.matProps.SetBuffer(s_GrassDataShaderBuffer, m_GrassDataBuffer);
            
            Graphics.RenderMeshIndirect(m_RenderParams, m_GrassMesh, m_ArgsBuffer); 
        }

        private void OnDisable()
        {
            m_GrassDataBuffer.Dispose();
            m_ArgsBuffer.Dispose();
            m_ArgsBufferData = null;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GrassData
    {
        public Vector3 Position;
    }
}