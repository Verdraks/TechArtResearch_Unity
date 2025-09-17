using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public class WaveSimulationCompute : MonoBehaviour
{
    private static readonly int WaveHeightBufferProp = Shader.PropertyToID("waveHeightBuffer");
    private static readonly int WaveHeightBufferSizeProp = Shader.PropertyToID("waveHeightBufferSize");
    private static readonly int TimeProp = Shader.PropertyToID("time");

    [Header("Reference")]
    [SerializeField] private ComputeShader waveComputeShader;
    [SerializeField] private Material waveMaterial;
    [SerializeField] private Mesh waveMeshPrimitive;
    [SerializeField] private int waveMeshCount = 64;
    
    private ComputeBuffer _waveBuffer;
    private Matrix4x4[] _meshInstanceMatrices;
    
    void Start()
    {
        _waveBuffer = new ComputeBuffer(waveMeshCount, Marshal.SizeOf(typeof(Matrix4x4)), ComputeBufferType.Structured);
        _meshInstanceMatrices = new Matrix4x4[waveMeshCount];
        waveComputeShader.SetBuffer(0, WaveHeightBufferProp, _waveBuffer);
        waveComputeShader.SetInt(WaveHeightBufferSizeProp, waveMeshCount);
    }

    void Update()
    {
        waveComputeShader.SetFloat(TimeProp, Time.time);
        waveComputeShader.Dispatch(0, waveMeshCount / 8, 1, 1);
        _waveBuffer.GetData(_meshInstanceMatrices);
        Graphics.DrawMeshInstanced(waveMeshPrimitive, 0, waveMaterial, _meshInstanceMatrices);
    }

    void OnDestroy()
    {
        _waveBuffer?.Release();
    }
}
