using UnityEditor;
using UnityEngine;

namespace RayTracing.Runtime
{
	[ExecuteInEditMode, RequireComponent(typeof(Camera))]
	public class RayTracingCamera : MonoBehaviour
	{
#if UNITY_EDITOR
		public const string CAMERA_MODE_NAME = "Ray Tracing";

		[InitializeOnLoadMethod]
		public static void Init()
		{
			SceneView.AddCameraMode(CAMERA_MODE_NAME, "Custom");
		}
#endif
	}
}