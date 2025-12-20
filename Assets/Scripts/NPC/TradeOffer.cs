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
    public ItemData stoneSO;
    public int requiredStone = 0;

    public ItemData woodSO;
    public int requiredWood = 0;

    public ItemData scrapSO;
    public int requiredScrapMetal = 0;

    public ItemData ammoSO;
    public int requiredAmmo = 0;

    public ItemData meatSO;
    public int requiredMeat = 0;

    public ItemData deerHideSO;
    public int requiredDeerHide = 0;

    public ItemData rabbitHideSO;
    public int requiredRabbitHide = 0;

    public ItemData herbSO;
    public int requiredHerb = 0;

    [Header("Player Gets (Rewards)")]
    public RewardKind rewardKind = RewardKind.Resource;

    // ✔ WeaponPart ödülü için (Yeni sistem)
    public WeaponPartItemData partToGive;
    public int amountToGive = 1;

    // Resource ödülü için
    public ItemData rewardItemSO;
    public int rewardAmount = 0;
}
