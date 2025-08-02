using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.gold += amount;
                Debug.Log($"💰 Altın alındı! Yeni bakiye: {stats.gold}");
            }

            Destroy(gameObject);
        }
    }
}
