using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    menuName = "Items/Weapon/Weapon Definition",
    fileName = "wpn_"
)]
public class WeaponDefinition : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;
    public WeaponType weaponType;

    [Header("Combat")]
    public int damage;
    public bool isAutomatic;
    public float fireRate = 2f;
    public float reloadTime = 1.5f;
    public float attackRange = 1.5f;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.18f;

    [Header("Shotgun")]
    public bool isShotgun;
    public int pelletsPerShot = 3;
    public float pelletSpreadAngle = 8f;
    public float shotgunCooldown = 2.5f;

    [Header("Sniper")]
    public bool isSniper;
    public float sniperCooldown = 3.0f;
    public int sniperPenetrationCount = 2;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Ammo & Magazine")]
    public AmmoTypeData ammoType;
    public List<MagazineType> acceptedMagazines = new();

    [Header("Throwable / Fire")]
    public bool isMolotov;
    public GameObject fireEffectPrefab;
    public float explosionRadius = 2.5f;
    public int burnDamage = 1;
    public float burnDuration = 5f;
    public float tickInterval = 1f;
}
