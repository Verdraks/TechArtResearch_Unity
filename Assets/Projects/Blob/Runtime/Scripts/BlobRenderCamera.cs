
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
        [SerializeField] private Material  _blobFullscreenMat;
        private BlobPass _pass;

        private void OnValidate()
        {
	        OnDisable();
	        OnEnable();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += InjectPass;
            _pass = CreatePass();
		}

        private BlobPass CreatePass()
        {
	        if (_blobFullscreenMat == null)
	        {
		        return null;
	        }
	        
	        BlobPass pass  = new BlobPass(_blobFullscreenMat);
	        return pass;
        }
        
		private void InjectPass(ScriptableRenderContext context, Camera cam)
		{
			if (_pass == null)
			{
				return;
			}

			// _pass.ConfigureInput(_pass.GetRequiredInput());
			cam.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_pass);
		}

        private void OnDisable()
        {
            _pass?.Dispose();
            _pass = null;
			RenderPipelineManager.beginCameraRendering -= InjectPass;
		}
    }
}