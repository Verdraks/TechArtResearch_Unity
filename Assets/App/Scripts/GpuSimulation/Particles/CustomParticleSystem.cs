using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace GpuSimulation
{
    public class CustomParticleSystem : MonoBehaviour
    {
        private static readonly int s_ParticlesDataProp = Shader.PropertyToID("particles_data");
        private static readonly int s_ParticlesMatrixProp = Shader.PropertyToID("particles_matrix");

        [FormerlySerializedAs("particleCount")]
        [Header("Settings")]
        [SerializeField] private int m_ParticleCount = 1000;
    
        [FormerlySerializedAs("computeShader")]
        [Header("References")]
        [SerializeField] private ComputeShader m_ComputeShader;
        [FormerlySerializedAs("particleMesh")] [SerializeField] private Mesh m_ParticleMesh;
        [FormerlySerializedAs("particleMaterial")] [SerializeField] private Material m_ParticleMaterial;
    
        private ComputeBuffer m_ParticlesDataBuffer;
        private ComputeBuffer m_ParticlesMatrixBuffer;
        private Matrix4x4[] m_ParticlesMatrix;
    
        private void Start()
        {
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            //Init array data buffer out for rendering
            m_ParticlesMatrix = new Matrix4x4[m_ParticleCount];
        
            m_ParticlesDataBuffer = new ComputeBuffer(m_ParticleCount, Marshal.SizeOf<ParticleData>(), ComputeBufferType.Structured);
            //Fill initial particle data
            var particlesData = new ParticleData[m_ParticleCount];
        
            float gridSize = Mathf.Sqrt(m_ParticleCount);
            float offset = gridSize * 2.0f / 2.0f;
        
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int index = i * (int)gridSize + j;
                    particlesData[index] = new ParticleData
                    {
                        Position = new float3(i * 2.0f - offset, 0, j * 2.0f - offset)
                    };
                }
            }
            m_ParticlesDataBuffer.SetData(particlesData);
            m_ComputeShader.SetBuffer(0, s_ParticlesDataProp, m_ParticlesDataBuffer);
            m_ComputeShader.SetBuffer(1,s_ParticlesDataProp, m_ParticlesDataBuffer);
        
            //Create buffer for particle matrices
            m_ParticlesMatrixBuffer = new ComputeBuffer(m_ParticleCount, Marshal.SizeOf<Matrix4x4>(), ComputeBufferType.Structured);
            m_ComputeShader.SetBuffer(1, s_ParticlesMatrixProp, m_ParticlesMatrixBuffer);
        }


        private void Update()
        {
            m_ComputeShader.Dispatch(0, m_ParticleCount / 16, 1, 1);
            m_ComputeShader.Dispatch(1, m_ParticleCount / 16, 1, 1);
            m_ParticlesMatrixBuffer.GetData(m_ParticlesMatrix);
            Graphics.DrawMeshInstanced(m_ParticleMesh,0,m_ParticleMaterial, m_ParticlesMatrix);
        }
    
        private void OnDestroy()
        {
            m_ParticlesDataBuffer?.Release();
            m_ParticlesMatrixBuffer?.Release();
        }

        private struct ParticleData
        {
            public float3 Position;
        }
    }
}