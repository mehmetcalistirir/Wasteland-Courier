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
    public WeaponData weapon;            // Craft edeceğin silah
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
        // WeaponData'da weaponName varsa onu kullan, yoksa asset name
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

    // --------------------------------------
    //  Bu tarifi craft edebiliyor muyuz?
    // --------------------------------------
    public bool CanCraft(WeaponRecipe recipe)
    {
        if (recipe == null || recipe.weapon == null) return false;
        if (IsUnlocked(recipe.weapon)) return false; // Zaten açık

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

    // 1) Kaynakları tüket
    foreach (var cost in recipe.costs)
    {
        if (cost.item == null || cost.amount <= 0)
            continue;

        Inventory.Instance.TryConsume(cost.item, cost.amount);
    }

    // 2) Silahı kalıcı olarak aç
    string key = GetKey(recipe.weapon);
    unlockedWeapons.Add(key);
    Debug.Log($"🔓 Silah craft edildi ve kalıcı açıldı → {key}");

    // 3) Silahı ENVANTERE EKLE
    ItemData weaponItem = recipe.weapon; // WeaponData, ItemData’dan türemiş
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
    WeaponSlotManager.Instance.EquipWeapon(weaponItem);
    Debug.Log($"🎯 Oyuncuya takıldı → {weaponItem.itemName}");

    return true;
}


}
