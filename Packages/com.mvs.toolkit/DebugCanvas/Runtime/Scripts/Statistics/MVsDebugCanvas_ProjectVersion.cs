using TMPro;
using UnityEngine;

namespace MVsToolkit.DebugCanvas.Statitics
{
    public class MVsDebugCanvas_ProjectVersion : MonoBehaviour
    {
        [SerializeField] TMP_Text m_DebugTxt;

        void OnEnable()
        {
            if (m_DebugTxt == null) return;

            m_DebugTxt.text = $"Version : {Application.version}";
        }
    }
}