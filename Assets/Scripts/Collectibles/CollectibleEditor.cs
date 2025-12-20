using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Collectible))]
public class CollectibleEditor : Editor
{
    SerializedProperty item;
    SerializedProperty ammoItemData;

    SerializedProperty minAmount;
    SerializedProperty maxAmount;

    SerializedProperty normalSprite;
    SerializedProperty highlightedSprite;

    void OnEnable()
    {
        item = serializedObject.FindProperty("item");
        ammoItemData = serializedObject.FindProperty("ammoItemData");

        minAmount = serializedObject.FindProperty("minAmount");
        maxAmount = serializedObject.FindProperty("maxAmount");

        normalSprite = serializedObject.FindProperty("normalSprite");
        highlightedSprite = serializedObject.FindProperty("highlightedSprite");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool hasItem = item.objectReferenceValue != null;
        bool hasAmmo = ammoItemData.objectReferenceValue != null;

        // ---------------- HEADER ----------------
        EditorGUILayout.LabelField("Collectible Data", EditorStyles.boldLabel);

        // ---------------- TYPE SELECTION ----------------
        EditorGUILayout.PropertyField(item);

        if (!hasItem)
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

            string amountLabel =
                hasAmmo ? "Ammo Amount (Mermi Sayısı)"
                        : "Item Amount";

            EditorGUILayout.LabelField(amountLabel, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(minAmount, new GUIContent("Min"));
            EditorGUILayout.PropertyField(maxAmount, new GUIContent("Max"));

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
