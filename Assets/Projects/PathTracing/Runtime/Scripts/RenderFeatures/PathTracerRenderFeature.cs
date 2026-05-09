using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PathTracer
{
    public class PathTracerRenderFeature : ScriptableRendererFeature
    {
        #region Intern class

        [Serializable]
        public class Settings
        {
            public Shader pathTracerShader;
            public Shader accumulationTracerShader;
            public int maxDepth = 10;
            [Min(1)] public int rayPerPixel = 1;
            public bool useAccumulation = true;
            [Min(0)] public float defocusStrength = 0.0f;
            [Min(0)] public float divergeStrength = 0.0f;
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

        #region Properties

        public PathTracerPass PathTracerPass => m_PathTracerPass;

        #endregion
        
        #region Methods

        public override void Create()
        {
            m_PathTracerPass = new PathTracerPass(m_Settings);
            m_AccumulationTracerPass = new AccumulationTracerPass(m_Settings);
            m_WritePathTracerHistoryPass = new WritePathTracerHistoryPass(m_Settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_PathTracerPass);
            renderer.EnqueuePass(m_AccumulationTracerPass);
            renderer.EnqueuePass(m_WritePathTracerHistoryPass);
        }

        #endregion Methods
    }
}