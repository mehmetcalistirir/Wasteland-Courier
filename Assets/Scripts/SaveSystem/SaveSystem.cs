using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave() => File.Exists(savePath);

    public static void DeleteSave()
    {
        if (HasSave())
            File.Delete(savePath);
    }

    // ===================== SAVE =====================

    public static void SavePlayerAndInventory(
        Transform player,
        Inventory inventory,
        PlayerStats stats)
    {
        if (player == null || inventory == null || stats == null)
        {
            Debug.LogWarning("SavePlayerAndInventory: Eksik referans!");
            return;
        }

        SaveData data = new SaveData();

        // Pozisyon
        data.posX = player.position.x;
        data.posY = player.position.y;
        data.posZ = player.position.z;

        // Statlar
        data.currentHealth = stats.currentHealth;
        data.maxHealth = stats.maxHealth;

        data.currentStamina = stats.GetStamina();
        data.maxStamina = stats.GetMaxStamina();

        data.currentHunger = stats.currentHunger;
        data.maxHunger = stats.maxHunger;

        data.gold = stats.gold;

        // Envanter
        data.inventory.Clear();
        foreach (var slot in inventory.slots)
        {
            if (slot.data == null) continue;

            var saved = new InventoryItemData
            {
                itemID = slot.data.itemID,
                amount = slot.count
            };

            data.inventory.Add(saved);
        }

        // Craft ile açılmış silahlar
        data.unlockedWeaponIDs.Clear();
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.unlockedWeapons != null)
        {
            foreach (var id in CraftingSystem.Instance.unlockedWeapons)
                data.unlockedWeaponIDs.Add(id);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("SAVE → Pozisyon + statlar + envanter + unlock silahlar kaydedildi.");
    }

    // ===================== LOAD =====================

    public static void LoadPlayerAndInventory(
        Transform player,
        Inventory inventory,
        PlayerStats stats)
    {
        if (player == null || inventory == null || stats == null)
        {
            Debug.LogWarning("LoadPlayerAndInventory: Eksik referans!");
            return;
        }

        if (!HasSave())
        {
            Debug.LogWarning("LoadPlayerAndInventory: Kayıt bulunamadı.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Pozisyon
        Vector3 loadedPos = new Vector3(data.posX, data.posY, data.posZ);
        if (loadedPos != Vector3.zero)
        {
            if (loadedPos.y < 1f) loadedPos.y = 1f;
            player.position = loadedPos;
        }

        // Statlar
        stats.maxHealth = data.maxHealth;
        stats.currentHealth = data.currentHealth;
        stats.RefreshHealthUI();

        stats.maxStamina = data.maxStamina;
        stats.ResetStamina();
        stats.ModifyStamina(data.currentStamina - stats.GetStamina());

        stats.maxHunger = data.maxHunger;
        stats.currentHunger = data.currentHunger;

        stats.gold = data.gold;

        // Envanter
        inventory.ClearInventory();
        foreach (var item in data.inventory)
        {
            if (string.IsNullOrEmpty(item.itemID) || item.amount <= 0) continue;

            ItemData so = ItemDatabase.Get(item.itemID);
            if (so == null) continue;

            inventory.TryAdd(so, item.amount);
        }

        // Craft ile açılmış silahlar
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.unlockedWeapons.Clear();
            foreach (var id in data.unlockedWeaponIDs)
                CraftingSystem.Instance.unlockedWeapons.Add(id);
        }

        Debug.Log("LOAD → Pozisyon + statlar + envanter + unlock silahlar yüklendi.");
    }
}
