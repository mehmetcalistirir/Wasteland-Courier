using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Collectible))]
public class CollectibleEditor : Editor
{
    SerializedProperty item;
    SerializedProperty minAmount;
    SerializedProperty maxAmount;

    SerializedProperty ammoItemData;


    SerializedProperty normalSprite;
    SerializedProperty highlightedSprite;

    void OnEnable()
    {
        item = serializedObject.FindProperty("item");
        minAmount = serializedObject.FindProperty("minAmount");
        maxAmount = serializedObject.FindProperty("maxAmount");

        ammoItemData = serializedObject.FindProperty("ammoItemData");


        normalSprite = serializedObject.FindProperty("normalSprite");
        highlightedSprite = serializedObject.FindProperty("highlightedSprite");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool hasItem = item.objectReferenceValue != null;
        bool hasAmmo = ammoItemData.objectReferenceValue != null;

        // ---------------- MODE INFO ----------------
        EditorGUILayout.LabelField("Collectible Data", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(item);
        EditorGUILayout.PropertyField(ammoItemData);

        // ---------------- VALIDATION ----------------
        if (hasItem && hasAmmo)
        {
            EditorGUILayout.HelpBox(
                "Bu Collectible hem Item hem Ammo içeriyor! Sadece biri dolu olmalı.",
                MessageType.Error
            );
        }
        else if (!hasItem && !hasAmmo)
        {
            EditorGUILayout.HelpBox(
                "Bu Collectible hiçbir şey vermiyor (Item veya Ammo seçmelisin).",
                MessageType.Warning
            );
        }

        // ---------------- AMOUNT ----------------
        if (hasItem || hasAmmo)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Amount Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(minAmount);
            EditorGUILayout.PropertyField(maxAmount);

            if (minAmount.intValue > maxAmount.intValue)
            {
                EditorGUILayout.HelpBox(
                    "Min Amount, Max Amount'tan büyük olamaz!",
                    MessageType.Warning
                );
            }
        }

        // ---------------- VISUAL ----------------
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(normalSprite);
        EditorGUILayout.PropertyField(highlightedSprite);

        serializedObject.ApplyModifiedProperties();
    }
}
