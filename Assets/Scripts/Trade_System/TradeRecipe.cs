using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    menuName = "Trade/Trade Recipe",
    fileName = "trade_"
)]
public class TradeRecipe : ScriptableObject
{
    [Header("NPC Gives (Reward)")]
    public ItemData resultItem;
    public int resultAmount = 1;

    [Header("Player Gives (Cost)")]
    public List<ItemCost> costs = new();

    [Header("Description")]
    [TextArea]
    public string description;
}
