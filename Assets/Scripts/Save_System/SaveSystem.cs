using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string savePath =
        Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave() => File.Exists(savePath);

    public static void DeleteSave()
    {
        if (HasSave())
            File.Delete(savePath);
    }

    // ============================================================
    // SAVE
    // ============================================================
    public static void SavePlayerAndInventory(
        Transform player,
        Inventory inventory,
        PlayerStats stats,
        PlayerWeapon playerWeapon)
    {
        if (!player || !inventory || !stats)
        {
            Debug.LogWarning("SavePlayerAndInventory: Eksik referans!");
            return;
        }

        SaveData data = new SaveData();

        // Position
        Vector3 p = player.position;
        data.posX = p.x;
        data.posY = p.y;
        data.posZ = p.z;

        // Stats
        data.currentHealth = stats.currentHealth;
        data.maxHealth = stats.maxHealth;
        data.currentStamina = stats.GetStamina();
        data.maxStamina = stats.GetMaxStamina();
        data.currentHunger = stats.currentHunger;
        data.maxHunger = stats.maxHunger;
        data.gold = stats.gold;

        // Inventory items
        data.inventory.Clear();
        foreach (var slot in inventory.slots)
        {
            if (slot == null || slot.data == null) continue;

            InventoryItemData itemData = new InventoryItemData
            {
                itemID = slot.data.itemID,
                amount = slot.count
            };

            if (slot.magazineInstance != null)
            {
                itemData.hasMagazineInstance = true;
                itemData.magazineCurrentAmmo =
                    slot.magazineInstance.currentAmmo;
            }
            else
            {
                itemData.hasMagazineInstance = false;
            }

            data.inventory.Add(itemData);
        }


        // Ammo Pool
        data.ammoTypeIDs.Clear();
        data.ammoAmounts.Clear();
        foreach (var pair in inventory.GetAmmoPool())
        {
            data.ammoTypeIDs.Add(pair.Key.ammoId);
            data.ammoAmounts.Add(pair.Value);
        }

        // Weapons
        WeaponSlotManager wsm = WeaponSlotManager.Instance;
        if (wsm != null)
        {
            for (int i = 0; i < 3; i++)
                data.equippedWeaponIDs[i] =
                    wsm.slots[i] != null ? wsm.slots[i].itemID : "";

            data.activeSlotIndex = wsm.activeSlotIndex;
        }

        // Equipped magazine (slot index + ammo)
        data.equippedMagazineSlotIndex = -1;
        data.equippedMagazineAmmo = 0;

        if (playerWeapon != null && playerWeapon.GetCurrentMagazine() != null)
        {
            MagazineInstance mag = playerWeapon.GetCurrentMagazine();

            for (int i = 0; i < inventory.slots.Length; i++)
            {
                if (inventory.slots[i].magazineInstance == mag)
                {
                    data.equippedMagazineSlotIndex = i;
                    data.equippedMagazineAmmo = mag.currentAmmo;
                    break;
                }
            }
        }

        // Craft unlocks
        data.unlockedWeaponIDs.Clear();
        if (CraftingSystem.Instance != null)
        {
            foreach (var id in CraftingSystem.Instance.unlockedWeapons)
                data.unlockedWeaponIDs.Add(id);
        }

        // Caravan
        if (CaravanInventory.Instance != null)
            data.caravanSave = CaravanInventory.Instance.GetSaveData();

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("💾 SAVE → Başarılı");
    }

    // ============================================================
    // LOAD
    // ============================================================
    public static void LoadPlayerAndInventory(
        Transform player,
        Inventory inventory,
        PlayerStats stats,
        PlayerWeapon playerWeapon)
    {
        if (!player || !inventory || !stats)
        {
            Debug.LogWarning("LoadPlayerAndInventory: Eksik referans!");
            return;
        }

        if (!HasSave())
        {
            Debug.LogWarning("LoadPlayerAndInventory: Kayıt yok.");
            return;
        }

        SaveData data =
            JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));

        // Position
        Vector3 pos = new Vector3(data.posX, data.posY, data.posZ);
        if (pos.y < 1f) pos.y = 1f;
        player.position = pos;

        // Stats
        stats.maxHealth = data.maxHealth;
        stats.currentHealth = data.currentHealth;
        stats.RefreshHealthUI();

        stats.maxStamina = data.maxStamina;
        stats.ResetStamina();
        stats.ModifyStamina(data.currentStamina - stats.GetStamina());

        stats.maxHunger = data.maxHunger;
        stats.currentHunger = data.currentHunger;
        stats.gold = data.gold;

        // Inventory
        inventory.ClearInventory();
        foreach (var it in data.inventory)
        {
            ItemData so = ItemDatabase.Get(it.itemID);
            if (so == null) continue;

            inventory.TryAdd(so, it.amount);

            // 🔥 Şarjörse, instance’ı düzelt
            if (it.hasMagazineInstance)
            {
                // En son eklenen slotu bul
                InventoryItem slot = inventory.GetLastAddedSlot();

                if (slot != null && slot.magazineInstance != null)
                {
                    slot.magazineInstance.currentAmmo =
                        it.magazineCurrentAmmo;
                }
            }
        }


        // Ammo pool
        inventory.ClearAmmoPool();
        for (int i = 0; i < data.ammoTypeIDs.Count; i++)
        {
            AmmoTypeData ammoType =
                AmmoTypeRegistry.Get(data.ammoTypeIDs[i]);
            if (ammoType != null)
                inventory.AddAmmo(ammoType, data.ammoAmounts[i]);
        }

        // Weapons
        WeaponSlotManager wsm = WeaponSlotManager.Instance;
        if (wsm != null)
        {
            for (int i = 0; i < 3; i++)
            {
                ItemData so = ItemDatabase.Get(data.equippedWeaponIDs[i]);
                wsm.slots[i] =
                    so is WeaponItemData wid ? wid.weaponData : null;
            }

            wsm.activeSlotIndex =
                Mathf.Clamp(data.activeSlotIndex, 0, 2);
            wsm.SwitchSlot(wsm.activeSlotIndex);
        }

        // Equipped magazine
        if (playerWeapon != null &&
            data.equippedMagazineSlotIndex >= 0 &&
            data.equippedMagazineSlotIndex < inventory.slots.Length)
        {
            var mag =
                inventory.slots[data.equippedMagazineSlotIndex]
                    .magazineInstance;

            if (mag != null)
            {
                mag.currentAmmo = data.equippedMagazineAmmo;
                playerWeapon.SetCurrentMagazine(mag);
            }
        }

        // Craft unlocks
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.unlockedWeapons.Clear();
            foreach (var id in data.unlockedWeaponIDs)
                CraftingSystem.Instance.unlockedWeapons.Add(id);
        }

        // Caravan
        if (data.caravanSave != null &&
            CaravanInventory.Instance != null)
        {
            CaravanInventory.Instance.LoadFromData(data.caravanSave);
        }


        Debug.Log("📥 LOAD → Başarılı");
    }
}
