using System.Collections.Generic;
using UnityEngine;

public class CaravanInventory : MonoBehaviour
{
    public static CaravanInventory Instance;

    // WeaponType → List<WeaponItemData>
    public Dictionary<WeaponType, List<WeaponItemData>> storedWeapons =
        new Dictionary<WeaponType, List<WeaponItemData>>();

    private void Awake()
    {
        Instance = this;

        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            storedWeapons[type] = new List<WeaponItemData>();
        }
    }

    // ============================================
    // 1) ItemID üzerinden silah ekleme (Save / SlotManager)
    // ============================================
    public void AddItem(string itemID, int amount = 1)
    {
        ItemData data = ItemDatabase.Get(itemID);
        if (data == null)
        {
            Debug.LogError("CaravanInventory → itemID bulunamadı: " + itemID);
            return;
        }

        if (data is not WeaponItemData weaponItem)
        {
            Debug.LogError("CaravanInventory.AddItem → Bu item bir silah değil: " + itemID);
            return;
        }

        StoreWeapon(weaponItem);
    }

    // ============================================
    // 2) Silah depolama (ITEM)
    // ============================================
    public void StoreWeapon(WeaponItemData weaponItem)
    {
        if (weaponItem == null)
            return;

        if (!storedWeapons.ContainsKey(weaponItem.weaponType))
            storedWeapons[weaponItem.weaponType] = new List<WeaponItemData>();

        storedWeapons[weaponItem.weaponType].Add(weaponItem);

        Debug.Log($"{weaponItem.weaponType} türünde silah karavana eklendi: {weaponItem.itemName}");
    }

    // ============================================
    // 3) Silah çekme (UI / Swap)
    // ============================================
    public WeaponItemData TakeWeapon(WeaponType type, int index)
    {
        if (!storedWeapons.ContainsKey(type) || storedWeapons[type].Count == 0)
            return null;

        if (index < 0 || index >= storedWeapons[type].Count)
            return null;

        WeaponItemData item = storedWeapons[type][index];
        storedWeapons[type].RemoveAt(index);

        return item;
    }

    // ============================================
    // 4) Silah listesini döndür (UI için)
    // ============================================
    public List<WeaponItemData> GetWeapons(WeaponType type)
    {
        if (!storedWeapons.ContainsKey(type))
            storedWeapons[type] = new List<WeaponItemData>();

        return storedWeapons[type];
    }

    // ============================================
    // 5) SAVE SYSTEM
    // ============================================
    public CaravanSaveData GetSaveData()
    {
        CaravanSaveData save = new CaravanSaveData();

        foreach (var kvp in storedWeapons)
        {
            WeaponSaveEntry entry = new WeaponSaveEntry
            {
                type = kvp.Key,
                weaponIDs = new List<string>()
            };

            foreach (var weaponItem in kvp.Value)
            {
                entry.weaponIDs.Add(weaponItem.itemID);
            }

            save.weaponEntries.Add(entry);
        }

        return save;
    }

    public bool HasWeapon(WeaponItemData weaponItem)
    {
        if (weaponItem == null)
            return false;

        if (!storedWeapons.ContainsKey(weaponItem.weaponType))
            return false;

        foreach (var w in storedWeapons[weaponItem.weaponType])
        {
            if (w.itemID == weaponItem.itemID)
                return true;
        }

        return false;
    }

    public void LoadFromData(CaravanSaveData save)
    {
        // Tüm silahları temizle
        foreach (var key in storedWeapons.Keys)
            storedWeapons[key].Clear();

        if (save == null)
            return;

        // Save verisini geri yükle
        foreach (var entry in save.weaponEntries)
        {
            if (!storedWeapons.ContainsKey(entry.type))
                storedWeapons[entry.type] = new List<WeaponItemData>();

            foreach (string id in entry.weaponIDs)
            {
                ItemData data = ItemDatabase.Get(id);

                if (data is WeaponItemData weaponItem)
                {
                    storedWeapons[entry.type].Add(weaponItem);
                }
                else
                {
                    Debug.LogWarning("Save'de silah item bulunamadı: " + id);
                }
            }
        }

        Debug.Log("CaravanInventory yüklendi.");
    }
    
}
