using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using WeaponInstance = CaravanInventory.WeaponInstance; // Adım 0

public class WeaponSlotManager : MonoBehaviour
{
    [Header("Weapon Objects")]
    [Tooltip("Sahnedeki silah GameObject'lerini buraya sırayla atayın (0: Makineli, 1: Tabanca, 2: Kılıç).")]
    public GameObject[] weaponSlots;

    private bool ammoInitialized = false;

    [Header("Spear Blueprints")]
    public WeaponBlueprint spearThrowBlueprint; // Fırlatma mızrağı
    public WeaponBlueprint spearMeleeBlueprint; // Yakın dövüş mızrağı
    private int spearSlotIndex = 4;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reloadPromptText;

    private bool[] unlockedWeapons;
    private WeaponBlueprint[] equippedBlueprints;

    [Header("Starting Equipment")]
    public List<WeaponBlueprint> startingEquippedWeapons;

    public static WeaponSlotManager Instance { get; private set; }
    public PlayerWeapon activeWeapon;
    public int activeSlotIndex = -1;

    private bool emptyClipSoundPlayedThisPress = false;

    [Header("Slots")]
    public WeaponInstance[] slots;  // sende zaten varsa bunu kullan
    public int activeIndex = 0;     // aktif slot indeksi

    // Instance -> sahnedeki silah GameObject eşlemesi
    private readonly Dictionary<WeaponInstance, GameObject> goByInstance = new();

    // === Yardımcılar ===
    public bool IsActiveSlotEmpty() => slots == null || slots.Length == 0 || slots[activeIndex] == null;

    // --- Slot-bazlı mermi durumu: TEK KAYNAK ---
    private int[] ammoInClips;
    private int[] totalReserveAmmo;

