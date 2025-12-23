using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    // References
    private GameObject currentModel;
    private AudioSource audioSource;
    private Animator animator;

    private WeaponSlotManager slotManager;

    [Header("Weapon Configuration")]
    public WeaponDefinition weaponData;

    [Header("Components")]
    public Transform firePoint;
    public LayerMask enemyLayer;

    [Header("Lights")]
    public Light2D spotLight;        // sürekli açık
    public Light2D muzzleFlash;      // ateşte yanar
    public float muzzleFlashDuration = 0.05f;
    public float muzzleFlashIntensity = 1f;
    private Coroutine muzzleFlashCo;
    [Header("Ammo Check")]
    public float ammoCheckDuration = 0.35f;
    public AudioClip ammoCheckSound;
    public string ammoCheckAnimTrigger = "AmmoCheck"; // Animator’da Trigger
    public AmmoCheckUI ammoCheckUI; // UI referansı (aşağıda yazacağım)
    private bool isCheckingMag = false;

    [Header("Reload Strategy")]
    public ReloadStrategy reloadStrategy = ReloadStrategy.MostFull;
    public bool IsBusy =>
    isReloading || isCheckingMag;

    private MagazineInstance lastUsedMagazine;
    private int lastMagIndex = -1;
    public event System.Action<int, int> OnMagazineChanged;
    // currentAmmo, capacity

    public event System.Action<int> OnSpareMagazineCountChanged;
    // yedek şarjör sayısı

    public event System.Action<string> OnAmmoCheckFeedback;
    // "Az mermi var", "Tam dolu" vs.

    private float lastShotTime;


    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClipSound;

    // Input System
    private InputAction reloadAction;


    // State
    private bool isReloading = false;
    private bool isAiming = false;

    private float lastAutoFireTime = 0f;
    private bool reloadHeldTriggered;
    private int _lastSpareCount = -1;



    // NEW MAGAZINE SYSTEM
    public MagazineInstance currentMagazine;
    public List<MagazineInstance> inventoryMags = new List<MagazineInstance>();


    [Header("Reload Input")]
    public float reloadHoldThreshold = 0.4f;


    private bool magCheckTriggered;



    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        if (muzzleFlash != null)
        {
            muzzleFlash.enabled = false;
            muzzleFlash.intensity = 0f;
        }
    }


    private void Start()
    {
        CollectMagazinesFromInventory();

        slotManager = WeaponSlotManager.Instance;

        if (slotManager != null)
            LoadFromSlot(slotManager.activeSlotIndex);
        else
            Debug.LogError("[PlayerWeapon] Slot Manager null!");

        CollectMagazinesFromInventory();

        if (currentMagazine == null && inventoryMags.Count > 0)
        {
            currentMagazine = SelectNextMagazine();
            inventoryMags.Remove(currentMagazine);

            OnMagazineChanged?.Invoke(
                currentMagazine.currentAmmo,
                currentMagazine.data.capacity
            );
        }

    }

    public void CollectMagazinesFromInventory()
    {
        var inv = Inventory.Instance;
        if (inv == null || weaponData == null)
            return;

        inventoryMags.Clear();

        // 🔍 Envanterdeki uyumlu şarjörleri topla
        for (int i = 0; i < inv.slots.Length; i++)
        {
            var slot = inv.slots[i];
            if (slot == null) continue;

            var mag = slot.magazineInstance;
            if (mag == null || mag.data == null) continue;

            if (!IsMagazineCompatible(mag)) continue;

            inventoryMags.Add(mag);
        }

        // 🔒 Takılı şarjör listeden çıkar (double sayılmasın)
        if (currentMagazine != null)
            inventoryMags.Remove(currentMagazine);

        // 📢 Event → SADECE sayı değiştiyse
        int spareCount = inventoryMags.Count;
        if (spareCount != _lastSpareCount)
        {
            _lastSpareCount = spareCount;
            OnSpareMagazineCountChanged?.Invoke(spareCount);
        }
    }




    private MagazineInstance SelectNextMagazine()
    {
        if (inventoryMags == null || inventoryMags.Count == 0)
            return null;

        // Uyumlu olanları topla (GC-free)
        // (İstersen liste oluşturmadan direkt seçebilirsin)
        MagazineInstance best = null;

        if (reloadStrategy == ReloadStrategy.MostFull || reloadStrategy == ReloadStrategy.LastUsed)
        {
            int bestAmmo = -1;

            for (int i = 0; i < inventoryMags.Count; i++)
            {
                var m = inventoryMags[i];
                if (m == null || m.data == null) continue;
                if (!IsMagazineCompatible(m)) continue;

                if (reloadStrategy == ReloadStrategy.LastUsed && lastUsedMagazine != null && m == lastUsedMagazine)
                    return m; // direkt dön

                if (m.currentAmmo > bestAmmo)
                {
                    bestAmmo = m.currentAmmo;
                    best = m;
                }
            }

            return best;
        }

        // NextInList (uyumlular arasında sırayla)
        // Uyumlu sayısını bul, sonra index seç
        int compatibleCount = 0;
        for (int i = 0; i < inventoryMags.Count; i++)
        {
            var m = inventoryMags[i];
            if (m == null || m.data == null) continue;
            if (!IsMagazineCompatible(m)) continue;
            compatibleCount++;
        }

        if (compatibleCount == 0) return null;

        lastMagIndex = (lastMagIndex + 1) % compatibleCount;

        // lastMagIndex’in işaret ettiği uyumlu elemanı bul
        int k = 0;
        for (int i = 0; i < inventoryMags.Count; i++)
        {
            var m = inventoryMags[i];
            if (m == null || m.data == null) continue;
            if (!IsMagazineCompatible(m)) continue;

            if (k == lastMagIndex)
                return m;

            k++;
        }

        return null;
    }


    // ============================
    // INPUT SYSTEM ONENABLE / OFF
    // ============================
