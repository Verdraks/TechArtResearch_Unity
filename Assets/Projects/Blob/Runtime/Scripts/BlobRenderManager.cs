
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blob.Runtime
{
    [ExecuteInEditMode]
    public class BlobRenderManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Shader _blobFullscreenShader;
        
        private CommandBuffer _cmd;
        private GraphicsBuffer _buffer;
        private Material _material;

        
        
        private void OnEnable()
        {
            _cmd = new CommandBuffer();
            _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1024, Marshal.SizeOf<BlobData>());
            _material = new Material(_blobFullscreenShader);
        }

        private void OnValidate()
        {
            OnDisable();
            OnEnable();
        }

        private void OnDisable()
        {
            _cmd?.Dispose();
            _buffer?.Dispose();
            _cmd = null;
            _buffer = null;
            _material =  null;
        }
        

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            return;
            BlobRenderer[] renderers = FindObjectsByType<BlobRenderer>(FindObjectsSortMode.None);
            int count = renderers.Length;
            BlobData[] data = renderers.Select(o=> o.BlobData).ToArray();
            
            _buffer.SetData(data);
            //
            // _material.SetBuffer(ShaderProperties.BlobBuffer, _buffer);
            // _material.SetInt(ShaderProperties.BlobCount, count);
            //
            // CoreUtils.DrawFullScreen(_cmd,  _material);
        }
    }
}