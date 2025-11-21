using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceCost
{
    public ItemData item;   // Taş, odun, metal, vs.
    public int amount = 1;  // Kaç tane gerekiyor
}

[System.Serializable]
public class WeaponRecipe
{
    public string id;                    // Opsiyonel: "Pistol", "Rifle" vs.

    [Header("Envantere girecek item (ZORUNLU)")]
    public WeaponItemData weaponItem;    // Inventory'e girecek item

    [Header("Silah statları (boş bırakılırsa weaponItem.weaponData kullanılır)")]
    public WeaponData weaponData;        // Silahın gerçek statları

    public List<ResourceCost> costs = new();  // Gerekli kaynaklar
}

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Tarifler (Recipe List)")]
    public List<WeaponRecipe> recipes = new();

    // Hangi silahların craft edildiğini tutar (sadece bellek içinde)
    public HashSet<string> unlockedWeapons = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --------------------------------------
    //  Yardımcı: Silah için benzersiz anahtar
    // --------------------------------------
    private string GetKey(WeaponData weapon)
    {
        if (weapon == null) return "";

        if (!string.IsNullOrWhiteSpace(weapon.itemName))
            return weapon.itemName;

        return weapon.name;
    }

    // --------------------------------------
    //  Bu silah daha önce craft edildi mi?
    // --------------------------------------
    public bool IsUnlocked(WeaponData weapon)
    {
        if (weapon == null) return false;
        return unlockedWeapons.Contains(GetKey(weapon));
    }

    // Tarif içinden güvenli şekilde WeaponData çek
    private WeaponData GetWeaponDataFromRecipe(WeaponRecipe recipe)
    {
        if (recipe == null) return null;

        if (recipe.weaponData != null)
            return recipe.weaponData;

        if (recipe.weaponItem != null)
            return recipe.weaponItem.weaponData;

        return null;
    }

    // --------------------------------------
    //  Bu tarifi craft edebiliyor muyuz?
    // --------------------------------------
    public bool CanCraft(WeaponRecipe recipe)
    {
        if (recipe == null) return false;

        WeaponData weapon = GetWeaponDataFromRecipe(recipe);
        if (weapon == null) return false;

        if (IsUnlocked(weapon)) return false; // Zaten açık
        if (Inventory.Instance == null) return false;

        foreach (var cost in recipe.costs)
        {
            if (cost.item == null || cost.amount <= 0)
                continue;

            if (!Inventory.Instance.HasEnough(cost.item, cost.amount))
                return false;
        }

        return true;
    }

    // --------------------------------------
    //  Craft et (1 kere)
    // --------------------------------------
    public bool TryCraft(WeaponRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.Log("CraftingSystem: Craft şartları sağlanmıyor.");
            return false;
        }

        WeaponData weapon = GetWeaponDataFromRecipe(recipe);
        if (weapon == null)
        {
            Debug.LogError("CraftingSystem: Recipe içinde geçerli WeaponData yok!");
            return false;
        }

        // 1) Kaynakları tüket
        foreach (var cost in recipe.costs)
        {
            if (cost.item == null || cost.amount <= 0)
                continue;

            Inventory.Instance.TryConsume(cost.item, cost.amount);
        }

        // 2) Silahı kalıcı olarak aç
        string key = GetKey(weapon);
        unlockedWeapons.Add(key);
        Debug.Log($"🔓 Silah craft edildi ve kalıcı açıldı → {key}");

        // 3) Silahı ENVANTERE EKLE
        ItemData weaponItem = recipe.weaponItem;
        if (weaponItem == null)
        {
            Debug.LogError("CraftingSystem: weaponItem atanmadı! Envantere eklenemedi.");
            return false;
        }

        bool added = Inventory.Instance.TryAdd(weaponItem, 1);

        if (!added)
        {
            Debug.LogWarning($"⚠ Envanter dolu, {weaponItem.itemName} envantere eklenemedi.");
        }
        else
        {
            Debug.Log($"📦 Envantere eklendi → {weaponItem.itemName}");
        }

        // 4) Silahı OTOMATİK TAK
        if (WeaponSlotManager.Instance != null)
        {
            // Silahı slot'a yerleştir
            WeaponSlotManager.Instance.EquipWeapon(weaponItem);

            // 🔥 OTOMATİK SLOT DEĞİŞTİR (MAİN FIX)
            int slotIndex = (int)WeaponSlotManager.Instance.GetSlotForWeapon(weapon);
            WeaponSlotManager.Instance.SwitchSlot(slotIndex);

            Debug.Log($"🎯 Oyuncuya takıldı ve slot değiştirildi → {weaponItem.itemName}");
        }
        else
        {
            Debug.LogWarning("CraftingSystem: WeaponSlotManager.Instance = null, silah takılamadı!");
        }

        return true;
    }

}
