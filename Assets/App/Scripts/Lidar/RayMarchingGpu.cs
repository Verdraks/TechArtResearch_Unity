using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class RayMarchingGpu : MonoBehaviour
{
    [FormerlySerializedAs("resolution")]
    [Header("Settings")]
    [SerializeField] private int m_Resolution = 512;
    
    [FormerlySerializedAs("renderer")]
    [Header("References")] 
    [SerializeField] private new Renderer m_Renderer;
    [FormerlySerializedAs("computeShader")] [SerializeField] private ComputeShader m_ComputeShader;
    
    private RenderTexture m_RenderTexture;
    [FormerlySerializedAs("_depthTexture")] public Texture DepthTexture;

    private void Start()
    {
        SetupRender();
    }

    private void LateUpdate()
    {
        DrawRender();
    }
    
    private void SetupRender()
    {
        m_RenderTexture = new RenderTexture(m_Resolution, m_Resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        m_RenderTexture.Create();
        
        m_Renderer.material.SetTexture("_BaseMap", m_RenderTexture);
    }

    private void DrawRender()
    {
        m_ComputeShader.SetTexture(0,"output_texture",m_RenderTexture);
        m_ComputeShader.SetTexture(0,"depth_texture", DepthTexture);
        m_ComputeShader.Dispatch(0,m_Resolution/16,m_Resolution/16,1);
    }

    private void FreeTextures()
    {
        m_RenderTexture.Release();
    }

    private void OnDestroy() => FreeTextures();
}