using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite weaponIcon;

    public WeaponType weaponType;
    [Header("Knockback Settings")]
    public float knockbackForce = 6f;       // Kuvvet (her zaman float)
    public float knockbackDuration = 0.18f; // Süre (float olmalı)
    // Ortak
    public bool isAutomatic;        // Melee için basılı tutma, ranged için otomatik ateş
    public float fireRate = 2f;     // saniyedeki atış/vuruş (shotgun/sniper'da YOK SAYILIR)
    public int damage;

    [Header("Ranged Weapon")]
    public int clipSize;            // Melee için 0
    public int maxAmmoCapacity;
    public float reloadTime = 1.5f;

    [Header("Melee Weapon")]
    [Tooltip("Vuruşun ne kadar uzağa etki edeceği.")]
    public float attackRange = 1.5f;

    // 🔽 Shotgun'a özel alanlar
    [Header("Shotgun Settings")]
    public bool isShotgun = false;
    [Range(1, 12)] public int pelletsPerShot = 3;
    [Range(0f, 45f)] public float pelletSpreadAngle = 8f;
    [Min(0.1f)] public float shotgunCooldown = 2.5f;

    // 🔽 Sniper'a özel alanlar
    [Header("Sniper Settings")]
    public bool isSniper = false;
    [Min(0.1f)] public float sniperCooldown = 3.0f;   // Sniper için özel bekleme süresi
    [Range(1, 5)] public int sniperPenetrationCount = 2;

    [Header("Ammo Type")]
    public ResourceType ammoType; // 👈 Hangi mermi kullanıyor?
    
    // 🔥 YENİ: Molotov özel alanları
    [Header("Molotov Settings")]
    public bool isMolotov; // sadece Molotov’lar için aktif olacak
    public GameObject fireEffectPrefab;
    public float explosionRadius = 2.5f;
    public int burnDamage = 1;
    public float burnDuration = 5f;
    public float tickInterval = 1f;
}
