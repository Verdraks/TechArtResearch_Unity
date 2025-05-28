using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public class ParticleGPUCompute : MonoBehaviour
{
    private static readonly int ParticlesDataProp = Shader.PropertyToID("particles_data");
    private static readonly int ParticlesMatrixProp = Shader.PropertyToID("particles_matrix");

    [Header("Settings")]
    [SerializeField] private int particleCount = 1000;
    
    [Header("References")]
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Mesh particleMesh;
    [SerializeField] private Material particleMaterial;
    
    private ComputeBuffer _particlesDataBuffer;
    private ComputeBuffer _particlesMatrixBuffer;
    private Matrix4x4[] _particlesMatrix;
    
    private void Start()
    {
        InitializeParticles();
    }

    private void InitializeParticles()
    {
        //Init array data buffer out for rendering
        _particlesMatrix = new Matrix4x4[particleCount];
        
        _particlesDataBuffer = new ComputeBuffer(particleCount, Marshal.SizeOf<ParticleData>(), ComputeBufferType.Structured);
        //Fill initial particle data
        var particlesData = new ParticleData[particleCount];
        
        float gridSize = Mathf.Sqrt(particleCount);
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
        _particlesDataBuffer.SetData(particlesData);
        computeShader.SetBuffer(0, ParticlesDataProp, _particlesDataBuffer);
        computeShader.SetBuffer(1,ParticlesDataProp, _particlesDataBuffer);
        
        //Create buffer for particle matrices
        _particlesMatrixBuffer = new ComputeBuffer(particleCount, Marshal.SizeOf<Matrix4x4>(), ComputeBufferType.Structured);
        computeShader.SetBuffer(1, ParticlesMatrixProp, _particlesMatrixBuffer);
    }


    private void Update()
    {
        computeShader.Dispatch(0, particleCount / 16, 1, 1);
        computeShader.Dispatch(1, particleCount / 16, 1, 1);
        _particlesMatrixBuffer.GetData(_particlesMatrix);
        Graphics.DrawMeshInstanced(particleMesh,0,particleMaterial, _particlesMatrix);
    }
    
    private void OnDestroy()
    {
        _particlesDataBuffer?.Release();
        _particlesMatrixBuffer?.Release();
    }

    private struct ParticleData
    {
        public float3 Position;
    }
}
