using UnityEngine;
using UnityEngine.UI;
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
    public float rowSpacing = 8f;
    public float innerSpacing = 12f;

    [Header("Layout (manual)")]
    public float topPadding = 10f;
    public float bottomPadding = 10f;

    [Header("UI Refs")]
    public GameObject tradeUIPanel;
    public GameObject interactPromptUI;
    public Transform tradeOffersContainer;
    public GameObject tradeOfferButtonPrefab;
    public ScrollRect tradeScrollRect;

    private bool inertiaDefault = true;
    private bool isPlayerNearby = false;
    private PlayerStats playerStats;

    public static NPCInteraction Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();

        if (tradeUIPanel != null) tradeUIPanel.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

        // (optional) load offers
        var loaded = Resources.LoadAll<TradeOffer>("Animals");
        if (loaded.Length > 0)
        {
            var set = new HashSet<TradeOffer>(tradeOffers);
            foreach (var t in loaded) if (!set.Contains(t)) tradeOffers.Add(t);
        }
    }

    // --------------------------------------------------------------------
    // PANEL AÇILMADAN ÖNCE TEKLİFLERİ DOLDURAN HAZIRLIK METODU
    // --------------------------------------------------------------------
    public void PrepareTradeUI()
    {
        PopulateTradeOffers();
        Canvas.ForceUpdateCanvases();

        if (tradeScrollRect != null)
        {
            tradeScrollRect.verticalNormalizedPosition = 1f;
            tradeScrollRect.StopMovement();
            tradeScrollRect.velocity = Vector2.zero;
        }

        EventSystem.current?.SetSelectedGameObject(null);
    }

    // --------------------------------------------------------------------
    // PLAYERINPUTROUTER TARAFINDAN ÇAĞRILAN PANEL AÇ/KAPA METODU
    // --------------------------------------------------------------------
    public void ToggleTradePanel()
    {
        bool next = !tradeUIPanel.activeSelf;

        if (next)   // açılıyorsa
        {
            PrepareTradeUI();
        }

        tradeUIPanel.SetActive(next);

        if (interactPromptUI != null)
            interactPromptUI.SetActive(!next);   // panel açıldığında prompt gizlensin
    }

    // --------------------------------------------------------------------
    // TRADE TEKLİFLERİNİ YERLEŞTİREN ESKİ METOD (DEĞİŞMEDİ)
    // --------------------------------------------------------------------
    private void PopulateTradeOffers()
    {
        float prevScroll = tradeScrollRect ? tradeScrollRect.verticalNormalizedPosition : 1f;

        if (!layoutDone)
        {
            for (int i = tradeOffersContainer.childCount - 1; i >= 0; i--)
                Destroy(tradeOffersContainer.GetChild(i).gameObject);

            float y = -topPadding;

            foreach (var offer in tradeOffers)
            {
                var go = Instantiate(tradeOfferButtonPrefab, tradeOffersContainer);
                var btn = go.GetComponent<TradeOfferButton>();

                var rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);

                float h = Mathf.Max(rt.sizeDelta.y, LayoutUtility.GetPreferredHeight(rt));
                rt.anchoredPosition = new Vector2(0f, y);
                y -= (h + rowSpacing);

                spawned.Add(btn);
            }

            var contentRT = (RectTransform)tradeOffersContainer;
            float needed = -y - rowSpacing + bottomPadding;
            if (needed < 0f) needed = 0f;
            var sz = contentRT.sizeDelta;
            contentRT.sizeDelta = new Vector2(sz.x, needed);

            layoutDone = true;
        }

        for (int i = 0; i < spawned.Count && i < tradeOffers.Count; i++)
            spawned[i].Setup(tradeOffers[i]);

        Canvas.ForceUpdateCanvases();

        if (tradeScrollRect)
        {
            tradeScrollRect.StopMovement();
            tradeScrollRect.velocity = Vector2.zero;
            tradeScrollRect.verticalNormalizedPosition = prevScroll;
        }

        EventSystem.current?.SetSelectedGameObject(null);
    }

    // --------------------------------------------------------------------
    // TRADE EXECUTION — OLDUKÇA DOĞRU, HİÇ DOKUNMADIM
    // --------------------------------------------------------------------
    private bool HasEnoughResources(TradeOffer offer)
    {
        if (playerStats == null || offer == null) return false;

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
        if (offer.requiredStone > 0) Inventory.Instance.TryConsume(offer.stoneSO, offer.requiredStone);
        if (offer.requiredWood > 0) Inventory.Instance.TryConsume(offer.woodSO, offer.requiredWood);
        if (offer.requiredScrapMetal > 0) Inventory.Instance.TryConsume(offer.scrapSO, offer.requiredScrapMetal);
        if (offer.requiredMeat > 0) Inventory.Instance.TryConsume(offer.meatSO, offer.requiredMeat);
        if (offer.requiredDeerHide > 0) Inventory.Instance.TryConsume(offer.deerHideSO, offer.requiredDeerHide);
        if (offer.requiredRabbitHide > 0) Inventory.Instance.TryConsume(offer.rabbitHideSO, offer.requiredRabbitHide);
        if (offer.requiredHerb > 0) Inventory.Instance.TryConsume(offer.herbSO, offer.requiredHerb);
        if (offer.requiredAmmo > 0) Inventory.Instance.TryConsume(offer.ammoSO, offer.requiredAmmo);
    }

    public void ExecuteTrade(TradeOffer offer)
    {
        if (!HasEnoughResources(offer))
        {
            Debug.Log("Yetersiz materyal!");
            return;
        }

        DeductResources(offer);

        if (offer.rewardKind == RewardKind.Resource)
        {
            if (offer.rewardItemSO != null && offer.rewardAmount > 0)
                Inventory.Instance.TryAdd(offer.rewardItemSO, offer.rewardAmount);
        }
        else if (offer.rewardKind == RewardKind.WeaponPart)
        {
            if (offer.partToGive != null && offer.amountToGive > 0)
                Inventory.Instance.TryAdd(offer.partToGive, offer.amountToGive);
        }

        PopulateTradeOffers();
    }

    // --------------------------------------------------------------------
    // PLAYER NEAR — ORİJİNAL
    // --------------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = true;

        if (interactPromptUI != null)
            interactPromptUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = false;

        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // Panel açıksa kapatmak istersen:
        // tradeUIPanel.SetActive(false);
    }

    public bool PlayerIsNear() => isPlayerNearby;
}
