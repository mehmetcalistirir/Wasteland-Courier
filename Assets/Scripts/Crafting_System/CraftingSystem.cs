using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Weapon Craft Recipes")]
    public List<WeaponCraftRecipe> recipes = new();
    
    [Header("Item Craft Recipes")]
    public List<ItemCraftRecipe> itemRecipes = new();

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
            if (!Inventory.Instance.HasEnoughByID(cost.item.itemID, cost.amount))
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
        return false;

    WeaponItemData weaponItem = recipe.resultWeapon;

    // 1️⃣ Kaynakları tüket
    foreach (var cost in recipe.costs)
        Inventory.Instance.TryConsumeByID(
            cost.item.itemID,
            cost.amount);

    // 2️⃣ Unlock
    unlockedWeapons.Add(weaponItem.itemID);

    // 3️⃣ Caravan'a ekle
    CaravanInventory.Instance.StoreWeapon(weaponItem);

    Debug.Log($"✔ WEAPON CRAFT → {weaponItem.itemName} karavana eklendi");

    return true;
}

    public bool CanCraftItem(ItemCraftRecipe recipe)
{
    if (recipe == null) return false;

    if (recipe.unlockOnce && unlockedWeapons.Contains(recipe.unlockID))
        return false;

    foreach (var cost in recipe.costs)
    {
        if (!Inventory.Instance.HasEnoughByID(
            cost.item.itemID,
            cost.amount))
            return false;
    }

    return Inventory.Instance.CanAdd(
        recipe.resultItem,
        recipe.resultAmount);
}

public bool TryCraftItem(ItemCraftRecipe recipe)
{
    if (!CanCraftItem(recipe))
        return false;

    // 1️⃣ Tüket
    foreach (var cost in recipe.costs)
    {
        Inventory.Instance.TryConsumeByID(
            cost.item.itemID,
            cost.amount);
    }

    // 2️⃣ Ver
    Inventory.Instance.TryAdd(
        recipe.resultItem,
        recipe.resultAmount);

    // 3️⃣ Unlock (opsiyonel)
    if (recipe.unlockOnce)
        unlockedWeapons.Add(recipe.unlockID);

    Debug.Log($"✔ ITEM CRAFT → {recipe.resultItem.itemName}");
    return true;
}

}
