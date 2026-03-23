using UI;
using UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuTestSwitcher : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
           UIManager.Instance.ShowUI("MainMenu");
           Debug.Log("Pressed Digit1");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            UIManager.Instance.ShowUI("SettingsMenu");
            Debug.Log("Pressed Digit2");
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            UIManager.Instance.ShowUI("PauseMenu");
            Debug.Log("Pressed Digit3");
        }
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            UIManager.Instance.HideUI("MainMenu");
            Debug.Log("Pressed Digit4");
        }
    }
}
