using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Pozisyon
    public float posX;
    public float posY;
    public float posZ;

    // Statlar
    public int currentHealth;
    public int maxHealth;
    public float currentStamina;
    public float maxStamina;
    public int currentHunger;
    public int maxHunger;
    public int gold;

    // Envanter
    public List<InventoryItemData> inventory = new List<InventoryItemData>();

    // Craft ile açılmış silah ID'leri
    public List<string> unlockedWeaponIDs = new List<string>();
}

[Serializable]
public class InventoryItemData
{
    public string itemID;
    public int amount;
}
