using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Position
    public float posX, posY, posZ;

    // Stats
    public int currentHealth, maxHealth;
    public float currentStamina, maxStamina;
    public int currentHunger, maxHunger;
    public int gold;

    // Inventory items
    public List<InventoryItemData> inventory = new List<InventoryItemData>();

    // Ammo Pool (JsonUtility uyumlu)
    public List<string> ammoTypeIDs = new List<string>();
    public List<int> ammoAmounts = new List<int>();

    // Weapons
    public string[] equippedWeaponIDs = new string[3];
    public int activeSlotIndex;

    // Equipped magazine
    public int equippedMagazineSlotIndex = -1;
    public int equippedMagazineAmmo = 0;

    // Craft
    public List<string> unlockedWeaponIDs = new List<string>();

    // Caravan
    public CaravanSaveData caravanSave;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemID;
    public int amount;
    public bool hasMagazineInstance;
    public int magazineCurrentAmmo;

}

[System.Serializable]
public class AmmoSaveData
{
    public string ammoId;
    public int amount;
}

[System.Serializable]
public class MagazineSaveData
{
    public string magazineItemID; // MagazineData.itemID
    public int currentAmmo;
}