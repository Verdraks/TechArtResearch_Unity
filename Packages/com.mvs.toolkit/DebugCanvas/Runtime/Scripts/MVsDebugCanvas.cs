using MVsToolkit.Preferences.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MVsToolkit.DebugCanvas
{
    public class MVsDebugCanvas : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool m_StartingActiveState = false;
        bool m_CurrentActiveState;

        [SerializeField] GameObject m_Statistics;
        [SerializeField] DebugCanvas.Commands.MVsDebugCanvas_CommandController m_Commands;

        //[Header("Input")]
        //[Header("Output")]

        private void Awake()
        {
            m_CurrentActiveState = m_StartingActiveState;

            m_Statistics.SetActive(m_StartingActiveState);

            if (MVsPrefs<MVsCommandsValues>.Values.UseCommands)
            {
                m_Commands.gameObject.SetActive(m_CurrentActiveState);

                if (m_CurrentActiveState) m_Commands.FocusCommands();
            }
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            {
                m_CurrentActiveState = !m_CurrentActiveState;

                m_Statistics.SetActive(m_CurrentActiveState);

                if (MVsPrefs<MVsCommandsValues>.Values.UseCommands)
                {
                    m_Commands.gameObject.SetActive(m_CurrentActiveState);
                    m_Commands.FocusCommands();
                }
            }
        }
    }
}