using System;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;

public class GrassBladesCompute : MonoBehaviour
{

        [Header("Settings")]
        [SerializeField] private int grassBladesCount = 20;
        [SerializeField] private Bounds grassBounds = new(Vector3.zero, new Vector3(100, 0, 100));
        
        [Header("References")]
        [SerializeField] private ComputeShader grassBladesComputeShader;
        [SerializeField] private Shader grassShader;
        
        private Material _grassMaterial;
        private ComputeBuffer _bladesOriginsBuffer;
        private ComputeBuffer _bladesMeshBuffer;

        private Vector3[] _bladesOriginPos;
        private const int MaxBlades = 100000;

        private void Awake()
        {
                
                grassBladesCount = Math.Min(MaxBlades, grassBladesCount);
                
                _grassMaterial = new Material(grassShader)
                {
                        enableInstancing = true
                };
                _bladesMeshBuffer = new ComputeBuffer(grassBladesCount, 24, ComputeBufferType.Structured);
                _bladesOriginsBuffer = new ComputeBuffer(grassBladesCount, sizeof(float) *3 , ComputeBufferType.Default);
                
                grassBladesComputeShader.SetBuffer( 0,"BladesMeshBuffer", _bladesMeshBuffer);
                grassBladesComputeShader.SetBuffer(0, "BladesOriginWSBuffer", _bladesOriginsBuffer);
                
        }

        private void Start()
        {
                FillBladesOriginsBuffer();
                grassBladesComputeShader.Dispatch(0,8,8,1);
        }

        private void FillBladesOriginsBuffer()
        {
                
                _bladesOriginPos = new Vector3[grassBladesCount];
                for (int i = 0; i < grassBladesCount; i++)
                {
                        var origin = new Vector3(
                                UnityEngine.Random.Range(grassBounds.min.x, grassBounds.max.x),
                                grassBounds.min.y,
                                UnityEngine.Random.Range(grassBounds.min.z, grassBounds.max.z)
                        );
                        _bladesOriginPos[i] = origin;
                }
                _bladesOriginsBuffer.SetData(_bladesOriginPos);
        }


        private void OnDestroy()
        {
                if (_grassMaterial) Destroy(_grassMaterial);
                _bladesMeshBuffer?.Release();
        }

        private void OnDrawGizmosSelected()
        {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(grassBounds.center, grassBounds.size);
        }
}