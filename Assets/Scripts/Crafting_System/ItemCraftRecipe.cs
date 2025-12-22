using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    menuName = "Crafting/Recipe/Item",
    fileName = "recipe_item_"
)]
public class ItemCraftRecipe : ScriptableObject
{
    [Header("Result")]
    public ItemData resultItem;
    public int resultAmount = 1;

    [Header("Costs")]
    public List<ItemCost> costs = new();

    [Header("Description")]
    [TextArea]
    public string description;

    // 🔓 Opsiyonel unlock
    public bool unlockOnce = false;
    public string unlockID;
}
