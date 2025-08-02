using UnityEngine;

[CreateAssetMenu(fileName = "New Trade Offer", menuName = "NPC/Trade Offer")]
public class TradeOffer : ScriptableObject
{
    [Header("Player Gives (Costs)")]
    public int requiredStone = 0;
    public int requiredWood = 0;
    public int requiredScrapMetal = 0;

    [Header("Player Gets (Rewards)")]
    public WeaponPartType partToGive;
    public int amountToGive = 1;
}