using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TradeOfferButton : MonoBehaviour
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject offerTextPrefab;      // (yoksa buradan oluşturur)
    [SerializeField] private Button tradeButton;
    [SerializeField] private Image iconImage;

    [Header("Layout")]

    [SerializeField] private float gap = 10f;
    [SerializeField] private float textPosX = 490f;          // Sağda sabit X
    [SerializeField] private float textWidth = 1000f;
    [SerializeField] private float textHeight = 100f; // ihtiyacına göre

    // --- Sprite haritaları (aynı) ---
    [Header("Sprites: Weapon Parts")]
    [SerializeField] private List<PartSpriteEntry> partSprites = new();
    [Header("Sprites: Resources")]
    [SerializeField] private List<ResourceSpriteEntry> resourceSprites = new();
    [SerializeField] private Sprite fallbackSprite;

    // Header'larına şunu ekleyebilirsin (istersen ayrı alan aç)
    [Header("List Row Layout")]
    [SerializeField] private float rowMinHeight = 120f; // her butonun satır yüksekliği


    private Dictionary<WeaponPartType, Sprite> partMap;
    private Dictionary<ResourceType, Sprite> resourceMap;

    private TradeOffer currentOffer;
    private NPCInteraction npc;
    private PlayerStats stats;

    // 🔹 Prefab içindeki Text'i buraya drag&drop yap
    [SerializeField] private TextMeshProUGUI offerTextSlot;
    [Header("Sprites: Resources")]


    private TextMeshProUGUI offerTextInstance;


    [System.Serializable] public struct PartSpriteEntry { public WeaponPartType part; public Sprite sprite; }
    [System.Serializable] public struct ResourceSpriteEntry { public ResourceType resource; public Sprite sprite; }

    void Awake()
    {
        // Map kurulumları
        partMap = new Dictionary<WeaponPartType, Sprite>();
        foreach (var e in partSprites) if (e.sprite && !partMap.ContainsKey(e.part)) partMap.Add(e.part, e.sprite);

        resourceMap = new Dictionary<ResourceType, Sprite>();
        foreach (var e in resourceSprites) if (e.sprite && !resourceMap.ContainsKey(e.resource)) resourceMap.Add(e.resource, e.sprite);

        if (!iconImage && tradeButton) iconImage = tradeButton.GetComponent<Image>();

        EnsureOfferText(); // ✅ ÇOCUK Text’i hazırla/konumlandır

        ApplyRowLayout();  // ⬅️ ekle
    }

    private void ApplyRowLayout()
    {
        // 1) Üst parent'taki VerticalLayoutGroup'un spacing'ini 'gap' ile ayarla
        var vlg = GetComponentInParent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.spacing = gap;                 // ⬅️ satırlar arası boşluk
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = true;     // preferredHeight'i dikkate alsın
        }

        // 2) Bu buton satırının yüksekliğini belirle (LayoutElement ile)
        var le = GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();

        // Satır yüksekliği: textHeight + üst/alt pay + istersen gap
        float h = Mathf.Max(rowMinHeight, textHeight + gap * 2f);
        le.minHeight = h;
        le.preferredHeight = h;
    }


    private void EnsureOfferText()
    {
        if (offerTextInstance) return;

        if (offerTextSlot != null)
            offerTextInstance = offerTextSlot;
        else if (offerTextPrefab != null)
            offerTextInstance = Instantiate(offerTextPrefab, transform, false)
                                .GetComponent<TextMeshProUGUI>();
        else
        {
            Debug.LogError("[TradeOfferButton] Text kaynağı yok!");
            return;
        }

        PositionAndSizeText(); // sadece bunu çağır
    }


    public void Setup(TradeOffer offer, PlayerStats statsRef)
    {
        currentOffer = offer;
        stats = statsRef;
        npc = FindObjectOfType<NPCInteraction>();
        if (!tradeButton || currentOffer == null) return;

        EnsureOfferText(); // güvence

        // İkon
        ApplyIconForOffer(currentOffer);

        // Metin
        if (offerTextInstance)
        {
            string costText =
                $"İstenen: {offer.requiredWood} Odun, {offer.requiredStone} Taş, {offer.requiredScrapMetal} Metal";

            if (offer.requiredMeat > 0) costText += $", {offer.requiredMeat} Et";
            if (offer.requiredDeerHide > 0) costText += $", {offer.requiredDeerHide} Geyik Derisi";
            if (offer.requiredRabbitHide > 0) costText += $", {offer.requiredRabbitHide} Tavşan Derisi";
            if (offer.requiredHerb > 0) costText += $", {offer.requiredHerb} Şifalı Ot";
            if (offer.requiredAmmo > 0) costText += $", {offer.requiredAmmo} Mermi";

            string rewardText = currentOffer.rewardKind == RewardKind.WeaponPart
    ? $"Verilen: {offer.amountToGive} x {offer.partToGive}"
    : $"Verilen: {offer.rewardAmount} x {offer.rewardItemSO?.itemName}";


            offerTextInstance.text = $"{costText}\n{rewardText}";
        }

        // Buton aktifliği
       bool canAfford = stats &&
    stats.GetResourceAmount(currentOffer.stoneSO) >= offer.requiredStone &&
    stats.GetResourceAmount(currentOffer.woodSO) >= offer.requiredWood &&
    stats.GetResourceAmount(currentOffer.scrapSO) >= offer.requiredScrapMetal &&
    stats.GetResourceAmount(currentOffer.meatSO) >= offer.requiredMeat &&
    stats.GetResourceAmount(currentOffer.deerHideSO) >= offer.requiredDeerHide &&
    stats.GetResourceAmount(currentOffer.rabbitHideSO) >= offer.requiredRabbitHide &&
    stats.GetResourceAmount(currentOffer.herbSO) >= offer.requiredHerb &&
    stats.GetResourceAmount(currentOffer.ammoSO) >= offer.requiredAmmo;

        tradeButton.interactable = canAfford;
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(OnTradeClicked);
    }

    private void PositionAndSizeText()
    {
        var rt = offerTextInstance.rectTransform;

        // Anchor/pivot
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        // Konum
        rt.anchoredPosition = new Vector2(textPosX, 0f);

        // 🔧 Boyutu açıkça belirle (TMP'nin 200x50 varsayılanını ez)
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        // Eğer parent zincirinde Layout Group varsa, bu text'i ondan etkilenmeyecek hale getir
        var le = offerTextInstance.GetComponent<LayoutElement>();
        if (le == null) le = offerTextInstance.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true; // Layout Group boyut/konumu değiştirmesin

        // Okunabilirlik için:
        offerTextInstance.enableWordWrapping = true;
        offerTextInstance.alignment = TextAlignmentOptions.TopLeft;

        offerTextInstance.lineSpacing = 1f;
    }

    private void ApplyIconForOffer(TradeOffer offer)
{
    if (!iconImage) return;
    Sprite s = null;

    if (offer.rewardKind == RewardKind.WeaponPart)
    {
        partMap?.TryGetValue(offer.partToGive, out s);
    }
    else if (offer.rewardKind == RewardKind.Resource)
    {
        if (offer.rewardItemSO != null)
            s = offer.rewardItemSO.icon;   // <-- direk ItemData’dan al
    }

    iconImage.sprite = s ? s : fallbackSprite;
    iconImage.enabled = (iconImage.sprite != null);
    if (iconImage.enabled) iconImage.preserveAspect = true;
}



    private void OnTradeClicked()
    {
        var inst = NPCInteraction.Instance;
        if (inst && inst.tradeScrollRect)
        {
            var sr = inst.tradeScrollRect;
            sr.StopMovement(); sr.velocity = Vector2.zero; sr.verticalNormalizedPosition = 1f;
        }

        if (!npc || currentOffer == null) return;
        npc.ExecuteTrade(currentOffer);
    }
}
