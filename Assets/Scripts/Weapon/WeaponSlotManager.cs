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

    [Header("Equipped Weapons (ITEM)")]
    public WeaponItemData[] slots = new WeaponItemData[3];

    public PlayerWeapon ActiveWeapon { get; private set; }

    [Header("Active Slot")]
    public int activeSlotIndex = 0;
    private InputAction weapon1Action;
    private InputAction weapon2Action;
    private InputAction weapon3Action;



    // ----------------------------------------------------
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

    }

    private void OnEnable()
    {
        var gameplay = PlayerInputRouter.Instance
            .inputActions
            .FindActionMap("Gameplay");

        weapon1Action = gameplay.FindAction("Weapon1");
        weapon2Action = gameplay.FindAction("Weapon2");
        weapon3Action = gameplay.FindAction("Weapon3");

        weapon1Action.performed += _ => SwitchSlot(0);
        weapon2Action.performed += _ => SwitchSlot(1);
        weapon3Action.performed += _ => SwitchSlot(2);
    }


    private void OnDisable()
    {
        if (weapon1Action != null)
            weapon1Action.performed -= _ => SwitchSlot(0);

        if (weapon2Action != null)
            weapon2Action.performed -= _ => SwitchSlot(1);

        if (weapon3Action != null)
            weapon3Action.performed -= _ => SwitchSlot(2);
    }

    public void SetWeaponToSlot(int slot, WeaponItemData weapon)
    {
        if (slot < 0 || slot >= slots.Length)
            return;

        slots[slot] = weapon;

        if (slot == activeSlotIndex)
            SwitchSlot(slot);
    }


    // ----------------------------------------------------
    // SLOT BELİRLEME
    // ----------------------------------------------------
    public WeaponSlotType GetSlotForWeapon(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                return WeaponSlotType.Pistol;

            case WeaponType.MachineGun:
            case WeaponType.Shotgun:
            case WeaponType.Sniper:
            case WeaponType.Bow:
                return WeaponSlotType.Rifle;

            case WeaponType.ThrowingSpear:
            case WeaponType.MeeleSword:
            case WeaponType.MeeleSpear:
                return WeaponSlotType.Melee;

            default:
                return WeaponSlotType.Rifle;
        }
    }

    // ----------------------------------------------------
    // DIŞ API (UI / Craft / Caravan)
    // ----------------------------------------------------
    public WeaponItemData GetWeaponItemInSlot(int slot)
    {
        return slots[slot];
    }

    public void EquipWeaponInSlot(WeaponItemData item, int slot)
    {
        slots[slot] = item;

        if (activeSlotIndex == slot)
            ApplyToHandler(slot);
    }

    public void EquipWeapon(WeaponItemData item)
    {
        int slot = (int)GetSlotForWeapon(item.weaponType);
        EquipWeaponInSlot(item, slot);
        SwitchSlot(slot);
    }

    // ----------------------------------------------------
    // SLOT DEĞİŞTİR
    // ----------------------------------------------------
    public void SwitchSlot(int newSlot)
    {
        if (newSlot < 0 || newSlot > 2)
            return;

        activeSlotIndex = newSlot;
        ApplyToHandler(newSlot);
    }

    // ----------------------------------------------------
    // HANDLER AKTARIMI
    // ----------------------------------------------------
    private void ApplyToHandler(int slot)
    {
        pistolHandler.gameObject.SetActive(false);
        rifleHandler.gameObject.SetActive(false);
        meleeHandler.gameObject.SetActive(false);

        PlayerWeapon handler = GetHandler(slot);
        if (handler == null)
            return;

        handler.gameObject.SetActive(true);
        ActiveWeapon = handler;

        WeaponItemData item = slots[slot];
        // 🔥 ÖNCE silahı yükle
        if (item != null && item.weaponDefinition != null)
        {
            handler.SetWeapon(item.weaponDefinition);
        }

        // 🔒 SONRA input / state hazır olsun
        handler.enabled = true;
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

    // ----------------------------------------------------
    // SAVE
    // ----------------------------------------------------
    public WeaponSlotSaveData GetSaveData()
    {
        WeaponSlotSaveData data = new WeaponSlotSaveData();

        for (int i = 0; i < 3; i++)
            data.equippedWeaponIDs[i] = slots[i] != null ? slots[i].itemID : "";

        data.activeSlotIndex = activeSlotIndex;
        return data;
    }

    // ----------------------------------------------------
    // LOAD
    // ----------------------------------------------------
    public void LoadData(WeaponSlotSaveData data)
    {
        for (int i = 0; i < 3; i++)
        {
            string id = data.equippedWeaponIDs[i];

            if (!string.IsNullOrEmpty(id))
                slots[i] = ItemDatabase.Get(id) as WeaponItemData;
            else
                slots[i] = null;
        }

        activeSlotIndex = Mathf.Clamp(data.activeSlotIndex, 0, 2);
        SwitchSlot(activeSlotIndex);
    }
}
