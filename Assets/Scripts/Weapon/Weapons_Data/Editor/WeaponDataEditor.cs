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

    SerializedProperty pelletsPerShot;
    SerializedProperty pelletSpreadAngle;
    SerializedProperty shotgunCooldown;

    SerializedProperty sniperCooldown;
    SerializedProperty sniperPenetrationCount;

    SerializedProperty ammoType;

    SerializedProperty fireEffectPrefab;
    SerializedProperty explosionRadius;
    SerializedProperty burnDamage;
    SerializedProperty burnDuration;
    SerializedProperty tickInterval;

    SerializedProperty isMolotov;

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

        pelletsPerShot = serializedObject.FindProperty("pelletsPerShot");
        pelletSpreadAngle = serializedObject.FindProperty("pelletSpreadAngle");
        shotgunCooldown = serializedObject.FindProperty("shotgunCooldown");

        sniperCooldown = serializedObject.FindProperty("sniperCooldown");
        sniperPenetrationCount = serializedObject.FindProperty("sniperPenetrationCount");

        ammoType = serializedObject.FindProperty("ammoType");

        fireEffectPrefab = serializedObject.FindProperty("fireEffectPrefab");
        explosionRadius = serializedObject.FindProperty("explosionRadius");
        burnDamage = serializedObject.FindProperty("burnDamage");
        burnDuration = serializedObject.FindProperty("burnDuration");
        tickInterval = serializedObject.FindProperty("tickInterval");

        isMolotov = serializedObject.FindProperty("isMolotov");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(prefab);
        EditorGUILayout.PropertyField(weaponType);

        WeaponType wt = (WeaponType)weaponType.enumValueIndex;

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

        // ---- Normal / Pistol / Rifle / MG vb. (Shotgun, Sniper, Molotov HARİÇ hepsi) ----
        if (wt != WeaponType.Shotgun && wt != WeaponType.Sniper && wt != WeaponType.Molotov)
        {
            EditorGUILayout.PropertyField(isAutomatic);
        }

        // ---- Shotgun ----
        if (wt == WeaponType.Shotgun)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Shotgun Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(pelletsPerShot);
            EditorGUILayout.PropertyField(pelletSpreadAngle);
            EditorGUILayout.PropertyField(shotgunCooldown);
        }

        // ---- Sniper ----
        if (wt == WeaponType.Sniper)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Sniper Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sniperCooldown);
            EditorGUILayout.PropertyField(sniperPenetrationCount);
        }

        // ---- Molotov ----
        if (wt == WeaponType.Molotov)
        {
            // İstersen runtime için bool’u da senkron tut:
            isMolotov.boolValue = true;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Molotov Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(fireEffectPrefab);
            EditorGUILayout.PropertyField(explosionRadius);
            EditorGUILayout.PropertyField(burnDamage);
            EditorGUILayout.PropertyField(burnDuration);
            EditorGUILayout.PropertyField(tickInterval);
        }
        else
        {
            // Diğer tüm silahlarda false
            isMolotov.boolValue = false;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(ammoType);

        serializedObject.ApplyModifiedProperties();
    }
}
