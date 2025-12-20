using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(WeaponDefinition))]
public class WeaponDataEditor : Editor
{
    // Core
    SerializedProperty prefab;
    SerializedProperty weaponType;
    // Projectile
    SerializedProperty projectilePrefab;

    // Combat
    SerializedProperty knockbackForce;
    SerializedProperty knockbackDuration;
    SerializedProperty isAutomatic;
    SerializedProperty fireRate;
    SerializedProperty damage;
    SerializedProperty reloadTime;
    SerializedProperty attackRange;

    // Shotgun
    SerializedProperty pelletsPerShot;
    SerializedProperty pelletSpreadAngle;
    SerializedProperty shotgunCooldown;

    // Sniper
    SerializedProperty sniperCooldown;
    SerializedProperty sniperPenetrationCount;

    // Ammo & Magazine
    SerializedProperty ammoType;
    SerializedProperty acceptedMagazines;

    // Molotov
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
        projectilePrefab = serializedObject.FindProperty("projectilePrefab");


        knockbackForce = serializedObject.FindProperty("knockbackForce");
        knockbackDuration = serializedObject.FindProperty("knockbackDuration");

        isAutomatic = serializedObject.FindProperty("isAutomatic");
        fireRate = serializedObject.FindProperty("fireRate");
        damage = serializedObject.FindProperty("damage");
        reloadTime = serializedObject.FindProperty("reloadTime");
        attackRange = serializedObject.FindProperty("attackRange");

        pelletsPerShot = serializedObject.FindProperty("pelletsPerShot");
        pelletSpreadAngle = serializedObject.FindProperty("pelletSpreadAngle");
        shotgunCooldown = serializedObject.FindProperty("shotgunCooldown");

        sniperCooldown = serializedObject.FindProperty("sniperCooldown");
        sniperPenetrationCount = serializedObject.FindProperty("sniperPenetrationCount");

        ammoType = serializedObject.FindProperty("ammoType");
        acceptedMagazines = serializedObject.FindProperty("acceptedMagazines");

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

        // ---------------- BASIC ----------------
        EditorGUILayout.PropertyField(prefab);
        EditorGUILayout.PropertyField(weaponType);

        WeaponType wt = (WeaponType)weaponType.enumValueIndex;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Combat Stats", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(knockbackForce);
        EditorGUILayout.PropertyField(knockbackDuration);
        EditorGUILayout.PropertyField(fireRate);
        EditorGUILayout.PropertyField(damage);
        EditorGUILayout.PropertyField(reloadTime);
        EditorGUILayout.PropertyField(attackRange);
        EditorGUILayout.PropertyField(projectilePrefab);


        // ---------------- AUTOMATIC ----------------
        if (wt != WeaponType.Shotgun &&
            wt != WeaponType.Sniper &&
            wt != WeaponType.Molotov)
        {
            EditorGUILayout.PropertyField(isAutomatic);
        }

        // ---------------- SHOTGUN ----------------
        if (wt == WeaponType.Shotgun)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Shotgun Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(pelletsPerShot);
            EditorGUILayout.PropertyField(pelletSpreadAngle);
            EditorGUILayout.PropertyField(shotgunCooldown);
        }

        // ---------------- SNIPER ----------------
        if (wt == WeaponType.Sniper)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Sniper Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(sniperCooldown);
            EditorGUILayout.PropertyField(sniperPenetrationCount);
        }

        // ---------------- MOLOTOV ----------------
        if (wt == WeaponType.Molotov)
        {
            isMolotov.boolValue = true;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Molotov Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(fireEffectPrefab);
            EditorGUILayout.PropertyField(explosionRadius);
            EditorGUILayout.PropertyField(burnDamage);
            EditorGUILayout.PropertyField(burnDuration);
            EditorGUILayout.PropertyField(tickInterval);
        }
        else
        {
            isMolotov.boolValue = false;
        }

        // ---------------- AMMO & MAGAZINE ----------------
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Ammo & Magazine", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(ammoType);

        if (acceptedMagazines != null)
        {
            EditorGUILayout.PropertyField(
                acceptedMagazines,
                new GUIContent("Accepted Magazines"),
                true
            );
        }

        serializedObject.ApplyModifiedProperties();
    }
}