    public WeaponBlueprint[] GetEquippedBlueprints() => equippedBlueprints;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }

        unlockedWeapons = new bool[weaponSlots.Length];

        equippedBlueprints = new WeaponBlueprint[weaponSlots.Length];
        foreach (var blueprint in startingEquippedWeapons)
        {
            if (blueprint == null) continue;
            int slotIndex = blueprint.weaponSlotIndexToUnlock;
            if (blueprint == spearThrowBlueprint || blueprint == spearMeleeBlueprint) slotIndex = spearSlotIndex;
            if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
            {
                equippedBlueprints[slotIndex] = blueprint;
                Debug.Log($"Başlangıçta Kuşanıldı: {blueprint.weaponName} -> Slot {slotIndex}");
            }
        }

        if (spearThrowBlueprint != null &&
            spearSlotIndex >= 0 && spearSlotIndex < equippedBlueprints.Length)
        {
            equippedBlueprints[spearSlotIndex] = spearThrowBlueprint;
        }

        InitializeAmmo(); // sadece ilk dolum
    }

    public void EquipIntoActive(WeaponInstance inst)
    {
        // Burada sadece "hangi instance aktif" bilgisini tutuyoruz.
        // GO ile eşleştirme RegisterEquippedGO'da yapılacak.
        slots[activeIndex] = inst;

        // UI/ikon güncellemen varsa burada çağır.
        // UpdateHotbarUI();
    }

    /// <summary> Silah sahneye konunca bu metodu ÇAĞIR. Durability event'ini bağlar. </summary>
    public void RegisterEquippedGO(WeaponInstance inst, GameObject go)
    {
        if (inst == null || go == null) return;

        goByInstance[inst] = go;

        var d = go.GetComponent<WeaponDurability>();
        if (d != null)
        {
            // Çifte kayıt olmasın:
            d.onBroken.RemoveAllListeners();
            d.onBroken.AddListener(() => OnWeaponBroken(inst));
        }
    }

      /// <summary> Silah kırılınca slotu boşalt ve eşleşmeyi temizle. </summary>
    private void OnWeaponBroken(WeaponInstance inst)
    {
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == inst)
                {
                    slots[i] = null;           // slot artık boş
                    if (i == activeIndex)
                    {
                        // İstersen burada "elde silah yok" anim/ikon temizliği yap.
                        // ClearEquippedVisuals();
                    }
                    break;
                }
            }
        }

        goByInstance.Remove(inst);
    }

    void Start()
    {
        foreach (var slot in weaponSlots)
            if (slot != null) slot.SetActive(false);

        SwitchToSlot(0);
    }

    public void HandleWeaponBroken(GameObject brokenGO)
{
    // Bu GO hangi slottaki fiziksel silah?
    int idx = System.Array.IndexOf(weaponSlots, brokenGO);
    if (idx < 0) return;

    // Blueprint’i ve mermiyi sıfırla -> slot gerçekten BOŞ kabul edilsin
    if (equippedBlueprints != null && idx < equippedBlueprints.Length)
        equippedBlueprints[idx] = null;

    if (ammoInClips != null && idx < ammoInClips.Length)      ammoInClips[idx] = 0;
    if (totalReserveAmmo != null && idx < totalReserveAmmo.Length) totalReserveAmmo[idx] = 0;

    // Silah GO’sunu gizle (senin mimaride slot GO’ları kalıcı)
    if (weaponSlots[idx] != null) weaponSlots[idx].SetActive(false);

    // Aktif slotsa UI’ı temizle
    if (idx == activeSlotIndex)
    {
        activeWeapon = null;
        UpdateUI();                    // mevcut metodun
        WeaponSlotUI.Instance?.UpdateHighlight(activeSlotIndex);
    }

    Debug.Log($"[WeaponBroken] Slot {idx} boşaltıldı.");
}


    void Update()
    {
        HandleWeaponSwitchingInput();

        if (activeWeapon == null) return;

        if (Keyboard.current.xKey.wasPressedThisFrame)
            HandleSpearKey();

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            emptyClipSoundPlayedThisPress = false;

        HandleShootingInput();
        HandleReloadInput();
        UpdateUI();
    }

    // --- Blueprint erişimleri ---
    public WeaponBlueprint GetBlueprintForSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
            return equippedBlueprints[slotIndex];
        return null;
    }

    public void EquipBlueprint(WeaponBlueprint blueprintToEquip)
{
    if (blueprintToEquip == null) return;

    int slotIndex = blueprintToEquip.weaponSlotIndexToUnlock;
    if (blueprintToEquip == spearThrowBlueprint || blueprintToEquip == spearMeleeBlueprint)
        slotIndex = spearSlotIndex;

    if (slotIndex < 0 || slotIndex >= equippedBlueprints.Length) return;

    // Doğru dizi: equippedBlueprints
    equippedBlueprints[slotIndex] = blueprintToEquip;

    // Slot ikonunu güncelle
    WeaponSlotUI.Instance?.RefreshIconForSlot(slotIndex);

    // Eğer aktif slot ise, sadece data’yı uygula; FULL yapma
    if (activeSlotIndex == slotIndex)
        ApplyEquippedBlueprintToActiveSlot();
}


    // Aktif slottaki canlı şarjörü state'e yaz
    public void CaptureActiveClipAmmo()
    {
        if (activeSlotIndex >= 0 &&
            activeWeapon != null &&
            activeWeapon.weaponData != null &&
            activeWeapon.weaponData.clipSize > 0)
        {
            ammoInClips[activeSlotIndex] = activeWeapon.GetCurrentAmmoInClip();
        }
    }

    // State'i silaha tekrar uygula (reset YOK)
    public void ForceReapplyActiveAmmo()
    {
        if (activeSlotIndex < 0 || activeWeapon == null) return;
        activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);
        UpdateUI();
        Debug.Log($"[ForceApply] slot {activeSlotIndex} -> {activeWeapon.weaponData.weaponName} clip:{ammoInClips[activeSlotIndex]} reserve:{totalReserveAmmo[activeSlotIndex]}");
    }

    // --- Mermi durum yardımcıları ---
    public (int clip, int reserve) GetAmmoStateForSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= ammoInClips.Length) return (0, 0);

        // Aktif slotun canlı değerini önce senkle
        if (slotIndex == activeSlotIndex && activeWeapon != null && activeWeapon.weaponData.clipSize > 0)
            ammoInClips[slotIndex] = activeWeapon.GetCurrentAmmoInClip();

        return (ammoInClips[slotIndex], totalReserveAmmo[slotIndex]);
    }

    public void SetAmmoStateForSlot(int slotIndex, int clip, int reserve)
    {
        if (slotIndex < 0 || slotIndex >= ammoInClips.Length) return;

        ammoInClips[slotIndex]     = Mathf.Max(0, clip);
        totalReserveAmmo[slotIndex] = Mathf.Max(0, reserve);

        if (slotIndex == activeSlotIndex && activeWeapon != null && activeWeapon.weaponData.clipSize > 0)
        {
            activeWeapon.SetAmmoInClip(ammoInClips[slotIndex]);
            UpdateUI();
        }
    }

    public void EquipBlueprintIntoActiveSlot(WeaponBlueprint bp)
    {
        if (activeSlotIndex < 0) { Debug.LogWarning("Aktif slot yok."); return; }

        if (activeWeapon != null && activeWeapon.IsReloading())
            activeWeapon.StopAllCoroutines();

        // Doğru dizi: equippedBlueprints
        equippedBlueprints[activeSlotIndex] = bp;

        // Sadece weaponData'yı değiştir ve mevcut state'i uygula (FULL yok)
        ApplyEquippedBlueprintToActiveSlot();

        if (activeWeapon != null && activeWeapon.weaponData.clipSize > 0)
            activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);

        UpdateUI();
        WeaponSlotUI.Instance?.RefreshIconForSlot(activeSlotIndex);
        WeaponSlotUI.Instance?.UpdateHighlight(activeSlotIndex);

        if (activeSlotIndex < 0) return;

        bool wasEmpty = (GetBlueprintForSlot(activeSlotIndex) == null);
        equippedBlueprints[activeSlotIndex] = bp;

        ApplyEquippedBlueprintToActiveSlot();

        if (wasEmpty) // sadece boş slota yeni silah takıldıysa full durability
        {
            var go = weaponSlots[activeSlotIndex];
            var wd = go ? go.GetComponent<WeaponDurability>() : null;
            if (wd != null) wd.RefillToMax();
        }
    }

