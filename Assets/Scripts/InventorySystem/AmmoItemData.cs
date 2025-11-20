using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ammo Data")]
public class AmmoItemData : ItemData
{
    [Header("Ammo Settings")]
    public int ammoPerItem = 15;
    public int ammoPerPickup = 10;
}
