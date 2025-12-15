using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ammo Item")]
public class AmmoItemData : ItemData
{
    [Header("Ammo Settings")]
    public AmmoTypeData ammoType;
    public int ammoAmount;
}
