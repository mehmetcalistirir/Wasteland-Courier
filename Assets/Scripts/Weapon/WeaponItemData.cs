using UnityEngine;

[CreateAssetMenu(
    menuName = "Items/Weapon",
    fileName = "itm_wpn_"
)]
public class WeaponItemData : ItemData
{
    [Header("Weapon Link")]
    public WeaponType weaponType;
    public WeaponDefinition weaponDefinition;

    private void OnValidate()
    {
        category = ItemCategory.Weapon;
        stackable = false;
        maxStack = 1;
    }
}
