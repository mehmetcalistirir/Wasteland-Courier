using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Inventory Item (opsiyonel)")]
    public ItemData item;
    public int minAmount = 1;
    public int maxAmount = 1;

    [Header("Ammo (opsiyonel)")]
    public AmmoTypeData ammoType;

    [Header("Visual")]
    public Sprite normalSprite;
    public Sprite highlightedSprite;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr && normalSprite)
            sr.sprite = normalSprite;
    }

    public void Highlight(bool state)
    {
        if (!sr) return;

        if (highlightedSprite != null)
            sr.sprite = state ? highlightedSprite : normalSprite;
    }

    public void Collect()
    {
        int amount = Random.Range(minAmount, maxAmount + 1);

        bool collected = false;

        // 1️⃣ Inventory Item
        if (item != null)
        {
            Inventory.Instance.TryAdd(item, amount);
            collected = true;
        }

        // 2️⃣ Ammo
        if (ammoType != null)
        {
            Inventory.Instance.AddAmmo(ammoType, amount);
            collected = true;
        }

        if (!collected)
        {
            Debug.LogWarning(
                $"[Collectible] {gameObject.name} üzerinde item veya ammo tanımlı değil!"
            );
            return;
        }

        Destroy(gameObject);
    }
}
