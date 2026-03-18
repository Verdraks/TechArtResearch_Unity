using TMPro;
using UnityEngine;

namespace MVsToolkit.DebugCanvas.Commands
{
    public class MVsAutoCompletionDescriptionPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Txt;
        [SerializeField] RectTransform m_AutoCompletionPanelRect;
        [SerializeField] Vector2 m_PosOffset;

        public void Hide()
        {
            m_Txt.text = string.Empty;
        }

        public void Show(Vector2 position, string str)
        {
            m_Txt.text = str;
            (transform as RectTransform).position = position + m_PosOffset + new Vector2(m_AutoCompletionPanelRect.rect.width, 0);
        }
    }
}