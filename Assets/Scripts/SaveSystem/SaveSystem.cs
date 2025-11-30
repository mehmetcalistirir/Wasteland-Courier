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
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.unlockedWeapons != null)
        {
            foreach (var id in CraftingSystem.Instance.unlockedWeapons)
                data.unlockedWeaponIDs.Add(id);
        }

        // ---- Ekipman silah slotları (WeaponSlotManager) ----
        WeaponSlotManager wsm = WeaponSlotManager.Instance;
        if (wsm != null)
        {
            // Diziler boşsa oluştur
            if (data.equippedWeaponKeys == null || data.equippedWeaponKeys.Length != 3)
                data.equippedWeaponKeys = new string[3];
            if (data.slotClip == null || data.slotClip.Length != 3)
                data.slotClip = new int[3];
            if (data.slotReserve == null || data.slotReserve.Length != 3)
                data.slotReserve = new int[3];

            for (int i = 0; i < 3; i++)
            {
                WeaponData wd = wsm.slots[i];

                // WeaponData → string key (weaponName veya asset name)
                if (wd != null)
                {
                    data.equippedWeaponKeys[i] = GetWeaponKey(wd);
                }
                else
                {
                    data.equippedWeaponKeys[i] = "";
                }

                var ammo = wsm.GetAmmo(i);
                data.slotClip[i] = ammo.clip;
                data.slotReserve[i] = ammo.reserve;
            }

            data.activeSlotIndex = wsm.activeSlotIndex;
        }

        // ---- JSON yaz ----
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("💾 SAVE → Pozisyon + statlar + envanter + unlock silahlar + ekipman slotları kaydedildi.");
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

        // ---- Pozisyon ----
        Vector3 loadedPos = new Vector3(data.posX, data.posY, data.posZ);
        if (loadedPos != Vector3.zero)
        {
            if (loadedPos.y < 1f) loadedPos.y = 1f;
            player.position = loadedPos;
        }

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

        // ---- WeaponSlotManager slotlarını geri yükle ----
        WeaponSlotManager wsm = WeaponSlotManager.Instance;
        if (wsm != null && data.equippedWeaponKeys != null && data.equippedWeaponKeys.Length == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                string key = data.equippedWeaponKeys[i];

                if (!string.IsNullOrEmpty(key))
                {
                    // Save'de tuttuğumuz key'e göre WeaponData bul
                    WeaponData wd = FindWeaponDataByKey(key);
                    if (wd != null)
                    {
                        wsm.slots[i] = wd;
                        // Ammo
                        if (data.slotClip != null && data.slotClip.Length > i)
                            wsm.clip[i] = data.slotClip[i];
                        if (data.slotReserve != null && data.slotReserve.Length > i)
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

            // Aktif slotu geri yükle
            int restoredSlot = Mathf.Clamp(data.activeSlotIndex, 0, 2);
            wsm.activeSlotIndex = restoredSlot;
            // 🔥 ApplyToHandler private olduğu için public SwitchSlot kullanıyoruz
            wsm.SwitchSlot(restoredSlot);
        }

        Debug.Log("📥 LOAD → Pozisyon + statlar + envanter + unlock silahlar + ekipman slotları yüklendi.");
    }

    // ===================== YARDIMCI METOTLAR =====================

    // WeaponData → Save için string key (CraftingSystem ile aynı mantık)
    private static string GetWeaponKey(WeaponData weapon)
{
    if (weapon == null) return "";

    // WeaponData içinde itemName var
    if (!string.IsNullOrWhiteSpace(weapon.itemName))
        return weapon.itemName;

    // fallback: Unity asset adı
    return weapon.name;
}


    // Save'den gelen key'e göre WeaponData bul
    private static WeaponData FindWeaponDataByKey(string key)
{
    if (string.IsNullOrEmpty(key)) return null;
    if (CraftingSystem.Instance == null) return null;

    foreach (var recipe in CraftingSystem.Instance.recipes)
    {
        if (recipe == null) continue;

        WeaponData wd = recipe.resultWeapon;   // YENİ DOĞRU ALAN

        if (wd == null) continue;

        // Key karşılaştırması
        string currentKey = wd.itemName;
        if (string.IsNullOrWhiteSpace(currentKey))
            currentKey = wd.name;

        if (currentKey == key)
            return wd;
    }

    Debug.LogWarning($"SaveSystem: '{key}' için WeaponData bulunamadı.");
    return null;
}


}
