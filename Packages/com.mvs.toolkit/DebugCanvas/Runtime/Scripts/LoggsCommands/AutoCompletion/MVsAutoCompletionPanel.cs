using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

namespace MVsToolkit.DebugCanvas.Commands
{
    public class MVsAutoCompletionPanel : MonoBehaviour
    {
        [SerializeField] int m_MaxPropositionsCount = 12;
        [SerializeField] int m_TxtPrewams;

        [Space]
        [SerializeField] Vector2 m_PosOffset;

        [Space]
        [SerializeField] MVsAutoCompletionDescriptionPanel m_DescriptionPanel;

        [Space]
        [SerializeField] MVsAutoCompletionTxtReference m_AutoCompletionReference;
        List<MVsAutoCompletionTxtReference> m_AutoCompletionReferences = new();

        int m_SelectedIndex = 0;
        int m_PropositionsCount = 0;

        public Action<string> OnReferenceClicked = delegate { };

        private void Start()
        {
            for (int i = 0; i < m_TxtPrewams; i++)
                CreateCompletionReference();
        }

        public void Show(Vector2 position, MVsToolkit.Commands.CommandAttribute[] commands, string initValue)
        {
            // Check size
            if (commands.Length > m_AutoCompletionReferences.Count)
                for (int i = 0; i < (commands.Length - m_AutoCompletionReferences.Count); i++)
                    CreateCompletionReference();

            m_SelectedIndex = 0;
            m_PropositionsCount = commands.Length;

            for (int i = 0; i < m_AutoCompletionReferences.Count; i++)
            {
                if (commands.Length > i)
                {
                    m_AutoCompletionReferences[i].Setup(commands[i], initValue);
                    m_AutoCompletionReferences[i].gameObject.SetActive(true);
                }
                else
                {
                    m_AutoCompletionReferences[i].gameObject.SetActive(false);
                }

                if (i == 0) m_AutoCompletionReferences[i].SetSelected();
                else m_AutoCompletionReferences[i].SetUnselected();
            }

            (transform as RectTransform).position = position + m_PosOffset;
        }

        private void Update()
        {
            if (m_AutoCompletionReferences.Count == 0)
                return;

            if (Keyboard.current == null)
                return;

            int previousIndex = m_SelectedIndex;

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                m_SelectedIndex = (m_SelectedIndex + 1) % m_PropositionsCount;
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                m_SelectedIndex = (m_SelectedIndex - 1 + m_PropositionsCount) % m_PropositionsCount;
            }

            if (previousIndex != m_SelectedIndex)
            {
                for (int i = 0; i < m_AutoCompletionReferences.Count; i++)
                {
                    if (i == m_SelectedIndex)
                        m_AutoCompletionReferences[i].SetSelected();
                    else
                        m_AutoCompletionReferences[i].SetUnselected();
                }

                ShowDescriptionPanel(m_AutoCompletionReferences[m_SelectedIndex]);
            }
        }

        public void Hide()
        {
            for (int i = 0; i < m_AutoCompletionReferences.Count; i++)
            {
                m_AutoCompletionReferences[i].SetUnselected();
                m_AutoCompletionReferences[i].gameObject.SetActive(false);
            }

            HideDescriptionPanel();
        }

        public void ShowDescriptionPanel(MVsAutoCompletionTxtReference completionRef)
        {
            m_DescriptionPanel.Show((completionRef.transform as RectTransform).position, completionRef.GetCmdDescription());

            for (int i = 0; i < m_AutoCompletionReferences.Count; i++)
            {
                if (completionRef == m_AutoCompletionReferences[i])
                {
                    m_SelectedIndex = i;
                    m_AutoCompletionReferences[i].SetSelected();
                }
                else
                    m_AutoCompletionReferences[i].SetUnselected();
            }
        }
        public void HideDescriptionPanel()
        {
            m_DescriptionPanel.Hide();
        }

        void CreateCompletionReference()
        {
            MVsAutoCompletionTxtReference obj = Instantiate(m_AutoCompletionReference, m_AutoCompletionReference.transform.parent);

            m_AutoCompletionReferences.Add(obj);
            obj.OnClicked += OnReferenceClicked;
            obj.OnSelected += ShowDescriptionPanel;
        }

        public string GetSelected() => m_AutoCompletionReferences[m_SelectedIndex].GetCmdName();
        public int GetCurentPropositionsCount() => m_PropositionsCount;
        public int GetMaxPropositionsCount() => m_MaxPropositionsCount;
    }
}