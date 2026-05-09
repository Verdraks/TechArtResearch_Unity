using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PathTracer
{
    public static class SRPHelper
    {
        private static Dictionary<Type, object> m_RenderPassCached = new();
        
        private static UniversalRenderPipelineAsset m_CurrentPipeline;
        
        public static T GetRendererFeature<T>() where T : ScriptableRendererFeature
        {
            ValidateRenderPipeline();
            
            Type type = typeof(T);

            if (m_RenderPassCached.ContainsKey(type) && m_RenderPassCached[type] != null)
            {
                return (T)m_RenderPassCached[type];
            }
            
            foreach (ScriptableRendererData rendererData in m_CurrentPipeline.rendererDataList)
            {
                foreach (ScriptableRendererFeature rendererFeature in rendererData.rendererFeatures)
                {
                    if (rendererFeature.GetType() != type)
                    {
                        continue;
                    }
                    
                    m_RenderPassCached[type] = rendererFeature;
                    
                    return (T)rendererFeature;
                }
            }
            
            Debug.LogError($"RendererData does not contain a {type}");
            return default(T);
        }

        private static void ValidateRenderPipeline()
        {
            if (!m_CurrentPipeline || Equals(m_CurrentPipeline, GraphicsSettings.currentRenderPipeline) == false)
            {
                m_CurrentPipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline ;
            }
        }
    }
}