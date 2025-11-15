using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using WeaponInstance = CaravanInventory.WeaponInstance; // Adım 0


public enum SlotItemType
{
    Weapon,
    Bandage
}

public class WeaponSlotManager : MonoBehaviour
{
    [Header("Weapon Objects")]
    [Tooltip("Sahnedeki silah GameObject'lerini buraya sırayla atayın (0: Makineli, 1: Tabanca, 2: Kılıç).")]
    public GameObject[] weaponSlots;
    // Her slotta takılı silahın benzersiz kimliği
    // 🔹 BURAYA EKLE (class'ın İÇİNDE)
    [Header("Bandage Slots")]
    public GenericItemData[] bandageSlots;
    public SlotItemType[] slotTypes;
    private string[] equippedInstanceIds;




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

    [Header("References")]
    public PlayerWeapon playerWeapon;          // Ateşli silah script’in
    public MolotovThrower molotovThrower;      // Molotov script’in
    public WeaponData currentWeapon;           // Şu anda aktif olan silahın verisi

    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;

        // Eğer Molotov ise PlayerWeapon devre dışı kalacak
        bool isMolotov = newWeapon != null && newWeapon.isMolotov;

        if (playerWeapon != null)
            playerWeapon.enabled = !isMolotov;

        if (molotovThrower != null)
            molotovThrower.enabled = isMolotov;

        // 🔥 Molotov aktifse FirePoint’i bağla
        if (isMolotov && molotovThrower.throwPoint == null)
        {
            molotovThrower.throwPoint = playerWeapon.firePoint; // aynı noktayı paylaşabilir
        }

