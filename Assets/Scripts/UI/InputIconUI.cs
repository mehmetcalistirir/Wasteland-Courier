using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputIconUI : MonoBehaviour
{
    public InputActionReference action;
    public Image iconImage;
    public InputIconSet iconSet;

    void OnEnable()
    {
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        if (action == null || iconSet == null || iconImage == null)
            return;

        var binding = action.action.bindings[0];
        string path = binding.effectivePath;

        Sprite icon = iconSet.GetIconFromPath(path);

        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }
}
