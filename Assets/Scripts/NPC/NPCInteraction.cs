using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;                     // ✅ eklendi
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class NPCInteraction : MonoBehaviour
{


    private readonly List<TradeOfferButton> spawned = new List<TradeOfferButton>();
    private bool layoutDone = false;

    [Header("Trade Data")]
    public List<TradeOffer> tradeOffers;

    [Header("Layout Settings (code-driven)")]
    public float rowSpacing = 8f;   // satırlar arası
    public float innerSpacing = 12f; // buton ile text arası (prefab içi HLG.spacing)

    [Header("Layout (manual)")]
    public float topPadding = 10f;
    public float bottomPadding = 10f;



    [Header("UI Refs")]
    public GameObject tradeUIPanel;
    public GameObject interactPromptUI;
    public Transform tradeOffersContainer;          // Content
    public GameObject tradeOfferButtonPrefab;       // içinde TradeOfferButton var
    public ScrollRect tradeScrollRect;              // ✅ Scroll View’in üzerindeki ScrollRect

    private bool inertiaDefault = true;

    [Header("Auto Load (optional)")]
    public bool autoLoadFromResources = true;
    public string resourcesFolder = "Animals";

    private bool isPlayerNearby = false;
    private PlayerStats playerStats;

    public static NPCInteraction Instance { get; private set; }
    public static bool IsTradeOpen =>
        Instance != null && Instance.tradeUIPanel != null && Instance.tradeUIPanel.activeSelf;

    private void Awake() { Instance = this; }

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (tradeUIPanel != null) tradeUIPanel.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

        // (opsiyonel) Resources’tan teklifleri yükle
        if (autoLoadFromResources)
        {
            var loaded = Resources.LoadAll<TradeOffer>(resourcesFolder);
            if (loaded != null && loaded.Length > 0)
            {
                var set = new HashSet<TradeOffer>(tradeOffers);
                foreach (var t in loaded) if (!set.Contains(t)) tradeOffers.Add(t);
            }
        }

        // ✅ Content doğru yapılandırılmış mı emin ol (runtime’da güvenlik)

    }

    private System.Collections.IEnumerator OpenTradePanelStable()
    {
        // 1) UI’ı kur
        PopulateTradeOffers();

        // 2) 1 frame bekle -> layout otursun
        yield return null;
        Canvas.ForceUpdateCanvases();

        // 3) Content’i tepeye çek
        var contentRT = (RectTransform)tradeOffersContainer;
        // Yalnızca Y’yi 0’la (pivot 0,1 olduğunda tepe demek)
        contentRT.anchoredPosition = new Vector2(contentRT.anchoredPosition.x, 0f);

        // 4) Scroll’u dondur ve tepeye al
        if (tradeScrollRect != null)
        {
            tradeScrollRect.StopMovement();
            tradeScrollRect.velocity = Vector2.zero;
            tradeScrollRect.verticalNormalizedPosition = 1f;
            tradeScrollRect.inertia = false; // ister kalıcı kapat
        }

        // 5) Otomatik seçili UI temizle
        EventSystem.current?.SetSelectedGameObject(null);
    }

    private void Update()
    {
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            ToggleTradePanel();

        if (IsTradeOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseTradePanel();
    }

    public void CloseTradePanel()
    {
        if (tradeUIPanel == null) return;
        tradeUIPanel.SetActive(false);
        Time.timeScale = 1f;
        if (isPlayerNearby && interactPromptUI != null) interactPromptUI.SetActive(true);
    }

    private System.Collections.IEnumerator AfterFirstFrameStabilize()
    {
        yield return null; // 1 frame bekle (layout bitsin)
        Canvas.ForceUpdateCanvases();

        if (tradeScrollRect != null)
        {
            tradeScrollRect.StopMovement();
            tradeScrollRect.velocity = Vector2.zero;                 // güvence
            tradeScrollRect.verticalNormalizedPosition = 1f;         // tekrar tepe
            tradeScrollRect.inertia = inertiaDefault;                // eski ayara dön
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);         // oto-seçimi temizle
    }


    private void ToggleTradePanel()
    {
        if (tradeUIPanel == null) return;

        bool shouldOpen = !tradeUIPanel.activeSelf;
        tradeUIPanel.SetActive(shouldOpen);

        if (shouldOpen)
        {
            StartCoroutine(OpenTradePanelStable());
            Time.timeScale = 0f;
            interactPromptUI?.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            if (isPlayerNearby) interactPromptUI?.SetActive(true);
        }
    }

    private void PopulateTradeOffers()
    {
        // Scroll pozisyonunu koru
        float prevScroll = tradeScrollRect ? tradeScrollRect.verticalNormalizedPosition : 1f;

        // İLK KEZ: oluştur + konumlandır
        if (!layoutDone)
        {
            // Var olan çocukları temizle (sadece 1 kere)
            for (int i = tradeOffersContainer.childCount - 1; i >= 0; i--)
                Destroy(tradeOffersContainer.GetChild(i).gameObject);

            float y = -topPadding;
            for (int i = 0; i < tradeOffers.Count; i++)
            {
                var go = Instantiate(tradeOfferButtonPrefab, tradeOffersContainer);
                var btn = go.GetComponent<TradeOfferButton>();

                // Anchor/pivot sabit
                var rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);

                // YERLEŞİM YALNIZCA BURADA!
                float h = Mathf.Max(rt.sizeDelta.y, LayoutUtility.GetPreferredHeight(rt));
                rt.anchoredPosition = new Vector2(0f, y);
                y -= (h + rowSpacing);

                spawned.Add(btn);
            }

            // Content yüksekliği
            var contentRT = (RectTransform)tradeOffersContainer;
            float needed = -y - rowSpacing + bottomPadding;
            if (needed < 0f) needed = 0f;
            var sz = contentRT.sizeDelta;
            contentRT.sizeDelta = new Vector2(sz.x, needed);

            layoutDone = true;
        }

        // HER SEFERİNDE: sadece veri / interaktiflik güncelle
        for (int i = 0; i < spawned.Count && i < tradeOffers.Count; i++)
            spawned[i].Setup(tradeOffers[i]);


        // UI sabitle
        Canvas.ForceUpdateCanvases();
        if (tradeScrollRect)
        {
            tradeScrollRect.StopMovement();
            tradeScrollRect.velocity = Vector2.zero;
            tradeScrollRect.verticalNormalizedPosition = prevScroll; // kayma yok
        }
        EventSystem.current?.SetSelectedGameObject(null);
    }

    // --- Kaynak kontrol & düşüm ---
    private bool HasEnoughResources(TradeOffer offer)
{
    if (playerStats == null || offer == null) return false;

    // 0 isteyenleri atla
    if (offer.requiredStone > 0 && !Inventory.Instance.HasEnough(offer.stoneSO, offer.requiredStone)) return false;
    if (offer.requiredWood > 0 && !Inventory.Instance.HasEnough(offer.woodSO, offer.requiredWood)) return false;
    if (offer.requiredScrapMetal > 0 && !Inventory.Instance.HasEnough(offer.scrapSO, offer.requiredScrapMetal)) return false;
    if (offer.requiredMeat > 0 && !Inventory.Instance.HasEnough(offer.meatSO, offer.requiredMeat)) return false;
    if (offer.requiredDeerHide > 0 && !Inventory.Instance.HasEnough(offer.deerHideSO, offer.requiredDeerHide)) return false;
    if (offer.requiredRabbitHide > 0 && !Inventory.Instance.HasEnough(offer.rabbitHideSO, offer.requiredRabbitHide)) return false;
    if (offer.requiredHerb > 0 && !Inventory.Instance.HasEnough(offer.herbSO, offer.requiredHerb)) return false;
    if (offer.requiredAmmo > 0 && !Inventory.Instance.HasEnough(offer.ammoSO, offer.requiredAmmo)) return false;

    return true;
}


    private void DeductResources(TradeOffer offer)
{
    if (offer == null) return;

    if (offer.requiredStone > 0) Inventory.Instance.TryConsume(offer.stoneSO, offer.requiredStone);
    if (offer.requiredWood > 0) Inventory.Instance.TryConsume(offer.woodSO, offer.requiredWood);
    if (offer.requiredScrapMetal > 0) Inventory.Instance.TryConsume(offer.scrapSO, offer.requiredScrapMetal);
    if (offer.requiredMeat > 0) Inventory.Instance.TryConsume(offer.meatSO, offer.requiredMeat);
    if (offer.requiredDeerHide > 0) Inventory.Instance.TryConsume(offer.deerHideSO, offer.requiredDeerHide);
    if (offer.requiredRabbitHide > 0) Inventory.Instance.TryConsume(offer.rabbitHideSO, offer.requiredRabbitHide);
    if (offer.requiredHerb > 0) Inventory.Instance.TryConsume(offer.herbSO, offer.requiredHerb);
    if (offer.requiredAmmo > 0) Inventory.Instance.TryConsume(offer.ammoSO, offer.requiredAmmo);
}

    // --- Takas işlemi ---