// Rezerv mermi ekle (tek slot)
public void AddReserveAmmoToSlot(int slotIndex, int amount, bool clampToMax = false)
{
    if (totalReserveAmmo == null || slotIndex < 0 || slotIndex >= totalReserveAmmo.Length) return;

    // O slotta silah yoksa eklemeyelim (istersen burada bir genel havuz mantığı kurabilirsin)
    if (GetBlueprintForSlot(slotIndex) == null)
    {
        Debug.Log("[AmmoPickup] Bu slota takılı silah yok, ammo eklenmedi.");
        return;
    }

    int before = totalReserveAmmo[slotIndex];
    totalReserveAmmo[slotIndex] = Mathf.Max(0, before + amount);

    if (clampToMax)
    {
        int cap = GetBlueprintForSlot(slotIndex)?.weaponData?.maxAmmoCapacity ?? int.MaxValue;
        totalReserveAmmo[slotIndex] = Mathf.Min(totalReserveAmmo[slotIndex], cap);
    }

    if (slotIndex == activeSlotIndex) UpdateUI();
    Debug.Log($"[AmmoPickup] slot {slotIndex} reserve: {before} -> {totalReserveAmmo[slotIndex]} (+{amount})");
}

// Aktif slota kısayol
public void AddReserveAmmoToActive(int amount, bool clampToMax = false)
{
    AddReserveAmmoToSlot(activeSlotIndex, amount, clampToMax);
}


    public void RefillDurabilityForSlot(int slotIndex)
    {
        if (weaponSlots == null || slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        var go = weaponSlots[slotIndex];
        if (go == null) return;

        var wd = go.GetComponent<WeaponDurability>();
        if (wd != null) wd.RefillToMax();
    }


    private void SaveActiveClipAmmo()
    {
        if (activeWeapon != null && activeWeapon.weaponData != null && activeWeapon.weaponData.clipSize > 0)
            ammoInClips[activeSlotIndex] = activeWeapon.GetCurrentAmmoInClip();
    }

    private void ToggleSpearInActiveSlot()
    {
        if (activeSlotIndex != spearSlotIndex) return;

        var current = equippedBlueprints[spearSlotIndex];
        if (current == null) return;

        if (current == spearThrowBlueprint)
            equippedBlueprints[spearSlotIndex] = spearMeleeBlueprint;
        else if (current == spearMeleeBlueprint)
            equippedBlueprints[spearSlotIndex] = spearThrowBlueprint;
        else
            return;

        ApplyEquippedBlueprintToActiveSlot();
    }

    public void UnlockWeapon(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
        {
            if (!unlockedWeapons[slotIndex])
            {
                unlockedWeapons[slotIndex] = true;
                Debug.Log($"<color=lime>SİLAH KİLİDİ AÇILDI:</color> {weaponSlots[slotIndex].name}");
                SwitchToSlot(slotIndex);
            }
        }
        else
        {
            Debug.LogError($"Geçersiz slot index ({slotIndex})!");
        }
    }

    public bool IsWeaponUnlocked(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
            return unlockedWeapons[slotIndex];
        return false;
    }

    // --- SADECE İLK DOLUM: sonradan yazılan state'i ezme ---
    private void InitializeAmmo()
{
    if (ammoInitialized) return;   // ✅ tekrar çalışmasın

    ammoInClips      = new int[weaponSlots.Length];
    totalReserveAmmo = new int[weaponSlots.Length];

    for (int i = 0; i < weaponSlots.Length; i++)
    {
        if (equippedBlueprints[i] == null) continue;
        var data = equippedBlueprints[i].weaponData;
        if (data == null || data.clipSize <= 0) continue;

        ammoInClips[i]      = data.clipSize;
        totalReserveAmmo[i] = data.maxAmmoCapacity;
    }

    ammoInitialized = true;
    Debug.Log("Mermi sistemi başlangıç değerleri yüklendi (tek sefer).");
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
        if (weaponSlots[newIndex] == null) { Debug.LogError($"Fiziksel silah slotu {newIndex} boş!"); return; }
        if (equippedBlueprints[newIndex] == null) { Debug.Log($"Slot {newIndex} boş."); return; }

        // Eski aktif silahın canlı şarjörünü kaydet
        if (activeSlotIndex != -1 && activeWeapon != null)
        {
            if (activeWeapon.weaponData.clipSize > 0)
            {
                ammoInClips[activeSlotIndex] = activeWeapon.GetCurrentAmmoInClip();
                Debug.Log($"{activeWeapon.weaponData.weaponName} (Slot {activeSlotIndex}) mermisi kaydedildi: {ammoInClips[activeSlotIndex]}");
            }

            if (activeWeapon.IsReloading())
                activeWeapon.StopAllCoroutines();

            weaponSlots[activeSlotIndex].SetActive(false);
        }

        activeSlotIndex = newIndex;
        GameObject newWeaponObject = weaponSlots[activeSlotIndex];
        newWeaponObject.SetActive(true);
        activeWeapon = newWeaponObject.GetComponent<PlayerWeapon>();

        if (activeWeapon != null)
        {
            activeWeapon.weaponData = equippedBlueprints[activeSlotIndex].weaponData;

            if (activeWeapon.weaponData.clipSize > 0)
                activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]); // state'ten uygula

            Debug.Log($"Başarıyla '{activeWeapon.weaponData.weaponName}' silahına geçildi.");
            UpdateUI();
            WeaponSlotUI.Instance?.UpdateHighlight(activeSlotIndex);
        }
    }

    // Aktif slottaki blueprint değiştiğinde, objeyi kapatıp açmadan verileri uygula.
    private void ApplyEquippedBlueprintToActiveSlot()
    {
        if (activeSlotIndex < 0 || activeWeapon == null) return;
        var bp = equippedBlueprints[activeSlotIndex];
        if (bp == null) return;

        // Reload açıksa iptal (reload şarjörü doldurur)
        if (activeWeapon.IsReloading())
            activeWeapon.StopAllCoroutines();

        // Sadece weaponData'yı değiştir (FULL YOK)
        activeWeapon.weaponData = bp.weaponData;

        // Slot state'ini aynen uygula
        if (activeWeapon.weaponData != null && activeWeapon.weaponData.clipSize > 0)
            activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);

        UpdateUI();
        WeaponSlotUI.Instance?.UpdateHighlight(activeSlotIndex);
    }

    // --- Ateş / Reload / UI ---
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
            else
            {
                if (!emptyClipSoundPlayedThisPress)
                {
                    activeWeapon.PlayEmptyClipSound();
                    emptyClipSoundPlayedThisPress = true;
                    StartReload();
                }

                if (activeWeapon.weaponData.weaponName.Contains("ThrowingSpear"))
                {
                    equippedBlueprints[activeSlotIndex] = spearMeleeBlueprint;
                    ApplyEquippedBlueprintToActiveSlot();
                }
            }
        }
    }

    private void HandleReloadInput()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            StartReload();
    }

    public void StartReload()
    {
        if (activeWeapon.weaponData.clipSize <= 0) return;

        if (!activeWeapon.IsReloading() && totalReserveAmmo[activeSlotIndex] > 0 &&
            activeWeapon.GetCurrentAmmoInClip() < activeWeapon.weaponData.clipSize)
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

        // Dizileri güncel tutmak için
        ammoInClips[activeSlotIndex] = activeWeapon.GetCurrentAmmoInClip();
        UpdateUI();
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
            if (activeWeapon.weaponData.clipSize <= 0)
                ammoText.text = "";
            else
                ammoText.text = $"{activeWeapon.GetCurrentAmmoInClip()} / {totalReserveAmmo[activeSlotIndex]}";
        }
    }

    private void UpdateReloadPrompt()
    {
        if (activeWeapon == null || reloadPromptText == null) return;

        if (activeWeapon.IsReloading())
        {
            reloadPromptText.text = "Reloading...";
            reloadPromptText.gameObject.SetActive(true);
        }
        else if (activeWeapon.GetCurrentAmmoInClip() <= 0 && totalReserveAmmo[activeSlotIndex] > 0)
        {
            reloadPromptText.text = "Press 'R' to Reload";
            reloadPromptText.gameObject.SetActive(true);
        }
        else
        {
            reloadPromptText.gameObject.SetActive(false);
        }
    }

    private void HandleSpearKey()
    {
        if (activeSlotIndex != spearSlotIndex) return;

        SaveActiveClipAmmo();

        var current = equippedBlueprints[spearSlotIndex];
        if (current == null) return;

        if (current == spearThrowBlueprint)
            equippedBlueprints[spearSlotIndex] = spearMeleeBlueprint;
        else if (current == spearMeleeBlueprint)
            equippedBlueprints[spearSlotIndex] = spearThrowBlueprint;
        else
            return;

        ApplyEquippedBlueprintToActiveSlot();
    }
}