        Debug.Log(isMolotov
            ? "💣 Molotov aktif edildi — PlayerWeapon devre dışı bırakıldı."
            : "🔫 Normal silah aktif — MolotovThrower devre dışı.");
    }

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }

        unlockedWeapons = new bool[weaponSlots.Length];

        equippedBlueprints = new WeaponBlueprint[weaponSlots.Length];

        // Eğer inspector’dan atanmadıysa default olarak 8 slot aç
        if (weaponSlots == null || weaponSlots.Length == 0)
            weaponSlots = new GameObject[8];

        if (equippedBlueprints == null || equippedBlueprints.Length == 0)
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
        bandageSlots = new GenericItemData[weaponSlots.Length];
        slotTypes = new SlotItemType[weaponSlots.Length];

        for (int i = 0; i < slotTypes.Length; i++)
            slotTypes[i] = SlotItemType.Weapon; // default silah slotu

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

    // Verilen payload ile, belirli bir slota silahı takar (FULL yapmaz, payload'ı uygular)

    public void AddAmmoToWeaponType(AmmoItemData ammo)
    {
        for (int i = 0; i < equippedBlueprints.Length; i++)
        {
            var bp = equippedBlueprints[i];
            if (bp == null || bp.weaponData == null) continue;

            // Eşleşme kontrolü
            bool matches = false;
            switch (ammo.resourceType)
            {
                case ResourceType.AmmoPistol:
                    matches = bp.weaponData.weaponType == WeaponType.Pistol;
                    break;
                case ResourceType.AmmoMachineGun:
                    matches = bp.weaponData.weaponType == WeaponType.MachineGun;
                    break;
                case ResourceType.AmmoShotgun:
                    matches = bp.weaponData.weaponType == WeaponType.Shotgun;
                    break;
                case ResourceType.AmmoSniper:
                    matches = bp.weaponData.weaponType == WeaponType.Sniper;
                    break;
            }

            if (matches)
            {
                int before = totalReserveAmmo[i];
                totalReserveAmmo[i] += ammo.ammoPerItem;
                Debug.Log($"🔧 {ammo.itemName} -> {bp.weaponData.weaponName} (Slot {i}) mermisi arttı: {before} -> {totalReserveAmmo[i]}");
                UpdateUI();
                return;
            }
        }

        Debug.Log($"⚠️ {ammo.itemName} için uygun silah bulunamadı, ammo eklenmedi.");
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
        Debug.Log("🚀 WeaponSlotManager.Start() ÇALIŞTI!");

        Transform weaponHolder = transform.Find("WeaponHolder");
        if (weaponHolder == null)
        {
            weaponHolder = new GameObject("WeaponHolder").transform;
            weaponHolder.SetParent(transform);
            weaponHolder.localPosition = Vector3.zero;
        }

        // Eğer equippedBlueprints dolu değilse, startingEquippedWeapons'tan yükle
        if (equippedBlueprints == null || equippedBlueprints.Length == 0)
        {
            if (startingEquippedWeapons != null && startingEquippedWeapons.Count > 0)
            {
                equippedBlueprints = startingEquippedWeapons.ToArray();
                Debug.Log($"📦 {equippedBlueprints.Length} blueprint yüklendi (startingEquippedWeapons).");
            }
            else
            {
                Debug.LogWarning("⚠️ Hiçbir blueprint atanmadı! Lütfen Starting Equipped Weapons kısmını doldur.");
                return;
            }
        }

        weaponSlots = new GameObject[equippedBlueprints.Length];
        ammoInClips = new int[equippedBlueprints.Length];
        totalReserveAmmo = new int[equippedBlueprints.Length];

        for (int i = 0; i < equippedBlueprints.Length; i++)
        {
            var bp = equippedBlueprints[i];
            if (bp == null || bp.weaponData == null)
            {
                Debug.LogWarning($"⚠️ Slot {i} boş veya eksik blueprint.");
                continue;
            }

            var data = bp.weaponData;
            if (data.prefab == null)
            {
                Debug.LogError($"❌ {data.weaponName} için prefab atanmadı!");
                continue;
            }

            // 🔹 Prefab oluştur
            var weaponGO = Instantiate(data.prefab, weaponHolder);
            weaponGO.name = $"Weapon_{data.weaponName}";
            weaponGO.transform.localPosition = Vector3.zero;
            weaponGO.transform.localRotation = Quaternion.identity;
            weaponGO.SetActive(false);

            // 🔹 PlayerWeapon bağlantısı
            var pw = weaponGO.GetComponent<PlayerWeapon>();
            if (pw != null)
            {
                pw.weaponData = data;
                pw.SetAmmoInClip(data.clipSize);
            }

            weaponSlots[i] = weaponGO;
            ammoInClips[i] = data.clipSize;
            totalReserveAmmo[i] = data.maxAmmoCapacity;

            Debug.Log($"✅ {data.weaponName} prefab WeaponHolder altına eklendi.");
        }

        // 🔥 İlk silahı aktif et
        // 🔥 İlk silahı aktif et
        SwitchToSlot(0);

        // 🔁 UI’da ikonları doldur
        WeaponSlotUI.Instance?.RefreshAllFromState();

    }


    public InventoryItem.WeaponInstancePayload BuildPayloadForSlot(int slotIndex)
    {
        var bp = GetBlueprintForSlot(slotIndex);
        if (bp == null) return null;

        var (clip, reserve) = GetAmmoStateForSlot(slotIndex);

        return new InventoryItem.WeaponInstancePayload
        {
            id = System.Guid.NewGuid().ToString("N"),
            clip = clip,
            reserve = reserve,
            durability = 100 // şimdilik
        };
    }

    public void EquipWeaponInstanceIntoSlot(int slotIndex, WeaponBlueprint bp, InventoryItem.WeaponInstancePayload payload)
    {
        if (slotIndex < 0 || bp == null || payload == null) return;

        equippedBlueprints[slotIndex] = bp;

        SetAmmoStateForSlot(slotIndex, payload.clip, payload.reserve);
        SwitchToSlot(slotIndex);

        Debug.Log($"[Slot {slotIndex}] {bp.weaponName} kuşanıldı (clip:{payload.clip}, reserve:{payload.reserve}).");
    }

    public void HandleWeaponBroken(GameObject brokenGO)
    {
        // Bu GO hangi slottaki fiziksel silah?
        int idx = System.Array.IndexOf(weaponSlots, brokenGO);
        if (idx < 0) return;

        // Blueprint’i ve mermiyi sıfırla -> slot gerçekten BOŞ kabul edilsin
        if (equippedBlueprints != null && idx < equippedBlueprints.Length)
            equippedBlueprints[idx] = null;

        if (ammoInClips != null && idx < ammoInClips.Length) ammoInClips[idx] = 0;
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

    public void OnMolotovUsed()
    {
        // Aktif slot Molotov değilse hiçbir şey yapma
        var bp = GetBlueprintForSlot(activeSlotIndex);
        if (bp == null || bp.weaponData == null || !bp.weaponData.isMolotov)
            return;

        Debug.Log("💥 Molotov stoğu bitti! WeaponSlot'tan kaldırılıyor...");

        // Blueprint'i ve prefab referansını temizle
        equippedBlueprints[activeSlotIndex] = null;

        if (weaponSlots[activeSlotIndex] != null)
        {
            Destroy(weaponSlots[activeSlotIndex]);
            weaponSlots[activeSlotIndex] = null;
        }

        // UI güncelle
        WeaponSlotUI.Instance?.RefreshAllFromState();

        // Eğer aktif silah buysa, elindeki silahı da kaldır
        activeWeapon = null;
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
        if (WeaponSlotUI.Instance != null)
        {
            var bp = GetBlueprintForSlot(slotIndex);
            if (bp != null && bp.weaponData != null && bp.weaponData.weaponIcon != null)
                WeaponSlotUI.Instance.SetSlotIcon(slotIndex, bp.weaponData.weaponIcon);
        }


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

        ammoInClips[slotIndex] = Mathf.Max(0, clip);
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
        if (WeaponSlotUI.Instance != null)
        {
            var currentBlueprint = GetBlueprintForSlot(activeSlotIndex);
            if (currentBlueprint != null && currentBlueprint.weaponData != null && currentBlueprint.weaponData.weaponIcon != null)
                WeaponSlotUI.Instance.SetSlotIcon(activeSlotIndex, currentBlueprint.weaponData.weaponIcon);
        }


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

        ammoInClips = new int[weaponSlots.Length];
        totalReserveAmmo = new int[weaponSlots.Length];

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (equippedBlueprints[i] == null) continue;
            var data = equippedBlueprints[i].weaponData;
            if (data == null || data.clipSize <= 0) continue;

            ammoInClips[i] = data.clipSize;
            totalReserveAmmo[i] = data.maxAmmoCapacity;
        }

        ammoInitialized = true;
        Debug.Log("Mermi sistemi başlangıç değerleri yüklendi (tek sefer).");
        equippedInstanceIds = new string[weaponSlots.Length];

    }

    public void RefreshAllWeaponIcons()
    {
        // 🔹 WeaponSlot UI yenile
        if (WeaponSlotUI.Instance != null)
        {
            var bp = GetBlueprintForSlot(activeSlotIndex);
            if (bp != null && bp.weaponData != null && bp.weaponData.weaponIcon != null)
                WeaponSlotUI.Instance.SetSlotIcon(activeSlotIndex, bp.weaponData.weaponIcon);
        }


        // 🔹 Envanter UI yenile
        Inventory.Instance?.RaiseChanged();

        Debug.Log("🎨 Silah ikonları yenilendi.");
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
        if (Keyboard.current.digit0Key.wasPressedThisFrame) SwitchToSlot(9);

    }

    public void SwitchToSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weaponSlots.Length) return;

        // Aynı slot ise hiçbir şey yapma
        if (newIndex == activeSlotIndex) return;

        // 🔹 Eski aktif silahın canlı şarjörünü kaydet
        if (activeSlotIndex != -1 && activeWeapon != null)
        {
            if (activeWeapon.weaponData.clipSize > 0)
                ammoInClips[activeSlotIndex] = activeWeapon.GetCurrentAmmoInClip();

            if (activeWeapon.IsReloading())
                activeWeapon.StopAllCoroutines();

            if (weaponSlots[activeSlotIndex] != null)
                weaponSlots[activeSlotIndex].SetActive(false);
        }

        // 🔹 Yeni slotu aktif et
        activeSlotIndex = newIndex;
        GameObject newWeaponObject = weaponSlots[activeSlotIndex];

        if (newWeaponObject == null)
        {
            Debug.Log($"[INFO] Slot {newIndex} boş — silah bulunmuyor. Boş slota geçiş yapıldı.");
            activeWeapon = null;
            WeaponSlotUI.Instance?.UpdateHighlight(activeSlotIndex);
            UpdateUI();
            return; // ❗ Silah yoksa sadece boş geç
        }

        // 🔹 Slotta silah varsa aktif et
        newWeaponObject.SetActive(true);
        activeWeapon = newWeaponObject.GetComponent<PlayerWeapon>();

        if (activeWeapon != null)
        {
            activeWeapon.weaponData = equippedBlueprints[activeSlotIndex].weaponData;

            if (activeWeapon.weaponData.clipSize > 0)
                activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);

            Debug.Log($"Başarıyla '{activeWeapon.weaponData.weaponName}' silahına geçildi.");
        }

        // 🔹 Molotov / normal silah ayrımı
        var equippedWeaponData = equippedBlueprints[activeSlotIndex]?.weaponData;
        bool isMolotov = equippedWeaponData != null && equippedWeaponData.isMolotov;

        if (playerWeapon != null)
            playerWeapon.enabled = !isMolotov;

        if (molotovThrower != null)
        {
            molotovThrower.enabled = isMolotov;
            if (isMolotov && molotovThrower.throwPoint == null && playerWeapon != null)
                molotovThrower.throwPoint = playerWeapon.firePoint;
        }

        Debug.Log(isMolotov
            ? "💣 Molotov aktif edildi — PlayerWeapon devre dışı bırakıldı."
            : "🔫 Normal silah aktif — MolotovThrower devre dışı.");

        // 🔹 UI güncelle
        UpdateUI();
        WeaponSlotUI.Instance?.UpdateHighlight(activeSlotIndex);
    }


    // Aktif slottaki blueprint değiştiğinde, objeyi kapatıp açmadan verileri uygula.
    public void ApplyEquippedBlueprintToActiveSlot()
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

    // WeaponSlotManager.cs - en alta yakına ekle
    public void EquipFromInventorySlotToActive(int inventoryIndex)
    {
        var item = Inventory.Instance?.slots[inventoryIndex];
        if (item == null || item.data is not WeaponItemData wid || wid.blueprint == null)
        {
            Debug.Log("⚠️ Bu slotta kuşanılabilir bir silah yok.");
            return;
        }

        // Aktif slot index'i zaten var: activeSlotIndex
        // Bu slotun payload'unu (clip/reserve/durability) item.weapon’dan alıyoruz:
        var payload = item.weapon ?? new InventoryItem.WeaponInstancePayload
        {
            id = System.Guid.NewGuid().ToString("N"),
            clip = wid.blueprint.weaponData.clipSize,
            reserve = wid.blueprint.weaponData.maxAmmoCapacity,
            durability = 100
        };

        // Slot'a tak ve aktif et:
        EquipWeaponInstanceIntoSlot(activeSlotIndex, wid.blueprint, payload);

        // Envanterden “kullanılan” item'ı kaldır (silah tekil olduğundan 1 adet)
        Inventory.Instance.TryRemoveAt(inventoryIndex, 1);

        Debug.Log($"🎒 Envanterden kuşanıldı → {wid.blueprint.weaponName} (Aktif slot: {activeSlotIndex})");

        // UI tazele
        Inventory.Instance.RaiseChanged();
        WeaponSlotUI.Instance?.RefreshAllFromState();
    }


    /*public void RefreshWeaponVisual(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;

        var bp = GetBlueprintForSlot(slotIndex);
        if (bp == null || bp.weaponData == null)
        {
            if (weaponSlots[slotIndex] != null)
                weaponSlots[slotIndex].SetActive(false);
            return;
        }

        var data = bp.weaponData;

        // 🔹 Eski model varsa kaldır
        if (weaponSlots[slotIndex] != null)
            Destroy(weaponSlots[slotIndex]);

        // 🔹 Yeni prefab oluştur
        if (data.prefab != null)
        {
            var newGO = Instantiate(data.prefab, transform);
            newGO.name = $"Weapon_{data.weaponName}";
            weaponSlots[slotIndex] = newGO;

            // 🔹 Aktif slotsa görünür olsun
            newGO.SetActive(slotIndex == activeSlotIndex);

            // 🔹 PlayerWeapon referansını güncelle
            var pw = newGO.GetComponent<PlayerWeapon>();
            if (pw != null)
            {
                pw.weaponData = data;
                pw.SetAmmoInClip(ammoInClips[slotIndex]);
            }
        }

        // 🔹 Hotbar ikonunu güncelle
        WeaponSlotUI.Instance?.SetSlotIcon(slotIndex, data.weaponIcon);

        // 🔹 Envanter UI güncelle
        Inventory.Instance?.RaiseChanged();
    }*/

    public void ForceSwapActiveWeaponPrefab(WeaponBlueprint newBlueprint)
    {
        if (activeSlotIndex < 0 || newBlueprint == null || newBlueprint.weaponData == null)
            return;

        var data = newBlueprint.weaponData;

        // 1️⃣ WeaponHolder referansını bul
        Transform weaponHolder = transform.Find("WeaponHolder");
        if (weaponHolder == null)
        {
            Debug.LogWarning("⚠️ WeaponHolder bulunamadı, otomatik oluşturuluyor...");
            weaponHolder = new GameObject("WeaponHolder").transform;
            weaponHolder.SetParent(transform);
            weaponHolder.localPosition = Vector3.zero;
        }

        // 2️⃣ Eski aktif prefab'ı kaldır
        if (activeWeapon != null && activeWeapon.gameObject != null)
        {
            Destroy(activeWeapon.gameObject);
            activeWeapon = null;
        }

        // 3️⃣ Yeni prefab oluştur
        if (data.prefab != null)
        {
            var newWeaponGO = Instantiate(data.prefab, weaponHolder);
            newWeaponGO.transform.localPosition = Vector3.zero;
            newWeaponGO.transform.localRotation = Quaternion.identity;
            newWeaponGO.name = $"Weapon_{data.weaponName}";
            newWeaponGO.SetActive(true);

            // PlayerWeapon component’ini bağla
            activeWeapon = newWeaponGO.GetComponent<PlayerWeapon>();
            if (activeWeapon != null)
            {
                activeWeapon.weaponData = data;
                activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);
            }

            // WeaponSlot referanslarını güncelle
            weaponSlots[activeSlotIndex] = newWeaponGO;
            equippedBlueprints[activeSlotIndex] = newBlueprint;

            Debug.Log($"✅ Yeni silah oluşturuldu: {data.weaponName}");
        }
        else
        {
            Debug.LogError($"⚠️ {data.weaponName} prefab atanmamış! Lütfen WeaponData.prefab alanını doldur.");
        }

        // 4️⃣ Molotov / normal silah ayrımı
        bool isMolotov = data.isMolotov;
        if (playerWeapon != null)
            playerWeapon.enabled = !isMolotov;

        if (molotovThrower != null)
        {
            molotovThrower.enabled = isMolotov;
            if (isMolotov && molotovThrower.throwPoint == null && playerWeapon != null)
            {
                molotovThrower.throwPoint = playerWeapon.firePoint;
                Debug.Log("MolotovThrower → FirePoint otomatik bağlandı.");
            }
        }

        // 5️⃣ UI senkronizasyonu
        // 🔄 Yeni blueprint'i dizilere uygula
        equippedBlueprints[activeSlotIndex] = newBlueprint;

        // 🔧 Model ve ammo UI güncelle
        // 5️⃣ UI senkronizasyonu
        ApplyEquippedBlueprintToActiveSlot();
        UpdateUI();

        // 🎯 Aktif slottaki ikonun yeni silaha göre güncellenmesi
        if (WeaponSlotUI.Instance != null)
        {
            var currentBlueprint = newBlueprint; // Yeni blueprint doğrudan kullanılıyor
            if (currentBlueprint != null && currentBlueprint.weaponData != null && currentBlueprint.weaponData.weaponIcon != null)
            {
                WeaponSlotUI.Instance.SetSlotIcon(activeSlotIndex, currentBlueprint.weaponData.weaponIcon);
                Debug.Log($"✅ Aktif slot {activeSlotIndex} ikonu {currentBlueprint.weaponData.weaponName} olarak güncellendi.");
            }
            else
            {
                Debug.LogWarning("⚠️ Yeni silahın ikon datası eksik!");
            }
        }

        WeaponSlotUI.Instance?.RefreshAllFromState();



        Debug.Log($"🎯 ActiveWeapon prefab güncellendi -> {data.weaponName}");
        Debug.Log($"[DEBUG] {data.weaponName} için prefab kontrolü: {data.prefab}");
        Debug.Log($"[DEBUG] {data.weaponName} prefab kontrolü: {(data.prefab ? data.prefab.name : "NULL")}");


    }





    // --- Ateş / Reload / UI ---
    private void HandleShootingInput()
    {
        if (activeWeapon == null || Mouse.current == null) return;

        // 🔹 Otomatik silahlar basılı tutularak ateş eder
        bool isAutomatic = activeWeapon.weaponData.isAutomatic;
        bool canShoot = isAutomatic
            ? Mouse.current.leftButton.isPressed
            : Mouse.current.leftButton.wasPressedThisFrame;

        if (canShoot)
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
            }
        }
    }


    private void TryShoot()
    {
        // Fire rate kontrolü zaten PlayerWeapon içinde var
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

    public void UpdateUI()
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
