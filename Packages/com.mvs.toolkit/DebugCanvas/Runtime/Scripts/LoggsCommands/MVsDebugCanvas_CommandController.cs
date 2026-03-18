using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using MVsToolkit.Commands;

namespace MVsToolkit.DebugCanvas.Commands
{
    public class MVsDebugCanvas_CommandController : MonoBehaviour
    {
        [SerializeField] Color m_ErrorColor = Color.red;

        [Header("References")]
        [SerializeField] MVsAutoCompletionPanel m_AutoCompletionPanel;

        [Space]
        [SerializeField] DebugCanvas.MVsLockableInputField m_CmdInputField;

        [Space]
        [SerializeField] Transform m_LogsContent;
        [SerializeField] TMP_Text m_LogTxtRef;

        private void Awake()
        {
            m_AutoCompletionPanel.OnReferenceClicked += CompleteInputField;
        }

        private void Start()
        {
            m_LogsContent.gameObject.SetActive(false);
            m_CmdInputField.onValueChanged.AddListener(OnInputChange);
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.tabKey.wasPressedThisFrame && m_AutoCompletionPanel.GetCurentPropositionsCount() > 0)
            {
                CompleteInputField(m_AutoCompletionPanel.GetSelected());
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame
                && m_CmdInputField.text != string.Empty)
            {
                string text = m_CmdInputField.text;
                m_CmdInputField.text = string.Empty;

                if (MVsCommandManager.TryGetCmd(text, out CommandAttribute cmd))
                {
                    HandleNewLogg(text, Color.white);
                    MVsCommandManager.Execute(text);
                }
                else
                {
                    HandleNewLogg($"The command '{text}' does not exist", m_ErrorColor);
                }

                m_CmdInputField.ActivateInputField();
            }
        }

        void CompleteInputField(string str)
        {
            m_CmdInputField.SetTextWithoutNotify(str);
            m_CmdInputField.caretPosition = str.Length;

            m_AutoCompletionPanel.Hide();
        }

        public void FocusCommands()
        {
            m_CmdInputField.ActivateInputField();
        }

        void OnInputChange(string value)
        {
            CommandAttribute[] cmds = MVsCommandManager.QueryCommands(value, m_AutoCompletionPanel.GetMaxPropositionsCount());
            m_CmdInputField.SetActiveVerticalArrows(cmds.Length > 0);

            TMP_Text inputTxt = m_CmdInputField.textComponent;
            Vector2 pos = new Vector2(inputTxt.GetPreferredValues(value).x, 0);

            m_AutoCompletionPanel.Show(pos, cmds, value);
        }

        void HandleNewLogg(string msg, Color color)
        {
            TMP_Text txt = Instantiate(m_LogTxtRef, m_LogsContent);
            txt.text = msg;
            txt.color = color;

            m_LogsContent.gameObject.SetActive(true);
            txt.gameObject.SetActive(true);
        }

        [Command("Help", "Display every commands in the logg console")]
        void HelpCommand()
        {
            foreach (var cmd in MVsCommandManager.GetCommands())
            {
                HandleNewLogg(cmd.CmdName + (cmd.CmdDescription != string.Empty ? (" : <i>" + cmd.CmdDescription + "</i>") : ""), Color.yellow);
            }
        }
    }
}