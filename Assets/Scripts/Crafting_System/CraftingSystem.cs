using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Weapon Craft Recipes")]
    public List<WeaponCraftRecipe> recipes = new();

    public CaravanInventory caravanInventory;

    // Craft edilip kalıcı açılan silahlar (itemID)
    public HashSet<string> unlockedWeapons = new();

    private void Awake()
    {
        Instance = this;

        if (caravanInventory == null)
            caravanInventory = FindObjectOfType<CaravanInventory>();
    }

    // ------------------------------------------------------------
    // Recipe → WeaponItemData
    // ------------------------------------------------------------
    private WeaponItemData GetWeaponItem(WeaponCraftRecipe recipe)
    {
        return recipe != null ? recipe.resultWeapon : null;
    }

    private string GetKey(WeaponItemData weaponItem)
    {
        return weaponItem != null ? weaponItem.itemID : string.Empty;
    }

    public bool IsUnlocked(WeaponCraftRecipe recipe)
    {
        WeaponItemData item = GetWeaponItem(recipe);
        return unlockedWeapons.Contains(GetKey(item));
    }

    // ------------------------------------------------------------
    // Craft edilebilir mi?
    // ------------------------------------------------------------
    public bool CanCraft(WeaponCraftRecipe recipe)
    {
        if (recipe == null) return false;

        WeaponItemData weaponItem = GetWeaponItem(recipe);
        if (weaponItem == null) return false;

        if (IsUnlocked(recipe)) return false;

        foreach (var cost in recipe.costs)
        {
            if (!Inventory.Instance.HasEnough(cost.item, cost.amount))
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Craft → Oyuncuya silahı VER
    // ------------------------------------------------------------
    public bool TryCraft(WeaponCraftRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.Log("Craft başarısız: Gereken koşullar sağlanmıyor.");
            return false;
        }

        WeaponItemData weaponItem = GetWeaponItem(recipe);

        // 1) Kaynakları tüket
        foreach (var cost in recipe.costs)
            Inventory.Instance.TryConsume(cost.item, cost.amount);

        // 2) Unlock et (ITEM ID)
        unlockedWeapons.Add(GetKey(weaponItem));

        // 3) Silahı oyuncuya ver / tak
        WeaponSlotManager.Instance.EquipWeapon(weaponItem);


        Debug.Log($"✔ CRAFT BAŞARILI → {weaponItem.itemName} oyuncuya takıldı!");

        return true;
    }
}
