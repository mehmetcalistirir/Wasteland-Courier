using UnityEngine;

public class BandagePickup : MonoBehaviour
{
    public GenericItemData bandageSO;
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null)
            return;

        Debug.Log("🩹 Bandaj toplandı +" + amount);

        // Envantere ekle
        stats.AddResource(bandageSO, amount);

        // Prefab'ı yok et
        Destroy(gameObject);
    }
}
