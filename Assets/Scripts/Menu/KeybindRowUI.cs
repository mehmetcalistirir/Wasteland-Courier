using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class KeybindRowUI : MonoBehaviour
{
    public TMP_Text actionNameText;
    public Button rebindButton;
    public TMP_Text bindingText;

    private InputAction action;
    private int bindingIndex;

    public void Setup(InputAction action, int bindingIndex = 0)
{
    this.action = action;
    this.bindingIndex = bindingIndex;

    actionNameText.text = action.name;
    UpdateBindingText();

    rebindButton.onClick.RemoveAllListeners();
    rebindButton.onClick.AddListener(StartRebind);
}


    void UpdateBindingText()
    {
        bindingText.text = action.GetBindingDisplayString(bindingIndex);
    }

    void StartRebind()
    {
        bindingText.text = "Press key...";

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnComplete(op =>
            {
                op.Dispose();
                action.Enable();
                UpdateBindingText();
                SaveBindings();
            })
            .Start();
    }

    void SaveBindings()
    {
        string json = action.actionMap.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", json);
        PlayerPrefs.Save();
    }
}
