// WeaponData.cs (YAKIN DÖVÜŞ İÇİN GÜNCELLENMİŞ HALİ)

using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite weaponIcon;
    public bool isAutomatic; // Melee için de kullanılabilir (basılı tutunca sürekli sallar)
    public float fireRate = 2f;    // Saldırı hızı (saniyedeki vuruş sayısı)
    public int damage;

    [Header("Ranged Weapon")]
    public int clipSize; // Yakın dövüş silahları için bu değer 0 olacak.
    public int maxAmmoCapacity;
    public float reloadTime = 1.5f;

    [Header("Melee Weapon")]
    [Tooltip("Vuruşun ne kadar uzağa etki edeceği.")]
    public float attackRange = 1.5f; // Saldırı menzili
}