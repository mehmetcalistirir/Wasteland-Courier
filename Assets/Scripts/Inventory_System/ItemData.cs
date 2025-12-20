using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Base")]
    public string itemName;

    [Tooltip("Unique, stable id used by ItemDatabase & Save/Load")]
    public string itemID;

    public Sprite icon;

    [HideInInspector]
    public ItemCategory category;

    [Header("Stack")]
    public bool stackable = true;
    [Min(1)] public int maxStack = 99;

    public bool showCountBadge => stackable;
}
