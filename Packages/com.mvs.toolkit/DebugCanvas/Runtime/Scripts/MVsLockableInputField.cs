using TMPro;
using UnityEngine.InputSystem;

namespace MVsToolkit.DebugCanvas
{
    public class MVsLockableInputField : TMP_InputField
    {
        bool m_IsVerticalArrowsLocked = false;

        public void SetActiveVerticalArrows(bool isActive) => m_IsVerticalArrowsLocked = isActive;

        public override void OnUpdateSelected(UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (m_IsVerticalArrowsLocked && Keyboard.current != null)
            {
                if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                    Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    return;
                }
            }

            base.OnUpdateSelected(eventData);
        }
    }
}