using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Tarifler (ScriptableObject WeaponCraftRecipe)")]
    public List<WeaponCraftRecipe> recipes = new();

    public CaravanInventory caravanInventory;

    // Craft edilip kalıcı açılan silahlar
    public HashSet<string> unlockedWeapons = new();

    private void Awake()
    {
        Instance = this;

        if (caravanInventory == null)
            caravanInventory = FindObjectOfType<CaravanInventory>();
    }

    // ------------------------------------------------------------
    // Tarife bağlı WeaponData'yı al
    // ------------------------------------------------------------
    private WeaponData GetWeapon(WeaponCraftRecipe recipe)
    {
        return recipe?.resultWeapon;
    }

    private string GetKey(WeaponData weapon)
    {
        if (weapon == null) return "";

        if (!string.IsNullOrWhiteSpace(weapon.itemName))
            return weapon.itemName;

        return weapon.name;
    }

    public bool IsUnlocked(WeaponCraftRecipe recipe)
    {
        WeaponData w = GetWeapon(recipe);
        return unlockedWeapons.Contains(GetKey(w));
    }

    // ------------------------------------------------------------
    // Craft edilebilir mi?
    // ------------------------------------------------------------
    public bool CanCraft(WeaponCraftRecipe recipe)
    {
        if (recipe == null) return false;

        WeaponData weapon = GetWeapon(recipe);
        if (weapon == null) return false;

        if (IsUnlocked(recipe)) return false;

        foreach (var cost in recipe.costs)
        {
            if (!Inventory.Instance.HasEnough(cost.item, cost.amount))
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Craft → KARAVANA EKLE
    // ------------------------------------------------------------
public bool TryCraft(WeaponCraftRecipe recipe)
{
    if (!CanCraft(recipe))
    {
        Debug.Log("Craft başarısız: Gereken koşullar sağlanmıyor.");
        return false;
    }

    WeaponData weapon = GetWeapon(recipe);

    // 1) Kaynakları tüket
    foreach (var cost in recipe.costs)
        Inventory.Instance.TryConsume(cost.item, cost.amount);

    // 2) Silahı unlock et
    unlockedWeapons.Add(GetKey(weapon));

    // ❌ Eskisi: Craft edilen silah karavana ekleniyordu
    // caravanInventory.StoreWeapon(weapon);

    // ✔ YENİ DAVRANIŞ:
    // 3) Craft edilen silah DOĞRUDAN oyuncuya takılır
    WeaponSlotManager.Instance.EquipCraftedWeapon(weapon);

    Debug.Log($"✔ CRAFT BAŞARILI → {weapon.itemName} oyuncuya takıldı!");

    return true;
}

}
