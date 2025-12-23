using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NPCTradeInteract : MonoBehaviour
{
    [Header("Trade")]
    public TradeUIController tradeUI;

    private NPCTradeInventory tradeInventory;

    public bool playerInRange { get; private set; }

    // ======================================================
    // LIFECYCLE
    // ======================================================
    private void Awake()
    {
        tradeInventory = GetComponent<NPCTradeInventory>();

        // Güvenlik: collider trigger olmalı
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    // ======================================================
    // PROMPT GÖSTERME
    // ======================================================
    private void Update()
    {
        // Trade panel açıkken prompt gösterme
        if (playerInRange && !PlayerInputRouter.Instance.tradePanel.activeSelf)
        {
            InteractionPromptUI.Instance?.Show("Ticaret Yap");
        }
    }

    // ======================================================
    // TRIGGER
    // ======================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        // Router'a bu NPC'nin aktif olduğunu bildir
        PlayerInputRouter.Instance?.SetActiveTradeNPC(this);

        // İlk girişte prompt göster
        InteractionPromptUI.Instance?.Show("Ticaret Yap");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        // Router'dan aktif NPC'yi temizle
        PlayerInputRouter.Instance?.SetActiveTradeNPC(null);

        // Prompt'u kapat
        InteractionPromptUI.Instance?.Hide();
    }

    // ======================================================
    // ROUTER TARAFINDAN ÇAĞRILIR
    // ======================================================
    public void OpenTrade()
    {
        if (tradeUI == null || tradeInventory == null)
            return;

        // Prompt kesin kapanır
        InteractionPromptUI.Instance?.Hide();

        // Trade UI aç
        tradeUI.Open(tradeInventory);

        // Oyun duraklat
        Time.timeScale = 0f;
        GameStateManager.SetPaused(true);
    }
}
