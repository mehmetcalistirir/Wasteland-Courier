using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(InputIconSet))]
public class InputIconSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        InputIconSet set = (InputIconSet)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Auto Fill From Sprites"))
        {
            AutoFill(set);
        }
    }

    void AutoFill(InputIconSet set)
    {
        set.keyboardIcons.Clear();

        // Tüm sprite’ları tara
        string[] guids = AssetDatabase.FindAssets("t:Sprite");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
                continue;

            // Örnek: keyboard_e_0
            if (!sprite.name.StartsWith("keyboard_"))
                continue;

            // keyboard_e_0 → e
            string key = sprite.name
                .Replace("keyboard_", "")
                .Split('_')[0];

            set.keyboardIcons.Add(new InputIconSet.IconEntry
            {
                control = key,
                icon = sprite
            });
        }

        EditorUtility.SetDirty(set);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ InputIconSet auto-filled");
    }
}
