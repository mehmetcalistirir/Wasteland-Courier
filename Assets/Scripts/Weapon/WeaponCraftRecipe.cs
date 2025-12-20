using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(
    menuName = "Crafting/Recipe/Weapon",
    fileName = "recipe_wpn_"
)]
public class WeaponCraftRecipe : ScriptableObject
{
    [Header("Result")]
    public WeaponItemData resultWeapon;

    [Header("Description")]
    [TextArea]
    public string description;

    [Header("Costs")]
    public List<ItemCost> costs = new();
}

[Serializable]
public class ItemCost
{
    public ItemData item;
    public int amount = 1;
}
