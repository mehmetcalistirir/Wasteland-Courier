using UnityEngine;

public class NPCTradeInteract : MonoBehaviour
{
    public TradeUIController tradeUI;

    private NPCTradeInventory tradeInventory;
    public bool playerInRange { get; private set; }

    private void Awake()
    {
        tradeInventory = GetComponent<NPCTradeInventory>();
    }

    private void Update()
    {
        // Trade açıksa prompt gösterme
        if (playerInRange && !PlayerInputRouter.Instance.tradePanel.activeSelf)
        {
            InteractionPromptUI.Instance?.Show("Ticaret Yap");
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        InteractionPromptUI.Instance?.Show("Ticaret Yap");
        PlayerInputRouter.Instance?.SetActiveTradeNPC(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        InteractionPromptUI.Instance?.Hide();
        PlayerInputRouter.Instance?.SetActiveTradeNPC(null);
    }


    // ✅ Router buradan çağıracak
    public void OpenTrade()
    {
        if (tradeUI == null || tradeInventory == null) return;

        InteractionPromptUI.Instance?.Hide();
        tradeUI.Open(tradeInventory);

        // Senin eski davranışın buysa kalsın:
        Time.timeScale = 0f;
        GameStateManager.SetPaused(true);
    }
}
