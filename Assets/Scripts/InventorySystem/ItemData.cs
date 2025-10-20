using UnityEngine;

public enum ItemCategory { Resource, Ammo, Weapon, Misc }

public abstract class ItemData : ScriptableObject
{

    public ResourceType resourceType;

    [Header("Base")]
    public string itemName;
    public Sprite icon;
    public ItemCategory category;
    public bool stackable = true;
    [Min(1)] public int maxStack = 99;

    public bool showCountBadge => stackable;
}
