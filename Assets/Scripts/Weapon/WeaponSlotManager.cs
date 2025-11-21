using UnityEngine;

public enum WeaponSlotType
{
    Pistol = 0,
    Rifle = 1,
    Melee = 2
}

public class WeaponSlotManager : MonoBehaviour
{
    public static WeaponSlotManager Instance;

    [Header("Player Weapon Handlers")]
    public PlayerWeapon pistolHandler;
    public PlayerWeapon rifleHandler;
    public PlayerWeapon meleeHandler;

    [Header("Equipped Weapons (WeaponData)")]
    public WeaponData[] slots = new WeaponData[3];

    // Mermi durumu (clip + reserve)
    [Header("Ammo State per Slot")]
    public int[] clip = new int[3];
    public int[] reserve = new int[3];

    [Header("Active Slot")]
    public int activeSlotIndex = 0;



    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Mermileri 0 başlat
        for (int i = 0; i < 3; i++)
        {
            clip[i] = 0;
            reserve[i] = 0;
        }
    }



    // -------------------------------
    // Silah tipine göre slot belirle
    // -------------------------------
    public WeaponSlotType GetSlotForWeapon(WeaponData weapon)
    {
        if (weapon == null)
            return WeaponSlotType.Rifle; // default

        switch (weapon.weaponType)
        {
            // TABANCA SLOTU (0)
            case WeaponType.Pistol:
                return WeaponSlotType.Pistol;

            // TÜFEK / UZUN MENZİL SLOTU (1)
            case WeaponType.MachineGun:
            case WeaponType.Shotgun:
            case WeaponType.Sniper:
            case WeaponType.Bow:
                return WeaponSlotType.Rifle;

            // YAKIN DÖVÜŞ SLOTU (2)
            case WeaponType.ThrowingSpear:
            case WeaponType.MeeleSword:
                return WeaponSlotType.Melee;

            // Tanınmayanlar varsayılan: tüfek slotu
            default:
                return WeaponSlotType.Rifle;
        }
    }


    // -------------------------------
    // Silah kuşan
    // -------------------------------
    public void EquipWeapon(ItemData item)
    {
        WeaponItemData wid = item as WeaponItemData;
        if (wid == null)
        {
            Debug.LogError("EquipWeapon → Bu item bir WeaponItemData değil!");
            return;
        }

        WeaponData weapon = wid.weaponData;
        if (weapon == null)
        {
            Debug.LogError("EquipWeapon → WeaponItemData.weaponData boş!");
            return;
        }

        int slot = (int)GetSlotForWeapon(weapon);
        slots[slot] = weapon;

        clip[slot] = weapon.clipSize;
        reserve[slot] = weapon.maxAmmoCapacity;

        ApplyToHandler(slot);

        Debug.Log($"[WeaponSlotManager] {item.itemName} (WeaponData: {weapon.name}) slot {slot} içine takıldı.");
    }


    // -------------------------------
    // Aktif slota geçiş
    // -------------------------------
    public void SwitchSlot(int newSlot)
    {
        if (newSlot < 0 || newSlot > 2) return;

        activeSlotIndex = newSlot;
        ApplyToHandler(newSlot);
    }

    // -------------------------------
    // PlayerWeapon'a silahı aktar
    // -------------------------------
    private void ApplyToHandler(int slot)
{
    // Tüm handler’ları kapat
    pistolHandler.gameObject.SetActive(false);
    rifleHandler.gameObject.SetActive(false);
    meleeHandler.gameObject.SetActive(false);

    // Yeni handler
    PlayerWeapon handler = GetHandler(slot);
    if (handler == null)
    {
        Debug.LogError("Handler bulunamadı! Slot: " + slot);
        return;
    }

    // 🔥 Kritik Fix: Ateş durmalı
    handler.ResetShootHold();


    // Handler’ı aktif et
    handler.gameObject.SetActive(true);

    // Silahı yükle
    WeaponData weapon = slots[slot];
    if (weapon != null)
    {
        handler.SetWeapon(weapon, clip[slot], reserve[slot]);
        Debug.Log("Handler açıldı ve silah verildi: " + weapon.name);
    }
    else
    {
        Debug.LogWarning("Slot boş ama handler aktif edildi: " + slot);
    }
}





    private PlayerWeapon GetHandler(int slot)
    {
        switch ((WeaponSlotType)slot)
        {
            case WeaponSlotType.Pistol: return pistolHandler;
            case WeaponSlotType.Rifle: return rifleHandler;
            case WeaponSlotType.Melee: return meleeHandler;
        }
        return null;
    }

    // -------------------------------
    // Ammo state
    // -------------------------------
    public (int clip, int reserve) GetAmmo(int slot)
    {
        return (clip[slot], reserve[slot]);
    }

    public void SetAmmo(int slot, int newClip, int newReserve)
    {
        clip[slot] = newClip;
        reserve[slot] = newReserve;

        // aktif slot ise PlayerWeapon’ı güncelle
        if (slot == activeSlotIndex)
        {
            ApplyToHandler(slot);
        }
    }

    // -------------------------------
    // Save/Load için Getter
    // -------------------------------
    public WeaponData GetEquippedWeapon(int slot)
    {
        return slots[slot];
    }
}
