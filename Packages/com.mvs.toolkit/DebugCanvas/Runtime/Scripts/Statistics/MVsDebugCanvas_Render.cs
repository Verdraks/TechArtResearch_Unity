using UnityEngine;
using TMPro;
using Unity.Profiling;

namespace MVsToolkit.DebugCanvas.Statitics
{
    public class MVsDebugCanvas_Render : MonoBehaviour
    {
        [Header("Références TMP_Text")]
        public TMP_Text batchesText;
        public TMP_Text vertsText;
        public TMP_Text trisText;

        private ProfilerRecorder batchesRecorder;
        private ProfilerRecorder vertsRecorder;
        private ProfilerRecorder trisRecorder;

        void OnEnable()
        {
            batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            vertsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
            trisRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        }

        void Update()
        {
            int batches = (int)batchesRecorder.LastValue;
            int verts = (int)vertsRecorder.LastValue;
            int tris = (int)trisRecorder.LastValue;

            batchesText.text = $"Batches : {batches}";
            vertsText.text = $"Verts : {verts}";
            trisText.text = $"Tris : {tris}";
        }

        void OnDisable()
        {
            batchesRecorder.Dispose();
            vertsRecorder.Dispose();
            trisRecorder.Dispose();
        }
    }
}