using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    SerializedProperty prefab;
    SerializedProperty weaponType;

    SerializedProperty knockbackForce;
    SerializedProperty knockbackDuration;

    SerializedProperty isAutomatic;
    SerializedProperty fireRate;
    SerializedProperty damage;

    SerializedProperty clipSize;
    SerializedProperty maxAmmoCapacity;
    SerializedProperty reloadTime;

    SerializedProperty attackRange;

    SerializedProperty isShotgun;
    SerializedProperty pelletsPerShot;
    SerializedProperty pelletSpreadAngle;
    SerializedProperty shotgunCooldown;

    SerializedProperty isSniper;
    SerializedProperty sniperCooldown;
    SerializedProperty sniperPenetrationCount;

    SerializedProperty ammoType;

    SerializedProperty isMolotov;
    SerializedProperty fireEffectPrefab;
    SerializedProperty explosionRadius;
    SerializedProperty burnDamage;
    SerializedProperty burnDuration;
    SerializedProperty tickInterval;

    void OnEnable()
    {
        prefab = serializedObject.FindProperty("prefab");
        weaponType = serializedObject.FindProperty("weaponType");

        knockbackForce = serializedObject.FindProperty("knockbackForce");
        knockbackDuration = serializedObject.FindProperty("knockbackDuration");

        isAutomatic = serializedObject.FindProperty("isAutomatic");
        fireRate = serializedObject.FindProperty("fireRate");
        damage = serializedObject.FindProperty("damage");

        clipSize = serializedObject.FindProperty("clipSize");
        maxAmmoCapacity = serializedObject.FindProperty("maxAmmoCapacity");
        reloadTime = serializedObject.FindProperty("reloadTime");

        attackRange = serializedObject.FindProperty("attackRange");

        isShotgun = serializedObject.FindProperty("isShotgun");
        pelletsPerShot = serializedObject.FindProperty("pelletsPerShot");
        pelletSpreadAngle = serializedObject.FindProperty("pelletSpreadAngle");
        shotgunCooldown = serializedObject.FindProperty("shotgunCooldown");

        isSniper = serializedObject.FindProperty("isSniper");
        sniperCooldown = serializedObject.FindProperty("sniperCooldown");
        sniperPenetrationCount = serializedObject.FindProperty("sniperPenetrationCount");

        ammoType = serializedObject.FindProperty("ammoType");

        isMolotov = serializedObject.FindProperty("isMolotov");
        fireEffectPrefab = serializedObject.FindProperty("fireEffectPrefab");
        explosionRadius = serializedObject.FindProperty("explosionRadius");
        burnDamage = serializedObject.FindProperty("burnDamage");
        burnDuration = serializedObject.FindProperty("burnDuration");
        tickInterval = serializedObject.FindProperty("tickInterval");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Base fields
        EditorGUILayout.PropertyField(prefab);
        EditorGUILayout.PropertyField(weaponType);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(knockbackForce);
        EditorGUILayout.PropertyField(knockbackDuration);

        EditorGUILayout.PropertyField(fireRate);
        EditorGUILayout.PropertyField(damage);
        EditorGUILayout.PropertyField(clipSize);
        EditorGUILayout.PropertyField(maxAmmoCapacity);
        EditorGUILayout.PropertyField(reloadTime);
        EditorGUILayout.PropertyField(attackRange);

        // Automatic (Pistol / MG)
        if (!isShotgun.boolValue && !isSniper.boolValue && !isMolotov.boolValue)
        {
            EditorGUILayout.PropertyField(isAutomatic);
        }

        // Shotgun
        if (isShotgun.boolValue)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Shotgun Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(pelletsPerShot);
            EditorGUILayout.PropertyField(pelletSpreadAngle);
            EditorGUILayout.PropertyField(shotgunCooldown);
        }

        // Sniper
        if (isSniper.boolValue)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Sniper Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sniperCooldown);
            EditorGUILayout.PropertyField(sniperPenetrationCount);
        }

        // Molotov
        if (isMolotov.boolValue)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Molotov Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(fireEffectPrefab);
            EditorGUILayout.PropertyField(explosionRadius);
            EditorGUILayout.PropertyField(burnDamage);
            EditorGUILayout.PropertyField(burnDuration);
            EditorGUILayout.PropertyField(tickInterval);
        }

        // Ammo selection (all guns)
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(ammoType);

        serializedObject.ApplyModifiedProperties();
    }
}
