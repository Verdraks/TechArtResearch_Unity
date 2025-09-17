using System;
using System.Collections;
using UnityEngine;

public class RayMarchingGpu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int resolution = 512;
    
    [Header("References")] 
    [SerializeField] private new Renderer renderer;
    [SerializeField] private ComputeShader computeShader;
    
    private RenderTexture _renderTexture;
    public Texture _depthTexture;

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
        _renderTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        _renderTexture.Create();
        
        renderer.material.SetTexture("_BaseMap", _renderTexture);
    }

    private void DrawRender()
    {
        computeShader.SetTexture(0,"output_texture",_renderTexture);
        computeShader.SetTexture(0,"depth_texture", _depthTexture);
        computeShader.Dispatch(0,resolution/16,resolution/16,1);
    }

    private void FreeTextures()
    {
        _renderTexture.Release();
    }

    private void OnDestroy() => FreeTextures();
}