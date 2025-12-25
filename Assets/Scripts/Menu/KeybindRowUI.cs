using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class KeybindRowUI : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text actionNameText;

    [Header("Button")]
    public Button rebindButton;

    [Header("Icon")]
    public Image bindingIcon;
    public InputIconSet iconSet;

    private InputAction action;
    private int bindingIndex;

    public void Setup(InputAction action, int bindingIndex = 0)
    {
        this.action = action;
        this.bindingIndex = bindingIndex;

        // 🔤 Aksiyon adı görünür
        actionNameText.text = action.name;

        UpdateBindingUI();

        rebindButton.onClick.RemoveAllListeners();
        rebindButton.onClick.AddListener(StartRebind);
    }

    void UpdateBindingUI()
    {
        if (bindingIcon == null || iconSet == null || action == null)
            return;

        string path = action.bindings[bindingIndex].effectivePath;
        Sprite icon = iconSet.GetIconFromPath(path);

        if (icon != null)
        {
            bindingIcon.sprite = icon;
            bindingIcon.enabled = true;
        }
        else
        {
            bindingIcon.enabled = false;
        }
    }

    void StartRebind()
    {
        if (bindingIcon != null)
            bindingIcon.enabled = false;

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnComplete(op =>
            {
                op.Dispose();
                action.Enable();

                UpdateBindingUI();   // 🔥 ICON REBIND SONRASI GÜNCELLENİR
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
