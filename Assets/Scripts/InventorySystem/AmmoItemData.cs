using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ammo")]
public class AmmoItemData : ItemData
{
    [Header("Ammo Settings")]
    public int ammoPerItem = 15;
}
