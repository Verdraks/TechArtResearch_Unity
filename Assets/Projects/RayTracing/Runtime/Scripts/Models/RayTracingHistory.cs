using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracing.Runtime
{
    public class RayTracingHistory : CameraHistoryItem
    {
        private const string HISTORY_NAME = "RayTracingHistory";

        private Hash128 m_DescKey;
        private int m_HistoryId;
        private int m_FrameIndex;

        public RTHandle CurrentFrame
        {
            get { return GetCurrentFrameRT(m_HistoryId); }
        }

        public RTHandle PreviousFrame
        {
            get { return GetPreviousFrameRT(m_HistoryId); }
        }

        public int FrameIndex
        {
            get { return m_FrameIndex; }
        }

        public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
        {
            base.OnCreate(owner, typeId);
            m_HistoryId = MakeId(0);
            m_FrameIndex = 0;
        }

        public override void Reset()
        {
            ReleaseHistoryFrameRT(m_HistoryId);
            m_FrameIndex = 0;
        }
        
        public void Increment()
        {
            m_FrameIndex++;
        }

        public void Update(RenderTextureDescriptor descriptor)
        {
            Hash128 descKey = Hash128.Compute(ref descriptor);
            if (m_DescKey != descKey)
            {
                Reset();
                m_DescKey = descKey;
            }

            if (CurrentFrame == null)
            {
                AllocHistoryFrameRT(m_HistoryId, 2, ref descriptor, HISTORY_NAME);
            }
        }

    }
}