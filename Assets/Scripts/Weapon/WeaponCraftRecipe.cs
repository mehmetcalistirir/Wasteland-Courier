using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Craft Recipe", menuName = "Crafting/Weapon Craft Recipe")]
public class WeaponCraftRecipe : ScriptableObject
{
    [Header("Üretilecek Silah (WeaponData)")]
    public WeaponData resultWeapon;

    [Header("Gerekli Kaynaklar")]
    public List<ResourceCost> costs = new List<ResourceCost>();
}

[Serializable]
public class ResourceCost
{
    public ItemData item;
    public int amount = 1;
}
