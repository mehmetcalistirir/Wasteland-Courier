using System;
using System.Collections.Generic;


[System.Serializable]
public class SaveData
{
    public float posX, posY, posZ;

    public int currentHealth;
    public int maxHealth;

    public float currentStamina;
    public float maxStamina;

    public int currentHunger;
    public int maxHunger;

    public int gold;

    public System.Collections.Generic.List<InventoryItemData> inventory = new System.Collections.Generic.List<InventoryItemData>();
    public System.Collections.Generic.List<string> unlockedWeaponIDs = new System.Collections.Generic.List<string>();

    // 🔽 Yeni eklenenler:
    public string[] equippedWeaponKeys; // slot 0–1–2 için weapon key (weaponName)
    public int[] slotClip;             // her slotun şarjör mermisi
    public int[] slotReserve;          // her slotun reserve mermisi
    public int activeSlotIndex;        // o anda seçili slot
}



[System.Serializable]
public class InventoryItemData
{
    public string itemID;
    public int amount;
}

[Serializable]
public class SaveWeaponSlotData
{
    public string[] equippedWeaponIDs = new string[3];
    public int[] clip = new int[3];
    public int[] reserve = new int[3];
}
