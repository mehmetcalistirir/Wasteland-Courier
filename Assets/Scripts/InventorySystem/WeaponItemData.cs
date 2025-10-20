using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon Item")]
public class WeaponItemData : ItemData
{
    [Header("Weapon Link")]
    public WeaponBlueprint blueprint;
    public WeaponType weaponType;

    private void OnValidate()
    {
        category = ItemCategory.Weapon;
        stackable = false;
        maxStack = 1;
    }
}
