using System.Collections.Generic;
using UnityEngine;

public class CaravanInventory : MonoBehaviour
{
    // WeaponType enum'un zaten WeaponData'da kullanılıyor
    public Dictionary<WeaponType, List<WeaponData>> storedWeapons =
        new Dictionary<WeaponType, List<WeaponData>>();

    private void Awake()
    {
        // Tüm WeaponType değerleri için boş liste oluştur
        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            storedWeapons[type] = new List<WeaponData>();
        }
    }

    public void StoreWeapon(WeaponData data)
    {
        if (!storedWeapons.ContainsKey(data.weaponType))
            storedWeapons[data.weaponType] = new List<WeaponData>();

        storedWeapons[data.weaponType].Add(data);
        Debug.Log($"{data.weaponType} türünde bir silah karavana eklendi: {data.name}");
    }

    public List<WeaponData> GetWeapons(WeaponType type)
    {
        if (!storedWeapons.ContainsKey(type))
            storedWeapons[type] = new List<WeaponData>();

        return storedWeapons[type];
    }

    public CaravanSaveData GetSaveData()
{
    CaravanSaveData save = new();

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



}
