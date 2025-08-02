using UnityEngine;
using System.Collections.Generic;

public class CaravanInventory : MonoBehaviour
{
    public static CaravanInventory Instance { get; private set; }

    // Craft edilmiş ve şu an depoda olan silahların tariflerini saklar.
    // HashSet, bir silahtan sadece bir tane olmasını garantiler.
    [Header("Starting Stored Weapons")]
    public List<WeaponBlueprint> startingWeapons; 
    private HashSet<WeaponBlueprint> storedWeapons = new HashSet<WeaponBlueprint>();

 [Header("Starting Storage")]
    public List<WeaponBlueprint> startingStoredWeapons;
    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        foreach (var blueprint in startingWeapons)
        {
            if (blueprint != null)
            {
                StoreWeapon(blueprint);
            }
        }
    }

    // CraftingSystem, bir silah üretildiğinde bu fonksiyonu çağırır.
    public void StoreWeapon(WeaponBlueprint blueprint)
    {
        if (blueprint != null && !storedWeapons.Contains(blueprint))
        {
            storedWeapons.Add(blueprint);
            Debug.Log($"{blueprint.weaponName} karavan deposuna eklendi.");
        }
    }

    // UI'daki "Değiştir" butonu bu fonksiyonu çağırır.
    public void SwapWeapon(WeaponBlueprint weaponToEquip)
    {
        // 1. Bu silahın depoda olduğundan emin ol.
        if (!storedWeapons.Contains(weaponToEquip))
        {
            Debug.LogError($"{weaponToEquip.weaponName} depoda değil! Değiştirilemez.");
            return;
        }

        // 2. Bu silahın gitmesi gereken slotu belirle.
        int targetSlotIndex = weaponToEquip.weaponSlotIndexToUnlock;

        // 3. Oyuncunun o an o slotta kullandığı silahı al (depoya geri koymak için).
        WeaponBlueprint weaponToStore = WeaponSlotManager.Instance.GetBlueprintForSlot(targetSlotIndex);

WeaponBlueprint currentlyEquipped = WeaponSlotManager.Instance.GetBlueprintForSlot(targetSlotIndex);
        // 4. Depo envanterini güncelle.
        storedWeapons.Remove(weaponToEquip); // Alınacak silahı depodan çıkar.
        if (currentlyEquipped != null)
        {
            storedWeapons.Add(weaponToStore); // Çıkarılan silahı depoya ekle.
        }

        // 5. WeaponSlotManager'a yeni silahı kuşanmasını söyle.
        WeaponSlotManager.Instance.EquipBlueprint(weaponToEquip);

        Debug.Log($"Oyuncu {weaponToEquip.weaponName} kuşandı. {weaponToStore?.weaponName ?? "Boş Slot"} depoya kaldırıldı.");

        // 6. Değişikliği yansıtmak için Crafting UI'ını güncelle.
        if (WeaponCraftingSystem.Instance != null)
        {
            WeaponCraftingSystem.Instance.UpdateAllBlueprintUI();
        }
    }

    // Bir silahın depoda olup olmadığını kontrol eder.
    public bool IsWeaponStored(WeaponBlueprint blueprint)
    {
        return storedWeapons.Contains(blueprint);
    }
}