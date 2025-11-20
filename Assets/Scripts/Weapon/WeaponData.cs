using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Weapon Data")]
public class WeaponData : ItemData
{
    public GameObject prefab;
    public WeaponType weaponType;

    public float knockbackForce = 6f;
    public float knockbackDuration = 0.18f;

    public bool isAutomatic;
    public float fireRate = 2f;
    public int damage;

    public int clipSize;
    public int maxAmmoCapacity;
    public float reloadTime = 1.5f;

    public float attackRange = 1.5f;

    public bool isShotgun = false;
    public int pelletsPerShot = 3;
    public float pelletSpreadAngle = 8f;
    public float shotgunCooldown = 2.5f;

    public bool isSniper = false;
    public float sniperCooldown = 3.0f;
    public int sniperPenetrationCount = 2;

    public ResourceType ammoType;

    public bool isMolotov;
    public GameObject fireEffectPrefab;
    public float explosionRadius = 2.5f;
    public int burnDamage = 1;
    public float burnDuration = 5f;
    public float tickInterval = 1f;
}
