using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TradeOfferButton : MonoBehaviour
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject offerTextPrefab;
    [SerializeField] private Button tradeButton;
    [SerializeField] private Image iconImage;

    [Header("Layout")]
    [SerializeField] private float gap = 10f;
    [SerializeField] private float textPosX = 490f;
    [SerializeField] private float textWidth = 1000f;
    [SerializeField] private float textHeight = 100f;

    [Header("Sprites (Optional)")]
    public Sprite fallbackSprite;

    // Yeni sistem — PartItemData → Sprite
    private Dictionary<PartItemData, Sprite> partMap = new();

    private TradeOffer currentOffer;
    private NPCInteraction npc;
    private PlayerStats stats;

    [SerializeField] private TextMeshProUGUI offerTextSlot;
    private TextMeshProUGUI offerTextInstance;

    void Awake()
    {
        if (!iconImage && tradeButton)
            iconImage = tradeButton.GetComponent<Image>();

        EnsureOfferText();
    }

    private void EnsureOfferText()
    {
        if (offerTextInstance != null) return;

        if (offerTextSlot != null)
            offerTextInstance = offerTextSlot;
        else if (offerTextPrefab != null)
            offerTextInstance = Instantiate(offerTextPrefab, transform).GetComponent<TextMeshProUGUI>();
        else
            Debug.LogError("[TradeOfferButton] Text kaynağı yok!");

        PositionAndSizeText();
    }

    public void Setup(TradeOffer offer, PlayerStats statsRef)
    {
        currentOffer = offer;
        stats = statsRef;
        npc = NPCInteraction.Instance;

        if (!tradeButton || currentOffer == null)
            return;

        EnsureOfferText();
        ApplyIconForOffer(offer);

        // ------ Metin ------
        if (offerTextInstance)
        {
            string cost = BuildCostText(offer);

            string reward = offer.rewardKind == RewardKind.WeaponPart
                ? $"Verilen: {offer.amountToGive} x {offer.partToGive?.itemName}"
                : $"Verilen: {offer.rewardAmount} x {offer.rewardItemSO?.itemName}";

            offerTextInstance.text = $"{cost}\n{reward}";
        }

        tradeButton.interactable = CanAfford(offer);
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(OnTradeClicked);
    }

    private bool CanAfford(TradeOffer offer)
    {
        return
            stats.GetResourceAmount(offer.stoneSO) >= offer.requiredStone &&
            stats.GetResourceAmount(offer.woodSO) >= offer.requiredWood &&
            stats.GetResourceAmount(offer.scrapSO) >= offer.requiredScrapMetal &&
            stats.GetResourceAmount(offer.meatSO) >= offer.requiredMeat &&
            stats.GetResourceAmount(offer.deerHideSO) >= offer.requiredDeerHide &&
            stats.GetResourceAmount(offer.rabbitHideSO) >= offer.requiredRabbitHide &&
            stats.GetResourceAmount(offer.herbSO) >= offer.requiredHerb &&
            stats.GetResourceAmount(offer.ammoSO) >= offer.requiredAmmo;
    }

    private string BuildCostText(TradeOffer o)
    {
        List<string> parts = new();

        if (o.requiredWood > 0) parts.Add($"{o.requiredWood} Odun");
        if (o.requiredStone > 0) parts.Add($"{o.requiredStone} Taş");
        if (o.requiredScrapMetal > 0) parts.Add($"{o.requiredScrapMetal} Metal");
        if (o.requiredMeat > 0) parts.Add($"{o.requiredMeat} Et");
        if (o.requiredDeerHide > 0) parts.Add($"{o.requiredDeerHide} Geyik Derisi");
        if (o.requiredRabbitHide > 0) parts.Add($"{o.requiredRabbitHide} Tavşan Derisi");
        if (o.requiredHerb > 0) parts.Add($"{o.requiredHerb} Ot");
        if (o.requiredAmmo > 0) parts.Add($"{o.requiredAmmo} Mermi");

        return "İstenen: " + string.Join(", ", parts);
    }

    private void PositionAndSizeText()
    {
        var rt = offerTextInstance.rectTransform;

        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        rt.anchoredPosition = new Vector2(textPosX, 0f);

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        var le = offerTextInstance.GetComponent<LayoutElement>();
        if (!le) le = offerTextInstance.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    private void ApplyIconForOffer(TradeOffer offer)
    {
        if (!iconImage) return;

        Sprite s = null;

        if (offer.rewardKind == RewardKind.WeaponPart)
        {
            if (offer.partToGive != null)
                s = offer.partToGive.icon; // ItemData icon
        }
        else if (offer.rewardKind == RewardKind.Resource)
        {
            if (offer.rewardItemSO != null)
                s = offer.rewardItemSO.icon;
        }

        iconImage.sprite = s ? s : fallbackSprite;
        iconImage.enabled = iconImage.sprite != null;
        if (iconImage.enabled) iconImage.preserveAspect = true;
    }

    private void OnTradeClicked()
    {
        if (npc == null || currentOffer == null) return;

        npc.ExecuteTrade(currentOffer);
    }
}
