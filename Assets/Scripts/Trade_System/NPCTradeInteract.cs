using UnityEngine;

public class NPCTradeInteract : MonoBehaviour
{
    [Header("Trade")]
    public TradeUIController tradeUI;

    private NPCTradeInventory tradeInventory;
    private bool playerInRange;

    private void Awake()
    {
        tradeInventory = GetComponent<NPCTradeInventory>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenTrade();
        }
    }

    void OpenTrade()
    {
        if (tradeInventory == null)
        {
            Debug.LogWarning("NPCTradeInventory yok!");
            return;
        }

        tradeUI.Open(tradeInventory);

        // UI açıkken oyunu durdurmak istersen
        Time.timeScale = 0f;
    }
}
