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
            case ResourceType.Arrow:
                stats.AddResource("Arrow", amount);
                break;
            case ResourceType.Spear:
                stats.AddResource("Spear", amount);
                break;
            case ResourceType.Herb: stats.AddResource("Herb", amount); break;
            case ResourceType.CookedMeat: stats.AddResource("CookedMeat", amount); break;   // ✅ EKLENDİ

            case ResourceType.Ammo:
{
    var wsm = WeaponSlotManager.Instance;
    if (wsm == null) break;

    int slot = ammoToActiveSlot ? wsm.activeSlotIndex : ammoTargetSlotIndex;

    // O slotta takılı silah var mı?
    var bp = wsm.GetBlueprintForSlot(slot);
    var wd = bp != null ? bp.weaponData : null;
    if (wd == null)
    {
        Debug.Log("[AmmoPickup] Bu slota takılı silah yok, mermi eklenmedi.");
        break;
    }

    // Eklenecek miktarı hesapla
    int maxCap = wd.maxAmmoCapacity;
    int add = ammoAmountAsPercentOfMax
              ? Mathf.Max(1, Mathf.RoundToInt(maxCap * ammoPercentOfMax))
              : ammoFixedAmount;

    // Mevcut clip/reserve'i çek, rezerve ekle ve (istersen) max'a clamp et
    var (clip, reserve) = wsm.GetAmmoStateForSlot(slot);
    int newReserve = reserve + add;
    if (clampToMaxCapacity) newReserve = Mathf.Min(newReserve, maxCap);

    // State’i geri yaz ve UI’ı güncelle
    wsm.SetAmmoStateForSlot(slot, clip, newReserve);

    Debug.Log($"[AmmoPickup] slot {slot}: reserve {reserve} -> {newReserve} (+{add})");
    break;
}


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
