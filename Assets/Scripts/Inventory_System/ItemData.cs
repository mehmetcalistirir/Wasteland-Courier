using UnityEngine;


public enum ItemCategory { Resource, Ammo, Weapon, Misc }
[CreateAssetMenu(menuName = "Items/Item Data")]
public abstract class ItemData : ScriptableObject
{

    [Header("Base")]
    public string itemName;
    public string itemID;
    public Sprite icon;
    public ItemCategory category;
    public bool stackable = true;
    [Min(1)] public int maxStack = 99;

    public bool showCountBadge => stackable;
}
