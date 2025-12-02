using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave() => File.Exists(savePath);

    public static void DeleteSave()
    {
        if (HasSave())
            File.Delete(savePath);
    }

    // ============================================================
    //                          SAVE
    // ============================================================

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

        // ---- Pozisyon ----
        data.posX = player.position.x;
        data.posY = player.position.y;
        data.posZ = player.position.z;

        // ---- Statlar ----
        data.currentHealth = stats.currentHealth;
        data.maxHealth = stats.maxHealth;

        data.currentStamina = stats.GetStamina();
        data.maxStamina = stats.GetMaxStamina();

        data.currentHunger = stats.currentHunger;
        data.maxHunger = stats.maxHunger;

        data.gold = stats.gold;

        // ---- Envanter ----
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

        // ---- Craft ile açılmış silahlar ----
        data.unlockedWeaponIDs.Clear();
        if (CraftingSystem.Instance != null)
        {
            foreach (var id in CraftingSystem.Instance.unlockedWeapons)
                data.unlockedWeaponIDs.Add(id);
        }

        // ---- Ekipman Silah Slotları (WeaponSlotManager) ----
        WeaponSlotManager wsm = WeaponSlotManager.Instance;
        if (wsm != null)
        {
            for (int i = 0; i < 3; i++)
            {
                WeaponData wd = wsm.slots[i];

                if (wd != null)
                    data.equippedWeaponIDs[i] = wd.itemID;
                else
                    data.equippedWeaponIDs[i] = "";

                var ammo = wsm.GetAmmo(i);
                data.slotClip[i] = ammo.clip;
                data.slotReserve[i] = ammo.reserve;
            }

            data.activeSlotIndex = wsm.activeSlotIndex;
        }

        // ---- Karavan Envanteri ----
        if (CaravanInventory.Instance != null)
            data.caravanSave = CaravanInventory.Instance.GetSaveData();

        // ---- JSON Yaz ----
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("💾 SAVE → Tüm veriler kaydedildi.");
    }

    // ============================================================
    //                          LOAD
    // ============================================================

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

        // ---- Pozisyon ----
        Vector3 loadedPos = new Vector3(data.posX, data.posY, data.posZ);
        if (loadedPos.y < 1f) loadedPos.y = 1f;  // yere gömülme önlemi
        player.position = loadedPos;

        // ---- Statlar ----
        stats.maxHealth = data.maxHealth;
        stats.currentHealth = data.currentHealth;
        stats.RefreshHealthUI();

        stats.maxStamina = data.maxStamina;
        stats.ResetStamina();
        stats.ModifyStamina(data.currentStamina - stats.GetStamina());

        stats.maxHunger = data.maxHunger;
        stats.currentHunger = data.currentHunger;

        stats.gold = data.gold;

        // ---- Envanter ----
        inventory.ClearInventory();
        foreach (var item in data.inventory)
        {
            if (string.IsNullOrEmpty(item.itemID) || item.amount <= 0) continue;

            ItemData so = ItemDatabase.Get(item.itemID);
            if (so == null) continue;

            inventory.TryAdd(so, item.amount);
        }

        // ---- Craft ile açılmış silahlar ----
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.unlockedWeapons.Clear();
            foreach (var id in data.unlockedWeaponIDs)
                CraftingSystem.Instance.unlockedWeapons.Add(id);
        }

        // ---- WeaponSlotManager yükleme ----
        WeaponSlotManager wsm = WeaponSlotManager.Instance;
        if (wsm != null)
        {
            for (int i = 0; i < 3; i++)
            {
                string id = data.equippedWeaponIDs[i];

                if (!string.IsNullOrEmpty(id))
                {
                    ItemData so = ItemDatabase.Get(id);
                    if (so is WeaponItemData wid)
                    {
                        wsm.slots[i] = wid.weaponData;
                        wsm.clip[i] = data.slotClip[i];
                        wsm.reserve[i] = data.slotReserve[i];
                    }
                    else
                    {
                        wsm.slots[i] = null;
                        wsm.clip[i] = 0;
                        wsm.reserve[i] = 0;
                    }
                }
                else
                {
                    wsm.slots[i] = null;
                    wsm.clip[i] = 0;
                    wsm.reserve[i] = 0;
                }
            }

            // Aktif slotu uygula
            wsm.activeSlotIndex = Mathf.Clamp(data.activeSlotIndex, 0, 2);
            wsm.SwitchSlot(wsm.activeSlotIndex);
        }

        // ---- Karavan Yükleme ----
        if (data.caravanSave != null && CaravanInventory.Instance != null)
        {
            CaravanInventory.Instance.LoadFromData(data.caravanSave);
        }

        Debug.Log("📥 LOAD → Tüm veriler geri yüklendi.");
    }
}
