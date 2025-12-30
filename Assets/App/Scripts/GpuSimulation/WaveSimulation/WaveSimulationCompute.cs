using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class WaveSimulationCompute : MonoBehaviour
{
    private static readonly int s_WaveHeightBufferProp = Shader.PropertyToID("waveHeightBuffer");
    private static readonly int s_WaveHeightBufferSizeProp = Shader.PropertyToID("waveHeightBufferSize");
    private static readonly int s_TimeProp = Shader.PropertyToID("time");

    [FormerlySerializedAs("waveComputeShader")]
    [Header("Reference")]
    [SerializeField] private ComputeShader m_WaveComputeShader;
    [FormerlySerializedAs("waveMaterial")] [SerializeField] private Material m_WaveMaterial;
    [FormerlySerializedAs("waveMeshPrimitive")] [SerializeField] private Mesh m_WaveMeshPrimitive;
    [FormerlySerializedAs("waveMeshCount")] [SerializeField] private int m_WaveMeshCount = 64;
    
    private ComputeBuffer m_WaveBuffer;
    private Matrix4x4[] m_MeshInstanceMatrices;
    
    void Start()
    {
        m_WaveBuffer = new ComputeBuffer(m_WaveMeshCount, Marshal.SizeOf(typeof(Matrix4x4)), ComputeBufferType.Structured);
        m_MeshInstanceMatrices = new Matrix4x4[m_WaveMeshCount];
        m_WaveComputeShader.SetBuffer(0, s_WaveHeightBufferProp, m_WaveBuffer);
        m_WaveComputeShader.SetInt(s_WaveHeightBufferSizeProp, m_WaveMeshCount);
    }

    void Update()
    {
        m_WaveComputeShader.SetFloat(s_TimeProp, Time.time);
        m_WaveComputeShader.Dispatch(0, m_WaveMeshCount / 8, 1, 1);
        m_WaveBuffer.GetData(m_MeshInstanceMatrices);
        Graphics.DrawMeshInstanced(m_WaveMeshPrimitive, 0, m_WaveMaterial, m_MeshInstanceMatrices);
    }

    void OnDestroy()
    {
        m_WaveBuffer?.Release();
    }
}
