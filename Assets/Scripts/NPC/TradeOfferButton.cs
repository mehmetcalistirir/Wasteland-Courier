using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeOfferButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI offerText; // Örn: "30 Wood -> 1 Barrel"
    public Button tradeButton;

    private TradeOffer currentOffer;
    private NPCInteraction npc;

    // NPCInteraction bu fonksiyonu çağırarak butonu kurar.
    public void Setup(TradeOffer offer, PlayerStats stats)
    {
        currentOffer = offer;
        npc = FindObjectOfType<NPCInteraction>(); // NPC'ye referans al

        // Butonun metnini oluştur
        string costText = $"İstenen: {offer.requiredWood} Odun, {offer.requiredStone} Taş, {offer.requiredScrapMetal} Metal";
        string rewardText = $"Verilen: {offer.amountToGive} x {offer.partToGive}";
        offerText.text = $"{costText}\n{rewardText}";

        // Oyuncunun yeterli materyali var mı?
        bool canAfford = (stats.GetResourceAmount("Stone") >= offer.requiredStone &&
                          stats.GetResourceAmount("Wood") >= offer.requiredWood &&
                          stats.GetResourceAmount("scrapMetal") >= offer.requiredScrapMetal);

        // Eğer yeterli materyal yoksa, butonu tıklanamaz yap.
        tradeButton.interactable = canAfford;

        // Butona tıklandığında NPC'deki ExecuteTrade fonksiyonunu çağır.
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(() => npc.ExecuteTrade(currentOffer));
    }
}