private void OnEnable()
{
    var gameplay = PlayerInputRouter.Instance
        .inputActions
        .FindActionMap("Gameplay");

    reloadAction = gameplay.FindAction("Reload");

    reloadAction.started += OnReloadStarted;
    reloadAction.performed += OnReloadPerformed;
    reloadAction.canceled += OnReloadCanceled;

    if (Inventory.Instance != null)
        Inventory.Instance.OnChanged += OnInventoryChanged;
}





private void OnDisable()
{
    if (reloadAction != null)
    {
        reloadAction.started -= OnReloadStarted;
        reloadAction.performed -= OnReloadPerformed;
        reloadAction.canceled -= OnReloadCanceled;
    }

    if (Inventory.Instance != null)
        Inventory.Instance.OnChanged -= OnInventoryChanged;
}


    private void OnInventoryChanged()
    {
        CollectMagazinesFromInventory();
    }


    private void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        if (isReloading || isCheckingMag) return;
        if (weaponData == null) return;

        // HOLD tamamlandıysa → MagCheck
        if (ctx.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            reloadHeldTriggered = true;
            StartCoroutine(MagCheck());
            return;
        }

        // PRESS → Reload (ama hold’a gitmeyecekse)
        if (!reloadHeldTriggered)
            TryReload();
    }

    private void OnReloadCanceled(InputAction.CallbackContext ctx)
    {
        // Şimdilik boş
    }

    private void OnReloadStarted(InputAction.CallbackContext ctx)
    {
        reloadHeldTriggered = false;
    }




    private void SwitchToSlot(int slot)
    {
        WeaponSlotManager.Instance.SwitchSlot(slot);
    }

    // ============================
    // SHOOT (NEW INPUT SYSTEM)
    // ============================



    private void Update()
    {
        if (GameStateManager.IsGamePaused || weaponData == null)
            return;

        // 🔫 Tekli atış
        if (!weaponData.isAutomatic)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastAutoFireTime >= (1f / weaponData.fireRate))
                {
                    lastAutoFireTime = Time.time;
                    Shoot();
                }
            }
        }


        // 🔥 Otomatik atış
        if (weaponData.isAutomatic)
        {
            if (Input.GetMouseButton(0))
                AutoFire();
        }


    }

    // ============================
    // MAGAZINE CHECK (HOLD R)
    // ============================


    public bool TryEquipMagazineFromInventory(MagazineInstance mag)
    {
        if (mag == null || mag.data == null)
            return false;

        if (!CanEquipMagazine(mag))
        {
            ammoCheckUI?.Show("Bu şarjör uyumsuz!");
            return false;
        }

        if (isReloading || isCheckingMag)
            return false;

        currentMagazine = mag;

        Debug.Log(
            $"[Weapon] Şarjör takıldı → " +
            $"{mag.currentAmmo}/{mag.data.capacity}"
        );

        Inventory.Instance.RaiseChanged();
        return true;
    }


    private void AutoFire()
    {
        if (isReloading || isCheckingMag)
            return;

        float fireDelay = 1f / weaponData.fireRate;

        if (Time.time - lastAutoFireTime >= fireDelay)
        {
            lastAutoFireTime = Time.time;
            Shoot();
        }
    }

    private void TryReload()
    {
        if (isReloading || isCheckingMag) return;
        if (weaponData == null)
        {
            ammoCheckUI?.Show("Silah yok.");
            return;
        }

        // bozuk state koruması
        if (currentMagazine != null && currentMagazine.data == null)
            currentMagazine = null;

        // her reload girişinde listeyi tazele (pickup sonrası otomatik görsün)
        CollectMagazinesFromInventory();

        // Silah boşsa → direkt şarjör tak
        if (currentMagazine == null)
        {
            MagazineInstance next = SelectNextMagazine();
            if (next == null)
            {
                ammoCheckUI?.Show("Uyumlu şarjör yok.");
                return;
            }

            inventoryMags.Remove(next);
            currentMagazine = next;

            OnMagazineChanged?.Invoke(
                currentMagazine.currentAmmo,
                currentMagazine.data.capacity
            );
            OnSpareMagazineCountChanged?.Invoke(inventoryMags.Count);
            return;
        }


        // currentMagazine.data artık garanti
        if (currentMagazine.currentAmmo >= currentMagazine.data.capacity)
        {
            ammoCheckUI?.Show("Şarjör zaten dolu.");
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    public void NotifyMagazineAmmoChanged()
    {
        if (currentMagazine != null && currentMagazine.data != null)
        {
            OnMagazineChanged?.Invoke(
                currentMagazine.currentAmmo,
                currentMagazine.data.capacity
            );
        }

        CollectMagazinesFromInventory();
    }

    // ============================
    // ACTUAL SHOOT LOGIC
    // ============================
    public void Shoot()
    {
        float cooldown =
    weaponData.isShotgun
    ? weaponData.shotgunCooldown
    : (1f / weaponData.fireRate);

        if (Time.time - lastShotTime < cooldown)
            return;

        lastShotTime = Time.time;

        if (currentMagazine == null)
        {
            PlayEmptyClipSound();
            ammoCheckUI?.Show("Şarjör yok.");
            return;
        }

        if (isReloading || isCheckingMag)
            return;

        if (currentMagazine == null || currentMagazine.data == null)
        {
            Debug.Log("Şarjör takılı değil.");
            return;
        }

        if (weaponData == null || weaponData.ammoType == null)
        {
            Debug.LogError("WeaponData veya ammoType NULL!");
            return;
        }

        if (currentMagazine.data == null || currentMagazine.data.ammoType == null)
        {
            Debug.LogError("MagazineData veya ammoType NULL!");
            return;
        }

        if (isReloading) return;

        if (currentMagazine == null)
        {
            PlayEmptyClipSound();
            Debug.Log("Şarjör takılı değil.");
            return;
        }

        if (currentMagazine.data.ammoType != weaponData.ammoType)
        {
            Debug.LogError("UYUMSUZ ŞARJÖR TAKILI!");
            return;
        }

        if (currentMagazine.currentAmmo <= 0)
        {
            PlayEmptyClipSound();
            Debug.Log("Şarjör boş.");
            return;
        }
        if (currentMagazine == null || currentMagazine.currentAmmo <= 0)
        {
            PlayEmptyClipSound();
            Debug.Log("Boş şarjör!");
            return;
        }


        currentMagazine.currentAmmo--;
        RangedAttack();
        lastUsedMagazine = currentMagazine;
        OnMagazineChanged?.Invoke(
            currentMagazine.currentAmmo,
            currentMagazine.data.capacity
        );



    }
    public MagazineInstance GetCurrentMagazine()
    {
        return currentMagazine;
    }

    public void SetCurrentMagazine(MagazineInstance mag)
    {
        currentMagazine = mag;
    }

    public bool CanEquipMagazine(MagazineInstance mag)
    {
        return IsMagazineCompatible(mag);
    }



    private void RangedAttack()
    {
        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        animator?.SetTrigger("Shoot");

        if (weaponData.isShotgun)
            FireShotgunPellets();
        else
            FireSingleBullet();

        TryMuzzleFlash();
    }

    private void FireSingleBullet()
    {
        if (weaponData.projectilePrefab == null)
        {
            Debug.LogError("WeaponData projectilePrefab NULL!");
            return;
        }

        var projectile = Instantiate(
            weaponData.projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        if (projectile.TryGetComponent(out WeaponBullet b))
        {
            b.damage = weaponData.damage;
            b.owner = transform;
            b.weaponType = weaponData.weaponType;
            b.knockbackForce = weaponData.knockbackForce;
            b.knockbackDuration = weaponData.knockbackDuration;
        }
    }


    private void FireShotgunPellets()
    {
        if (weaponData.projectilePrefab == null)
            return;

        int pellets = Mathf.Max(weaponData.pelletsPerShot, 1);
        float spread = weaponData.pelletSpreadAngle;

        for (int i = 0; i < pellets; i++)
        {
            float t = pellets == 1 ? 0.5f : (float)i / (pellets - 1);
            float angle = Mathf.Lerp(-spread, spread, t);

            Quaternion rot =
                firePoint.rotation * Quaternion.Euler(0, 0, angle);

            var proj = Instantiate(
                weaponData.projectilePrefab,
                firePoint.position,
                rot
            );

            if (proj.TryGetComponent(out WeaponBullet b))
            {
                b.damage = weaponData.damage;
                b.owner = transform;
                b.weaponType = weaponData.weaponType;
            }
        }
    }


    // ============================
    // ADS
    // ============================
    private void StartADS()
    {
        isAiming = true;
        animator?.SetBool("ADS", true);
    }

    private void StopADS()
    {
        isAiming = false;
        animator?.SetBool("ADS", false);
    }

    // ============================
    // RELOAD (SHORT OR LONG HOLD)
    // ============================


    private IEnumerator ReloadRoutine()
    {
        if (isReloading)
            yield break;

        // 🔒 SADECE UYUMLU ŞARJÖRLER
        bool hasCompatible = false;
        for (int i = 0; i < inventoryMags.Count; i++)
        {
            var m = inventoryMags[i];
            if (m == null || m.data == null) continue;
            if (m.data.ammoType != weaponData.ammoType) continue;
            if (!IsMagazineCompatible(m)) continue;

            hasCompatible = true;
            break;
        }

        if (!hasCompatible)
        {
            // (Log istersen sadece editor)
            // #if UNITY_EDITOR
            // Debug.Log("Bu silaha uygun şarjör yok!");
            // #endif
            yield break;
        }


        isReloading = true;

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        currentMagazine = null;

        MagazineInstance nextMag = SelectNextMagazine();

        if (nextMag == null)
        {
            Debug.Log("Takılabilir şarjör yok!");
            isReloading = false;
            yield break;
        }

        inventoryMags.Remove(nextMag);
        currentMagazine = nextMag;
        lastUsedMagazine = currentMagazine;


        Debug.Log(
            $"Yeni şarjör takıldı → {currentMagazine.currentAmmo}/{currentMagazine.data.capacity}"
        );

        isReloading = false;
        OnMagazineChanged?.Invoke(
    currentMagazine.currentAmmo,
    currentMagazine.data.capacity
);

        OnSpareMagazineCountChanged?.Invoke(inventoryMags.Count);

    }

    private bool IsMagazineCompatible(MagazineInstance mag)
    {
        if (mag == null || mag.data == null || weaponData == null)
            return false;

        // acceptedMagazines null veya boş kontrolü
        if (weaponData.acceptedMagazines == null || weaponData.acceptedMagazines.Count == 0)
            return false;

        return weaponData.acceptedMagazines.Contains(mag.data.magazineType);
    }




    // ============================
    // MAGAZINE CHECK (HOLD R)
    // ============================
    private IEnumerator MagCheck()
    {
        // Zaten kontrol ediliyorsa tekrar girme
        if (isCheckingMag)
            yield break;

        // Reload sırasında check yapılamaz
        if (isReloading)
            yield break;

        // Şarjör yok
        if (currentMagazine == null || currentMagazine.data == null)
        {
            ammoCheckUI?.Show("Şarjör takılı değil.");
            OnAmmoCheckFeedback?.Invoke("Şarjör takılı değil.");
            yield break;
        }

        // Uyumsuz şarjör (sert kontrol)
        if (!CanEquipMagazine(currentMagazine))
        {
            ammoCheckUI?.Show("Uyumsuz şarjör!");
            OnAmmoCheckFeedback?.Invoke("Uyumsuz şarjör!");
            yield break;
        }

        isCheckingMag = true;

        // 🔊 Ses
        if (ammoCheckSound != null)
            audioSource.PlayOneShot(ammoCheckSound);

        // 🎞 Animasyon
        if (!string.IsNullOrEmpty(ammoCheckAnimTrigger))
            animator?.SetTrigger(ammoCheckAnimTrigger);

        // ⏱ Gerçekçi kontrol süresi
        yield return new WaitForSeconds(ammoCheckDuration);

        // 📊 Doluluk oranı
        int ammo = currentMagazine.currentAmmo;
        int cap = currentMagazine.data.capacity;

        float ratio = cap > 0 ? (float)ammo / cap : 0f;

        string msg;

        if (ammo <= 0)
            msg = "Boş gibi.";
        else if (ratio < 0.25f)
            msg = "Az mermi var.";
        else if (ratio < 0.6f)
            msg = "Yarısı dolu.";
        else if (ratio < 1f)
            msg = "Neredeyse dolu.";
        else
            msg = "Tam dolu.";

        // 📢 UI + Event
        ammoCheckUI?.Show(msg);
        OnAmmoCheckFeedback?.Invoke(msg);

        isCheckingMag = false;
    }




    // ============================
    // MUZZLE FLASH
    // ============================
    private void TryMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        muzzleFlash.transform.position = firePoint.position;

        if (muzzleFlashCo != null)
            StopCoroutine(muzzleFlashCo);

        muzzleFlashCo = StartCoroutine(MuzzleFlashRoutine());
    }


    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlash.enabled = true;
        muzzleFlash.intensity = muzzleFlashIntensity;

        yield return new WaitForSeconds(muzzleFlashDuration);

        muzzleFlash.intensity = 0f;
        muzzleFlash.enabled = false;
    }


    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null)
            audioSource.PlayOneShot(emptyClipSound);
    }

    // ============================
    // SLOT / WEAPON LOAD
    // ============================
    public void LoadFromSlot(int slot)
    {
        WeaponItemData item =
            WeaponSlotManager.Instance.GetWeaponItemInSlot(slot);

        if (item == null || item.weaponDefinition == null)
            return;

        weaponData = item.weaponDefinition;
        lastAutoFireTime = 0f;
        isReloading = false;

        if (currentModel != null)
            Destroy(currentModel);

        if (weaponData.prefab != null)
        {
            currentModel = Instantiate(weaponData.prefab, transform);
            Transform fp = currentModel.transform.Find("FirePoint");
            if (fp != null)
                firePoint = fp;
        }

        CollectMagazinesFromInventory();
    }


    public void SetWeapon(WeaponDefinition data)
    {
        StopAllCoroutines();
        isReloading = false;
        isCheckingMag = false;

        if (data == null)
            return;

        weaponData = data;

        // Model
        if (currentModel != null)
            Destroy(currentModel);

        if (weaponData.prefab != null)
        {
            currentModel = Instantiate(weaponData.prefab, transform);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            Transform fp = currentModel.transform.Find("FirePoint");
            if (fp != null)
                firePoint = fp;
        }

        // 🔥 ŞARJÖR BAĞLAMA
        inventoryMags.Clear();
        currentMagazine = null;

        var inv = Inventory.Instance;

        for (int i = 0; i < inv.slots.Length; i++)
        {
            var slot = inv.slots[i];

            if (slot.magazineInstance == null)
                continue;

            MagazineInstance mag = slot.magazineInstance;

            if (mag.data == null || mag.data.ammoType == null)
                continue;

            // 🔒 SADECE BU SİLAHA UYUMLU ŞARJÖR
            if (IsMagazineCompatible(mag))
            {
                inventoryMags.Add(mag);
            }

        }

        // 🔫 OTOMATİK TAK (opsiyonel ama önerilir)
        if (inventoryMags.Count > 0)
        {
            MagazineInstance best = null;
            int maxAmmo = -1;

            foreach (var m in inventoryMags)
            {
                if (m.currentAmmo > maxAmmo)
                {
                    maxAmmo = m.currentAmmo;
                    best = m;
                }
            }

            currentMagazine = best;


            inventoryMags.Remove(currentMagazine);

            Debug.Log(
                $"[Weapon] Otomatik şarjör takıldı → " +
                $"{currentMagazine.currentAmmo}/{currentMagazine.data.capacity}"
            );
        }
        else
        {
            Debug.Log("[Weapon] Uyumlu şarjör bulunamadı.");
        }
        CollectMagazinesFromInventory();

        if (currentMagazine == null && inventoryMags.Count > 0)
        {
            currentMagazine = SelectNextMagazine();
            inventoryMags.Remove(currentMagazine);
        }


    }



}
