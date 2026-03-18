using TMPro;
using UnityEngine;

namespace MVsToolkit.DebugCanvas.Statitics
{
    public class MVsDebugCanvas_ApplicationFPS : MonoBehaviour
    {
        [Header("Stats Settings")]
        [Tooltip("History duration in seconds")]
        [SerializeField] float m_HistoryDuration = 5f;

        [Header("UI References")]
        [SerializeField] TMP_Text m_FpsText;
        [SerializeField] TMP_Text m_AvgText;
        [SerializeField] TMP_Text m_MinText;
        [SerializeField] TMP_Text m_MaxText;

        float[] m_FrameTimes;
        int m_FrameCount;
        int m_FrameIndex;

        void Start()
        {
            int bufferSize = Mathf.CeilToInt(m_HistoryDuration / Mathf.Max(Time.fixedDeltaTime, 0.001f));
            m_FrameTimes = new float[bufferSize];
            m_FrameCount = 0;
            m_FrameIndex = 0;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (m_FrameTimes == null || m_FrameTimes.Length == 0) return;
            if (m_FrameCount == m_FrameTimes.Length)
            {
                // Rien à faire, on écrase simplement l'ancien
            }
            else
            {
                m_FrameCount++;
            }
            m_FrameTimes[m_FrameIndex] = dt;
            m_FrameIndex = (m_FrameIndex + 1) % m_FrameTimes.Length;

            // Calcul des stats
            float min = float.MaxValue;
            float max = float.MinValue;
            float sum = 0f;
            for (int i = 0; i < m_FrameCount; i++)
            {
                float t = m_FrameTimes[i];
                sum += t;
                if (t < min) min = t;
                if (t > max) max = t;
            }
            float avg = sum / m_FrameCount;
            float fps = 1f / avg;
            float minMs = min * 1000f;
            float maxMs = max * 1000f;
            float avgMs = avg * 1000f;
            if (m_FpsText != null) m_FpsText.text = $"FPS: {fps:F1}";
            if (m_AvgText != null) m_AvgText.text = $"Mid: {avgMs:F2} ms";
            if (m_MinText != null) m_MinText.text = $"Min: {minMs:F2} ms";
            if (m_MaxText != null) m_MaxText.text = $"Max: {maxMs:F2} ms";
        }
    }
}