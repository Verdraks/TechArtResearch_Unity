
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Blob.Runtime
{
    [ExecuteInEditMode]
	//https://docs.unity3d.com/6000.0/Documentation/Manual/urp/customize/inject-render-pass-via-script.html
	public class BlobRenderCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Shader _blobFullscreenShader;
        private BlobPass _pass;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += InjectPass;
            _pass = new BlobPass(_blobFullscreenShader);
		}

		private void InjectPass(ScriptableRenderContext context, Camera cam)
		{
			_pass.ConfigureInput(_pass.GetRequiredInput());
			cam.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_pass);
		}

        private void OnDisable()
        {
            _pass?.Dispose();
			RenderPipelineManager.beginCameraRendering -= InjectPass;
		}
    }
}