public void ExecuteTrade(TradeOffer offer)
{
    if (offer == null) return;

    // Yeterli kaynak var mı?
    if (!HasEnoughResources(offer))
    {
        Debug.Log("Takas başarısız! Yeterli materyal yok.");
        return;
    }

    // Kaynakları düş
    DeductResources(offer);

    // Ödül tipi: normal resource
    if (offer.rewardKind == RewardKind.Resource)
    {
        if (offer.rewardItemSO != null && offer.rewardAmount > 0)
        {
            Inventory.Instance.TryAdd(offer.rewardItemSO, offer.rewardAmount);
            Debug.Log($"Takas başarılı! {offer.rewardAmount} x {offer.rewardItemSO.itemName} alındı.");
        }
    }
    // Ödül tipi: silah parçası
    else if (offer.rewardKind == RewardKind.WeaponPart)
    {
        if (offer.partToGive != null && offer.amountToGive > 0)
        {
            Inventory.Instance.TryAdd(offer.partToGive, offer.amountToGive);
            Debug.Log($"Takas başarılı! {offer.amountToGive} x {offer.partToGive.itemName} alındı.");
        }
    }

    // UI yenile
    PopulateTradeOffers();
}



    // --- Trigger alanı ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = true;

        // Panel kapalıysa prompt’u göster
        if (!IsTradeOpen && interactPromptUI != null)
            interactPromptUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = false;

        // Alandan çıkınca prompt’u gizle
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // İsteğe bağlı: Alandan çıkınca paneli kapat
        // CloseTradePanel();
    }

    [ContextMenu("Reload Trade Offers")]
    private void ReloadTradeOffersInEditor()
    {
        var loaded = Resources.LoadAll<TradeOffer>(resourcesFolder);
        tradeOffers = loaded.ToList();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
