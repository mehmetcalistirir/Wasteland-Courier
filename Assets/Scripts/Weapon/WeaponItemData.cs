using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon Item")]
public class WeaponItemData : ItemData
{
    [Header("Weapon Link")]
    public WeaponType weaponType;   // Pistol / Rifle / Melee
    public WeaponData weaponData;   // Ateş etme davranışları

    private void OnValidate()
    {
        category = ItemCategory.Weapon;
        stackable = false;
        maxStack = 1;
    }
}
