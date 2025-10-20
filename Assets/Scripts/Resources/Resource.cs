using UnityEngine;

public class Resource : MonoBehaviour
{

    public ItemData itemData;       // Yeni sistemdeki ScriptableObject

    [Header("Resource Settings")]
    public ResourceType type;
    public int amount = 1;

    [Header("Collection Mechanics")]
    [Tooltip("Bu kaynağı toplamak için kaç kez 'E'ye basılması gerekiyor.")]
    public int hitsRequired = 3;

    [Tooltip("İki 'E' basışı arasındaki minimum bekleme süresi (saniye).")]
    public float hitCooldown = 1.0f;

    // --- AMMO PICKUP AYARLARI ---
    [Header("Ammo Pickup")]
    [Tooltip("Mermiyi aktif silaha mı ekleyelim? (false ise hedef slota ekler)")]
    public bool ammoToActiveSlot = true;

    [Tooltip("aktif değilse, bu slota ekle")]
    public int ammoTargetSlotIndex = 0;

    [Tooltip("Miktarı 'maxAmmoCapacity' yüzdesi olarak mı hesaplayalım?")]
    public bool ammoAmountAsPercentOfMax = true;

    [Range(0f, 1f)] public float ammoPercentOfMax = 0.25f; // örn. %25
    public int ammoFixedAmount = 12;                      // yüzdelik kapalıysa sabit ek
    [Tooltip("Rezerv mermiyi weaponData.maxAmmoCapacity üstüne taşmayacak şekilde sınırla")]
    public bool clampToMaxCapacity = true;


    // --- Sistem Değişkenleri ---
    private int currentHits = 0;
    private bool canBeHit = true; // Cooldown kontrolü için

    public Sprite normalSprite;
    public Sprite highlightedSprite;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null && normalSprite != null)
            sr.sprite = normalSprite;
    }


    public void Collect()
    {
        if (itemData == null)
    {
        Debug.LogError($"[Resource] itemData null! '{gameObject.name}' üzerinde itemData atanmamış.");
        return;
    }

    if (Inventory.Instance == null)
    {
        Debug.LogError("[Resource] Inventory.Instance bulunamadı.");
        return;
    }

        Inventory.Instance.TryAdd(itemData, amount);

        var stats = GameObject.FindWithTag("Player")?.GetComponent<PlayerStats>();
        if (stats == null) return;


        if (itemData is PartItemData partItem)
        {
            PlayerInventory.Instance.AddPart(partItem.partType, amount);
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

    public void Highlight(bool state)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (state && highlightedSprite != null)
            sr.sprite = highlightedSprite;
        else if (normalSprite != null)
            sr.sprite = normalSprite;
    }


}
