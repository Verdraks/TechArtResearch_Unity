using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RayTracing.Runtime
{
	public class RayTracingRenderFeature : ScriptableRendererFeature
	{
		#region Intern class
		[Serializable]
		public class Settings
		{
			public Shader pathTracerShader;
			public Shader accumulationTracerShader;
			[Space] [Min(1)] public int maxDepth = 10;
			[Min(1)] public int rayPerPixel = 1;
			[Space] 
			[Min(0.1f)] public float defocusStrength = 0.1f;
			[Min(0.1f)] public float divergeStrength = 0.1f;
		}
		#endregion Intern class

		#region Fields
		#region Serialized
		[SerializeField] private Settings m_Settings;
		#endregion Serialized

		#region Private
		private PathTracerPass m_PathTracerPass;
		private AccumulationTracerPass m_AccumulationTracerPass;
		private WritePathTracerHistoryPass m_WritePathTracerHistoryPass;
		#endregion Private
		#endregion Fields

		private static bool CanExecuteRenderFeature(ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType == CameraType.Game)
			{
				if (renderingData.cameraData.camera.GetComponent<RayTracingCamera>() == null)
				{
					return false;
				}

				return true;
			}
#if UNITY_EDITOR
			if (renderingData.cameraData.cameraType == CameraType.SceneView)
			{
				if (SceneView.currentDrawingSceneView.cameraMode.name != RayTracingCamera.CAMERA_MODE_NAME)
				{
					return false;
				}

				return true;
			}
#endif
			return false;
		}

		#region Override Methods
		public override void Create()
		{
			m_PathTracerPass = new PathTracerPass(m_Settings);
			m_AccumulationTracerPass = new AccumulationTracerPass(m_Settings);
			m_WritePathTracerHistoryPass = new WritePathTracerHistoryPass(m_Settings);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (CanExecuteRenderFeature(ref renderingData) == false)
			{
				return;
			}

			renderer.EnqueuePass(m_PathTracerPass);
			renderer.EnqueuePass(m_AccumulationTracerPass);
			renderer.EnqueuePass(m_WritePathTracerHistoryPass);
		}

		protected override void Dispose(bool disposing)
		{
			m_PathTracerPass.Dispose();
		}
		#endregion Override Methods
	}
}