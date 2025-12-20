using UnityEngine;

[CreateAssetMenu(
    menuName = "Items/Weapon/Ammo",
    fileName = "itm_ammo_"
)]
public class AmmoItemData : ItemData
{
    [Header("Ammo Settings")]
    public AmmoTypeData ammoType;

    [Tooltip("Bu item alındığında ammoPool'a eklenecek miktar")]
    public int ammoAmount = 10;

    private void OnValidate()
    {
        category = ItemCategory.Ammo;
        stackable = false;
        maxStack = 1;
    }
}
