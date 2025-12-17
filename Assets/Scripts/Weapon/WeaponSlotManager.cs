using UnityEngine;
using UnityEngine.InputSystem;



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
    public PlayerWeapon ActiveWeapon { get; private set; }


    [Header("Active Slot")]
    public int activeSlotIndex = 0;
    private PlayerControls controls;




    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        controls = new PlayerControls(); // 🔥 MapToggle ile birebir


    }
private void OnEnable()
{
    controls.Gameplay.Weapon1.performed += OnWeapon1;
    controls.Gameplay.Weapon2.performed += OnWeapon2;
    controls.Gameplay.Weapon3.performed += OnWeapon3;

    controls.Gameplay.Enable();
}

private void OnDisable()
{
    controls.Gameplay.Weapon1.performed -= OnWeapon1;
    controls.Gameplay.Weapon2.performed -= OnWeapon2;
    controls.Gameplay.Weapon3.performed -= OnWeapon3;

    controls.Gameplay.Disable();
}

private void OnWeapon1(InputAction.CallbackContext ctx)
{
    SwitchSlot(0);
}

private void OnWeapon2(InputAction.CallbackContext ctx)
{
    SwitchSlot(1);
}

private void OnWeapon3(InputAction.CallbackContext ctx)
{
    SwitchSlot(2);
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
            case WeaponType.MeeleSpear:
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

        // Handler’ı aktif et
        handler.gameObject.SetActive(true);

        // Silahı yükle
        WeaponData weapon = slots[slot];
        if (weapon != null)
        {
            handler.SetWeapon(weapon);

            Debug.Log("Handler açıldı ve silah verildi: " + weapon.name);
        }
        else
        {
            Debug.LogWarning("Slot boş ama handler aktif edildi: " + slot);
        }
    }

    public void SetActiveWeapon(PlayerWeapon weapon)
    {
        ActiveWeapon = weapon;
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


    public void SetAmmo(int slot, int newClip, int newReserve)
    {

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

    public void EquipCraftedWeapon(WeaponData newWeapon)
    {
        int slot = (int)GetSlotForWeapon(newWeapon);

        Debug.Log($"[EquipCraftedWeapon] Yeni silah: {newWeapon.itemName}, Slot: {slot}");

        // 1) Eski silahı karavana gönder
        WeaponData oldWeapon = slots[slot];
        if (oldWeapon != null)
        {
            CaravanInventory.Instance.StoreWeapon(oldWeapon);
            Debug.Log("Eski silah karavana gönderildi: " + oldWeapon.itemName);
        }

        // 2) Yeni silah slota ekle
        slots[slot] = newWeapon;

        // 4) Eğer aktif slot buysa hemen güncelle
        if (activeSlotIndex == slot)
        {
            ApplyToHandler(slot);
        }
        else
        {
            // Craft edilen silaha otomatik geçmek istiyorsan:
            activeSlotIndex = slot;
            ApplyToHandler(slot);
        }

        Debug.Log("Yeni silah oyuncuya takıldı: " + newWeapon.itemName);
    }

    // ----------------------------------------------------
    //  SAVE
    // ----------------------------------------------------
    public WeaponSlotSaveData GetSaveData()
    {
        WeaponSlotSaveData data = new WeaponSlotSaveData();

        for (int i = 0; i < 3; i++)
        {
            if (slots[i] != null)
                data.equippedWeaponIDs[i] = slots[i].itemID;
            else
                data.equippedWeaponIDs[i] = "";

        }

        data.activeSlotIndex = activeSlotIndex;

        return data;
    }


    // ----------------------------------------------------
    //  LOAD
    // ----------------------------------------------------
    public void LoadData(WeaponSlotSaveData data)
    {
        for (int i = 0; i < 3; i++)
        {
            string id = data.equippedWeaponIDs[i];

            if (!string.IsNullOrEmpty(id))
            {
                ItemData item = ItemDatabase.Get(id);
                if (item is WeaponItemData wid)
                {
                    slots[i] = wid.weaponData;

                }
                else
                {
                    slots[i] = null;

                }
            }
            else
            {
                slots[i] = null;

            }
        }

        activeSlotIndex = Mathf.Clamp(data.activeSlotIndex, 0, 2);

        // handler'ı güncelle
        SwitchSlot(activeSlotIndex);
    }



}
