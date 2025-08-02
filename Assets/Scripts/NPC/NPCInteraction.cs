using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class NPCInteraction : MonoBehaviour
{
    public List<TradeOffer> tradeOffers;
    public GameObject tradeUIPanel;

    private bool isPlayerNearby = false;
    private bool isPanelOpen = false;
    public GameObject interactPromptUI; // "E" yazan UI objesi
    public Transform tradeOffersContainer; // Ticaret butonlarının oluşturulacağı yer
    public GameObject tradeOfferButtonPrefab; // Tek bir ticaret teklifini temsil eden UI butonu
    private PlayerStats playerStats;


    void Start()
    {
        // PlayerStats referansını oyun başında bir kere alalım, daha verimli.
        playerStats = FindObjectOfType<PlayerStats>();
        if (tradeUIPanel != null) tradeUIPanel.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleTradePanel();
        }
    }
    private void ToggleTradePanel()
    {
        bool shouldOpen = !tradeUIPanel.activeSelf;
        tradeUIPanel.SetActive(shouldOpen);

        if (shouldOpen)
        {
            // Panel açıldığında, teklifleri UI'da oluştur.
            PopulateTradeOffers();
            Time.timeScale = 0f; // Oyunu durdur
        }
        else
        {
            Time.timeScale = 1f; // Oyunu devam ettir
        }
    }

    private void PopulateTradeOffers()
    {
        // Önce eski butonları temizle
        foreach (Transform child in tradeOffersContainer)
        {
            Destroy(child.gameObject);
        }

        // Her bir teklif için yeni bir buton oluştur
        foreach (TradeOffer offer in tradeOffers)
        {
            GameObject buttonObject = Instantiate(tradeOfferButtonPrefab, tradeOffersContainer);
            // Butonun üzerindeki script'e gerekli bilgileri ver.
            buttonObject.GetComponent<TradeOfferButton>().Setup(offer, playerStats);
        }
    }

    public void ExecuteTrade(TradeOffer offer)
    {
        // 1. Oyuncunun yeterli materyali var mı diye son bir kontrol.
        if (playerStats.GetResourceAmount("Stone") >= offer.requiredStone &&
            playerStats.GetResourceAmount("Wood") >= offer.requiredWood &&
            playerStats.GetResourceAmount("scrapMetal") >= offer.requiredScrapMetal)
        {
            // 2. Materyalleri oyuncunun envanterinden düş.
            playerStats.RemoveResource("Stone", offer.requiredStone);
            playerStats.RemoveResource("Wood", offer.requiredWood);
            playerStats.RemoveResource("scrapMetal", offer.requiredScrapMetal);

            // 3. Silah parçasını oyuncunun envanterine ekle.
            playerStats.CollectWeaponPart(offer.partToGive, offer.amountToGive);

            Debug.Log($"Takas başarılı! {offer.amountToGive} adet {offer.partToGive} alındı.");

            // 4. UI'ı anında güncelle ki oyuncu değişikliği görsün.
            PopulateTradeOffers();
        }
        else
        {
            Debug.Log("Takas başarısız! Yeterli materyal yok.");
            // İsteğe bağlı: Ekranda bir uyarı sesi çal veya mesaj göster.
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            CloseTrade();
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    void OpenTrade()
    {
        if (tradeUIPanel != null)
        {
            tradeUIPanel.SetActive(true);
            isPanelOpen = true;
            Time.timeScale = 0f;
           // Cursor.visible = true;
        }
    }

    void CloseTrade()
    {
        if (tradeUIPanel != null)
        {
            tradeUIPanel.SetActive(false);
            isPanelOpen = false;
            Time.timeScale = 1f;

            // Sadece oyun normal duruma döndüyse imleci gizle
            /*Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;*/
        }
    }

}
