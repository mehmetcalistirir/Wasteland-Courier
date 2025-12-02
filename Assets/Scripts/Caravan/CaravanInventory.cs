using System.Collections.Generic;
using UnityEngine;

public class CaravanInventory : MonoBehaviour
{
    public static CaravanInventory Instance;

    // WeaponType → List<WeaponData>
    public Dictionary<WeaponType, List<WeaponData>> storedWeapons =
        new Dictionary<WeaponType, List<WeaponData>>();

    private void Awake()
    {
        Instance = this;

        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            storedWeapons[type] = new List<WeaponData>();
        }
    }


    // ============================================
    // 1) WeaponSlotManager tarafından çağrılan sistem
    // ============================================
    public void AddItem(string itemID, int amount = 1)
    {
        ItemData data = ItemDatabase.Get(itemID);
        if (data == null)
        {
            Debug.LogError("CaravanInventory → itemID bulunamadı: " + itemID);
            return;
        }

        WeaponItemData wid = data as WeaponItemData;
        if (wid == null)
        {
            Debug.LogError("CaravanInventory.AddItem → Bu item bir silah değil: " + itemID);
            return;
        }

        StoreWeapon(wid.weaponData);
    }


    // ============================================
    // 2) Silah depolama (WeaponData)
    // ============================================
    public void StoreWeapon(WeaponData data)
    {
        if (!storedWeapons.ContainsKey(data.weaponType))
            storedWeapons[data.weaponType] = new List<WeaponData>();

        storedWeapons[data.weaponType].Add(data);

        Debug.Log($"{data.weaponType} türünde silah karavana eklendi: {data.itemName}");
    }


    // ============================================
    // 3) UI veya Swap için Silah Çekme
    // ============================================
    public WeaponData TakeWeapon(WeaponType type, int index)
    {
        if (!storedWeapons.ContainsKey(type) || storedWeapons[type].Count == 0)
            return null;

        if (index < 0 || index >= storedWeapons[type].Count)
            return null;

        WeaponData w = storedWeapons[type][index];
        storedWeapons[type].RemoveAt(index);

        return w;
    }


    // ============================================
    // 4) Silah listesini döndür (UI için)
    // ============================================
    public List<WeaponData> GetWeapons(WeaponType type)
    {
        if (!storedWeapons.ContainsKey(type))
            storedWeapons[type] = new List<WeaponData>();

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
            WeaponSaveEntry entry = new WeaponSaveEntry();
            entry.type = kvp.Key;
            entry.weaponIDs = new List<string>();

            foreach (var weapon in kvp.Value)
            {
                entry.weaponIDs.Add(weapon.itemID);
            }

            save.weaponEntries.Add(entry);
        }

        return save;
    }
    public bool HasWeapon(WeaponData weapon)
{
    if (!storedWeapons.ContainsKey(weapon.weaponType))
        return false;

    foreach (var w in storedWeapons[weapon.weaponType])
    {
        if (w.itemID == weapon.itemID)
            return true;
    }

    return false;
}

}
