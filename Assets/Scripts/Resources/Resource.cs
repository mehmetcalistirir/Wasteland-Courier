using UnityEngine;

public class Resource : MonoBehaviour
{
    [Header("Resource Settings")]
    public ResourceType type;
    public int amount = 1;

    [Header("Collection Mechanics")]
    [Tooltip("Bu kaynağı toplamak için kaç kez 'E'ye basılması gerekiyor.")]
    public int hitsRequired = 3;

    [Tooltip("İki 'E' basışı arasındaki minimum bekleme süresi (saniye).")]
    public float hitCooldown = 1.0f;

    // --- Sistem Değişkenleri ---
    private int currentHits = 0;
    private bool canBeHit = true; // Cooldown kontrolü için


    public void Collect()
    {
        var stats = GameObject.FindWithTag("Player")?.GetComponent<PlayerStats>();
        if (stats == null) return;

        switch (type)
        {
            case ResourceType.Stone:
                stats.AddResource("Stone", amount);
                break;
            case ResourceType.Wood:
                stats.AddResource("Wood", amount);
                break;
            case ResourceType.scrapMetal:
                stats.AddResource("scrapMetal", amount);
                break;
            case ResourceType.Meat:
                stats.AddResource("Meat", amount);
                break;
            case ResourceType.DeerHide:
                stats.AddResource("DeerHide", amount);
                break;
            case ResourceType.RabbitHide:
                stats.AddResource("RabbitHide", amount);
                break;
        }

        Destroy(gameObject);
    }

    public void HitResource()
    {
        // Eğer bekleme süresi aktifse, hiçbir şey yapma.
        if (!canBeHit)
        {
            Debug.Log("BEKLE! Çok hızlı basıyorsun.");
            return;
        }

        currentHits++;
        Debug.Log($"{type} vuruldu! ({currentHits} / {hitsRequired})");

        // İsteğe bağlı: Her vuruşta bir ses çal veya görsel efekt göster.
        // EffectManager.Instance.PlayHitEffect(transform.position);

        // Yeterli vuruş sayısına ulaşıldı mı?
        if (currentHits >= hitsRequired)
        {
            Collect();
        }
        else
        {
            // Henüz toplanmadıysa, bekleme süresini başlat.
            StartCoroutine(HitCooldown());
        }
    }

    private System.Collections.IEnumerator HitCooldown()
    {
        canBeHit = false;
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }
}
