using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MVsToolkit.Commands;

namespace MVsToolkit.DebugCanvas.Commands
{
    public class MVsAutoCompletionTxtReference : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] Color m_OutlineSelectedColor;
        [SerializeField] Color m_TextSelectedColor;

        [Space]
        [SerializeField] Image m_Outline;
        [SerializeField] TMP_Text m_Text;

        CommandAttribute m_Cmd;

        public Action<string> OnClicked;
        public Action<MVsAutoCompletionTxtReference> OnSelected;

        string m_HighlightedYellow;
        string m_HighlightedBlue;

        public void Setup(CommandAttribute atr, string initValue)
        {
            m_Cmd = atr;
            string cmd = atr.CmdName;

            if (!string.IsNullOrEmpty(initValue))
            {
                int index = cmd.IndexOf(initValue, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    string before = cmd.Substring(0, index);
                    string match = cmd.Substring(index, initValue.Length);
                    string after = cmd.Substring(index + initValue.Length);

                    m_HighlightedYellow = $"{before}<color=yellow>{match}</color>{after}";
                    m_HighlightedBlue = $"{before}<color=blue>{match}</color>{after}";
                }
                else
                {
                    m_HighlightedYellow = cmd;
                    m_HighlightedBlue = cmd;
                }
            }
            else
            {
                m_HighlightedYellow = cmd;
                m_HighlightedBlue = cmd;
            }

            m_Text.text = m_HighlightedYellow;
        }


        public void SetSelected()
        {
            m_Text.color = m_TextSelectedColor;
            m_Outline.color = m_OutlineSelectedColor;
            m_Text.text = m_HighlightedBlue;
        }

        public void SetUnselected()
        {
            m_Text.color = Color.white;
            m_Outline.color = new Color(0, 0, 0, 0);
            m_Text.text = m_HighlightedYellow;
        }

        public string GetCmdName() => m_Cmd.CmdName;
        public string GetCmdDescription() => m_Cmd.CmdDescription;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSelected.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked.Invoke(m_Cmd.CmdName);
        }
    }
}