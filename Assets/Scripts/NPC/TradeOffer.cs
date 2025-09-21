using UnityEngine;

public enum RewardKind
{
    WeaponPart,
    Resource
}

[CreateAssetMenu(fileName = "New Trade Offer", menuName = "NPC/Trade Offer")]
public class TradeOffer : ScriptableObject
{
    [Header("Player Gives (Costs)")]
    public int requiredStone = 0;
    public int requiredWood = 0;
    public int requiredScrapMetal = 0;

    // Et ve deri gibi ek maliyetler:
    public int requiredAmmo = 0;
    public int requiredMeat = 0;
    public int requiredDeerHide = 0;
    public int requiredRabbitHide = 0;
    public int requiredHerb = 0; // ⬅️ YENİ

    [Header("Player Gets (Rewards)")]
    public RewardKind rewardKind = RewardKind.WeaponPart;

    // WeaponPart ödülü için:
    public WeaponPartType partToGive;
    public int amountToGive = 1;

    // Resource ödülü için:
    public ResourceType resourceToGive;
    public int resourceAmountToGive = 0;
}
