using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RayTracing.Runtime
{
	public static class SRPHelper
	{
		#region Fields
		private static readonly Dictionary<Type, object> RENDER_PASS_CACHED = new();
		private static UniversalRenderPipelineAsset m_CurrentPipeline;
		#endregion

		#region Methods
		public static T GetRendererFeature<T>() where T : ScriptableRendererFeature
		{
			ValidateRenderPipeline();

			Type type = typeof(T);

			if (RENDER_PASS_CACHED.ContainsKey(type) && RENDER_PASS_CACHED[type] != null)
			{
				return (T)RENDER_PASS_CACHED[type];
			}

			foreach (ScriptableRendererData rendererData in m_CurrentPipeline.rendererDataList)
			{
				foreach (ScriptableRendererFeature rendererFeature in rendererData.rendererFeatures)
				{
					if (rendererFeature.GetType() != type)
					{
						continue;
					}

					RENDER_PASS_CACHED[type] = rendererFeature;

					return (T)rendererFeature;
				}
			}

			Debug.LogError($"RendererData does not contain a {type}");
			return null;
		}

		public static UniversalRenderPipelineAsset GetRenderPipeline()
		{
			ValidateRenderPipeline();
			 return m_CurrentPipeline;
		}

		private static void ValidateRenderPipeline()
		{
			if (!m_CurrentPipeline || Equals(m_CurrentPipeline, GraphicsSettings.currentRenderPipeline) == false)
			{
				m_CurrentPipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
			}
		}
		#endregion
	}
}