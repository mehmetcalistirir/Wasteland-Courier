using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Input/Input Icon Set")]
public class InputIconSet : ScriptableObject
{
    [System.Serializable]
    public class IconEntry
    {
        public string control;
        public Sprite icon;
    }

    public List<IconEntry> keyboardIcons = new();

    public Sprite GetIconFromPath(string controlPath)
    {
        if (string.IsNullOrEmpty(controlPath))
            return null;

        string key = controlPath.Substring(controlPath.LastIndexOf('/') + 1);

        foreach (var entry in keyboardIcons)
        {
            if (entry.control == key)
                return entry.icon;
        }

        return null;
    }
}
