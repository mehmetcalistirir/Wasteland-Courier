// WeaponSlotManager.cs (YENİ, SAĞLAM VE TEMİZ HALİ)

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class WeaponSlotManager : MonoBehaviour
{
    // --- INSPECTOR'DA AYARLANACAK ALANLAR ---
    [Header("Weapon Objects")]
    [Tooltip("Sahnedeki silah GameObject'lerini buraya sırayla atayın (0: Makineli, 1: Tabanca, 2: Kılıç).")]
    public GameObject[] weaponSlots;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reloadPromptText;

    private bool[] unlockedWeapons;

    private WeaponBlueprint[] equippedBlueprints;

    [Header("Starting Equipment")]
    public List<WeaponBlueprint> startingEquippedWeapons;


    // --- SİSTEM DEĞİŞKENLERİ ---
    public static WeaponSlotManager Instance { get; private set; }
    private PlayerWeapon activeWeapon;
    public int activeSlotIndex = -1; // -1, başlangıçta hiçbir silahın aktif olmadığını belirtir.

    private bool emptyClipSoundPlayedThisPress = false;

    // Mermi Yönetimi
    private int[] ammoInClips;
    private int[] totalReserveAmmo;

    // WeaponSlotManager.cs içindeki Awake fonksiyonu

// WeaponSlotManager.cs İÇİNE BU FONKSİYONU EKLEYİN

public WeaponBlueprint[] GetEquippedBlueprints()
{
    return equippedBlueprints;
}
    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);

        // 1. Kuşanılmış blueprint dizisini oluştur ve başlangıç silahlarını doldur.
        equippedBlueprints = new WeaponBlueprint[weaponSlots.Length];
        foreach (var blueprint in startingEquippedWeapons)
        {
            if (blueprint != null)
            {
                int slotIndex = blueprint.weaponSlotIndexToUnlock;
                if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
                {
                    equippedBlueprints[slotIndex] = blueprint;
                    Debug.Log($"Başlangıçta Kuşanıldı: {blueprint.weaponName} -> Slot {slotIndex}");
                }
            }
        }

        // 2. Mermi sistemini başlat.
        InitializeAmmo();
        
        
    }

    public WeaponBlueprint GetBlueprintForSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
        {
            return equippedBlueprints[slotIndex];
        }
        return null;
    }

    public void EquipBlueprint(WeaponBlueprint blueprintToEquip)
    {
        if (blueprintToEquip == null) return;
        
        int slotIndex = blueprintToEquip.weaponSlotIndexToUnlock;
        if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
        {
            equippedBlueprints[slotIndex] = blueprintToEquip;
            Debug.Log($"{blueprintToEquip.weaponName}, Slot {slotIndex}'e kuşandırıldı.");

            // Eğer o an o slot aktifse, değişikliği anında yansıt.
            if (activeSlotIndex == slotIndex)
            {
                SwitchToSlot(slotIndex);
            }
        }
    }



    public bool IsWeaponEquipped(WeaponBlueprint blueprint)
    {
        int slotIndex = blueprint.weaponSlotIndexToUnlock;
        return equippedBlueprints[slotIndex] == blueprint;
    }

    public void UnlockWeapon(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
        {
            if (!unlockedWeapons[slotIndex])
            {
                unlockedWeapons[slotIndex] = true;
                Debug.Log($"<color=lime>SİLAH KİLİDİ AÇILDI:</color> {weaponSlots[slotIndex].name} artık kullanılabilir!");

                // İsteğe bağlı: Kilidi açılan silaha otomatik geçiş yap
                SwitchToSlot(slotIndex);
            }
        }
        else
        {
            Debug.LogError($"Geçersiz bir silah slot index'i ({slotIndex}) kilidi açılmaya çalışıldı!");
        }
    }
    // WeaponSlotManager.cs'in içine

    public bool IsWeaponUnlocked(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
        {
            return unlockedWeapons[slotIndex];
        }
        return false;
    }

    void Start()
    {
        // Başlangıçta tüm silahların kapalı olduğundan emin olalım.
        foreach (var slot in weaponSlots)
        {
            if (slot != null) slot.SetActive(false);
        }

        // Başlangıçta 1. slottaki (Handgun) silahı seç.
        SwitchToSlot(0);
    }

    void Update()
    {
       HandleWeaponSwitchingInput();

        if (activeWeapon == null) return;
        
        // Fare bırakıldığında "boş şarjör sesi çalındı" bayrağını sıfırla.
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            emptyClipSoundPlayedThisPress = false;
        }

        HandleShootingInput();
        HandleReloadInput();
        UpdateUI();
    }

    private void InitializeAmmo()
    {
        ammoInClips = new int[weaponSlots.Length];
        totalReserveAmmo = new int[weaponSlots.Length];

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            // Mermi ve cephaneyi, kuşanılmış blueprint'e göre doldur.
            if (equippedBlueprints[i] != null)
            {
                WeaponData data = equippedBlueprints[i].weaponData;
                if (data.clipSize > 0)
                {
                    ammoInClips[i] = data.clipSize;
                    totalReserveAmmo[i] = data.maxAmmoCapacity;
                }
            }
        }
        Debug.Log("Mermi sistemi başlangıç değerleri yüklendi.");
    }

    private void HandleWeaponSwitchingInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchToSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchToSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchToSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchToSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SwitchToSlot(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) SwitchToSlot(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) SwitchToSlot(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) SwitchToSlot(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) SwitchToSlot(8);
}

    public void SwitchToSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weaponSlots.Length || newIndex == activeSlotIndex) return;
        if (weaponSlots[newIndex] == null)
        {
            Debug.LogError($"Fiziksel silah slotu {newIndex} boş!");
            return;
        }

        // Ana kontrol: Bu slotta kuşanılmış bir silah var mı?
        if (equippedBlueprints[newIndex] == null)
        {
            Debug.Log($"Slot {newIndex} boş, silah değiştirilemez.");
            return;
        }

        // Mevcut silahı kapat.
        if (activeSlotIndex != -1 && weaponSlots[activeSlotIndex] != null)
        {
            weaponSlots[activeSlotIndex].SetActive(false);
        }

        // Yeni silahı aç ve ayarla.
        activeSlotIndex = newIndex;
        GameObject newWeaponObject = weaponSlots[activeSlotIndex];
        newWeaponObject.SetActive(true);
        activeWeapon = newWeaponObject.GetComponent<PlayerWeapon>();

        if (activeWeapon != null)
        {
            // Fiziksel silahın verilerini, kuşanılmış blueprint'in verileriyle güncelle.
            activeWeapon.weaponData = equippedBlueprints[activeSlotIndex].weaponData;
            
            // Yeni silahın mermi durumunu yükle.
            if (activeWeapon.weaponData.clipSize > 0)
            {
                activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);
            }

            Debug.Log($"Başarıyla '{activeWeapon.weaponData.weaponName}' silahına geçildi.");
            UpdateUI(); // UI'ı anında güncelle.
            
            if (WeaponSlotUI.Instance != null)
            {
                WeaponSlotUI.Instance.UpdateHighlight(activeSlotIndex);
            }
        }
    }

    // --- Ateş Etme, Şarjör Değiştirme ve UI Fonksiyonları ---
    // Bu kısımlar önceki versiyonlarla büyük ölçüde aynı kalabilir.

    private void HandleShootingInput()
{
    if (activeWeapon == null || Mouse.current == null) return;

        bool isAutomatic = activeWeapon.weaponData.isAutomatic;
        bool isShootingPressed = isAutomatic ? Mouse.current.leftButton.isPressed : Mouse.current.leftButton.wasPressedThisFrame;

        if (isShootingPressed)
        {
            if (activeWeapon.GetCurrentAmmoInClip() > 0)
            {
                emptyClipSoundPlayedThisPress = false;
                activeWeapon.Shoot();
            }
            else // Mermi yoksa...
            {
                if (!emptyClipSoundPlayedThisPress)
                {
                    activeWeapon.PlayEmptyClipSound();
                    emptyClipSoundPlayedThisPress = true;
                    StartReload(); // Otomatik reload'u sadece bir kez dene.
                }
            }
        }
}


    private void HandleReloadInput()
{
    if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
    {
        StartReload();
    }
}

    public void StartReload()
    {
        // Kılıç gibi silahların şarjörü değiştirilemez.
        if (activeWeapon.weaponData.clipSize <= 0) return;

        if (!activeWeapon.IsReloading() && totalReserveAmmo[activeSlotIndex] > 0 && activeWeapon.GetCurrentAmmoInClip() < activeWeapon.weaponData.clipSize)
        {
            activeWeapon.StartCoroutine(activeWeapon.Reload());
        }
    }

    public void FinishReload()
    {
        if (activeWeapon == null || activeWeapon.weaponData.clipSize <= 0) return;

        int clipSize = activeWeapon.weaponData.clipSize;
        int currentAmmo = activeWeapon.GetCurrentAmmoInClip();
        int ammoNeeded = clipSize - currentAmmo;

        int ammoToTransfer = Mathf.Min(ammoNeeded, totalReserveAmmo[activeSlotIndex]);

        activeWeapon.SetAmmoInClip(currentAmmo + ammoToTransfer);
        totalReserveAmmo[activeSlotIndex] -= ammoToTransfer;
    }

    private void UpdateUI()
    {
        UpdateAmmoText();
        UpdateReloadPrompt();
    }

    public void UpdateAmmoText()
    {
        if (activeWeapon != null && ammoText != null)
        {
            // Mermisi olmayan silahlar (kılıç gibi) için UI'ı gizle
            if (activeWeapon.weaponData.clipSize <= 0)
            {
                ammoText.text = "";
            }
            else
            {
                ammoText.text = $"{activeWeapon.GetCurrentAmmoInClip()} / {totalReserveAmmo[activeSlotIndex]}";
            }
        }
    }

   private void UpdateReloadPrompt()
{
    // Aktif silah, UI text ve PlayerWeapon script'i var mı?
    if (activeWeapon == null || reloadPromptText == null) return;

    // 1. Durum: Şarjör değiştiriliyor mu?
    if (activeWeapon.IsReloading())
    {
        reloadPromptText.text = "Reloading...";
        reloadPromptText.gameObject.SetActive(true);
    }
    // 2. Durum: Mermi bitti ve şarjör değiştirilebilir mi?
    else if (activeWeapon.GetCurrentAmmoInClip() <= 0 && totalReserveAmmo[activeSlotIndex] > 0)
    {
        reloadPromptText.text = "Press 'R' to Reload";
        reloadPromptText.gameObject.SetActive(true);
    }
    // 3. Durum: Diğer tüm durumlar
    else
    {
        reloadPromptText.gameObject.SetActive(false);
    }
}
}