using UnityEngine;

public class NPCTradeInteract : MonoBehaviour
{
    public NPCTradeInventory tradeInventory;
    public bool playerInRange { get; private set; }

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
}
