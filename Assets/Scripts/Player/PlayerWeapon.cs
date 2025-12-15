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
    public WeaponData weaponData;

    [Header("Components")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;

    [Header("Muzzle Flash")]
    public Light2D muzzleFlash;
    public float muzzleFlashDuration = 0.05f;
    public float muzzleFlashIntensity = 3f;
    private Coroutine muzzleFlashCo;
    [Header("Ammo Check")]
    public float ammoCheckDuration = 0.35f;
    public AudioClip ammoCheckSound;
    public string ammoCheckAnimTrigger = "AmmoCheck"; // Animator’da Trigger
    public AmmoCheckUI ammoCheckUI; // UI referansı (aşağıda yazacağım)
    private bool isCheckingMag = false;

    [Header("Reload Strategy")]
    public ReloadStrategy reloadStrategy = ReloadStrategy.MostFull;

    private MagazineInstance lastUsedMagazine;
    private int lastMagIndex = -1;


    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClipSound;

    // Input System
    private PlayerControls controls;

    // State
    private bool isReloading = false;
    private bool isAiming = false;

    private float lastAutoFireTime = 0f;



    // NEW MAGAZINE SYSTEM
    public MagazineInstance currentMagazine;
    public List<MagazineInstance> inventoryMags = new List<MagazineInstance>();


    [Header("Reload Input")]
    public float reloadHoldThreshold = 0.4f;

    private float reloadPressTime;
    private bool reloadPressed;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        controls = new PlayerControls();

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;
    }

    private void Start()
    {
        slotManager = WeaponSlotManager.Instance;

        if (slotManager != null)
            LoadFromSlot(slotManager.activeSlotIndex);
        else
            Debug.LogError("[PlayerWeapon] Slot Manager null!");

        CollectMagazinesFromInventory();
    }

    private void CollectMagazinesFromInventory()
    {
        inventoryMags.Clear();
        currentMagazine = null;

        var inv = Inventory.Instance;
        if (inv == null) return;

        for (int i = 0; i < inv.slots.Length; i++)
        {
            var slot = inv.slots[i];

            if (slot == null || slot.magazineInstance == null)
                continue;

            var mag = slot.magazineInstance;

            // 🔒 silaha uygun mu?
            if (weaponData != null &&
                mag.data != null &&
                mag.data.ammoType == weaponData.ammoType)
            {
                inventoryMags.Add(mag);

                // inventory’den çıkar
                inv.slots[i] = new InventoryItem();
            }
        }

        // 🔥 otomatik en dolu şarjörü tak
        if (inventoryMags.Count > 0)
        {
            currentMagazine = inventoryMags
                .OrderByDescending(m => m.currentAmmo)
                .First();

            inventoryMags.Remove(currentMagazine);

            Debug.Log(
                $"[Weapon] Şarjör takıldı → {currentMagazine.currentAmmo}/{currentMagazine.data.capacity}"
            );
        }
        else
        {
            Debug.Log("[Weapon] Uygun şarjör bulunamadı.");
        }

        inv.RaiseChanged();
    }

    private MagazineInstance SelectNextMagazine()
    {
        if (inventoryMags == null || inventoryMags.Count == 0)
            return null;

        // 🔒 SADECE UYUMLU ŞARJÖRLER
        var compatible = inventoryMags
            .Where(m =>
                m != null &&
                m.data != null &&
                IsMagazineCompatible(m)
            )
            .ToList();

        if (compatible.Count == 0)
            return null;

        switch (reloadStrategy)
        {
            case ReloadStrategy.MostFull:
                return compatible
                    .OrderByDescending(m => m.currentAmmo)
                    .First();

            case ReloadStrategy.NextInList:
                lastMagIndex = (lastMagIndex + 1) % compatible.Count;
                return compatible[lastMagIndex];

            case ReloadStrategy.LastUsed:
                if (lastUsedMagazine != null &&
                    compatible.Contains(lastUsedMagazine))
                {
                    return lastUsedMagazine;
                }
                return compatible
                    .OrderByDescending(m => m.currentAmmo)
                    .First();
        }

        return null;
    }

    // ============================
    // INPUT SYSTEM ONENABLE / OFF
    // ============================
    private void OnEnable()
    {
        controls.Gameplay.Enable();

        // Slot Switching
        controls.Gameplay.Weapon1.performed += ctx => SwitchToSlot(0);
        controls.Gameplay.Weapon2.performed += ctx => SwitchToSlot(1);
        controls.Gameplay.Weapon3.performed += ctx => SwitchToSlot(2);

        // ADS
        controls.Gameplay.ADS.started += ctx => StartADS();
        controls.Gameplay.ADS.canceled += ctx => StopADS();

        // Reload Hold Detection
        controls.Gameplay.Reload.started += ctx =>
        {
            reloadPressTime = Time.time;
        };

        controls.Gameplay.Reload.canceled += ctx =>
        {
            float held = Time.time - reloadPressTime;

            if (held >= reloadHoldThreshold)
                StartCoroutine(MagCheck());
            else
                StartCoroutine(ReloadRoutine());
        };
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    private void SwitchToSlot(int slot)
    {
        WeaponSlotManager.Instance.SwitchSlot(slot);
    }

    // ============================
    // SHOOT (NEW INPUT SYSTEM)
    // ============================

    private void HandleShootStart()
    {

        if (!weaponData.isAutomatic)
            Shoot();
    }


    private void Update()
    {
        if (GameStateManager.IsGamePaused || weaponData == null || isReloading || isCheckingMag)
            return;


        // 🔫 Tekli atış (semi-auto)
        if (!weaponData.isAutomatic)
        {
            if (Input.GetMouseButtonDown(0))
                Shoot();
        }

        // 🔥 Otomatik atış (auto)
        if (weaponData.isAutomatic)
        {
            if (Input.GetMouseButton(0))
                AutoFire();
        }

        // R basıldı
        if (Input.GetKeyDown(KeyCode.R))
        {
            reloadPressTime = Time.time;
            reloadPressed = true;
        }

        // R bırakıldı
        if (Input.GetKeyUp(KeyCode.R) && reloadPressed)
        {
            reloadPressed = false;
            float heldTime = Time.time - reloadPressTime;

            if (heldTime >= reloadHoldThreshold)
            {
                StartCoroutine(MagCheck());
            }
            else
            {
                TryReload();
            }
        }
    }
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

        // Eski şarjörü geri koy
        if (currentMagazine != null)
            inventoryMags.Add(currentMagazine);

        inventoryMags.Remove(mag);
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
        float fireDelay = 1f / weaponData.fireRate;

        if (Time.time - lastAutoFireTime >= fireDelay)
        {
            lastAutoFireTime = Time.time;
            Shoot();
        }
    }

    private void TryReload()
    {
        if (isReloading || isCheckingMag)
            return;

        if (currentMagazine == null || currentMagazine.data == null)
        {
            ammoCheckUI?.Show("Şarjör takılı değil.");
            return;
        }

        if (!CanEquipMagazine(currentMagazine))
        {
            ammoCheckUI?.Show("Uyumsuz şarjör!");
            return;
        }

        if (currentMagazine.currentAmmo >= currentMagazine.data.capacity)
        {
            ammoCheckUI?.Show("Şarjör zaten dolu.");
            return;
        }

        StartCoroutine(ReloadRoutine());
    }



    // ============================
    // ACTUAL SHOOT LOGIC
    // ============================
    public void Shoot()
    {
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

        currentMagazine.currentAmmo--;
        RangedAttack();
        lastUsedMagazine = currentMagazine;

    }

    public bool CanEquipMagazine(MagazineInstance mag)
    {
        if (mag == null || mag.data == null) return false;
        return mag.data.ammoType == weaponData.ammoType;
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
        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent(out WeaponBullet b))
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
        int pellets = Mathf.Max(weaponData.pelletsPerShot, 1);
        float spread = weaponData.pelletSpreadAngle;

        for (int i = 0; i < pellets; i++)
        {
            float t = pellets == 1 ? 0f : (float)i / (pellets - 1);
            float angle = Mathf.Lerp(-spread, spread, t);
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            var bullet = Instantiate(bulletPrefab, firePoint.position, rot);

            if (bullet.TryGetComponent(out WeaponBullet b))
            {
                b.damage = weaponData.damage;
                b.owner = transform;
                b.weaponType = weaponData.weaponType;
                b.knockbackForce = weaponData.knockbackForce;
                b.knockbackDuration = weaponData.knockbackDuration;
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
        var compatibleMags = inventoryMags
            .Where(m =>
                m != null &&
                m.data != null &&
                m.data.ammoType == weaponData.ammoType
            )
            .ToList();

        if (compatibleMags.Count == 0)
        {
            Debug.Log("Bu silaha uygun şarjör yok!");
            yield break;
        }

        isReloading = true;

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        // Eski şarjörü geri koy (uyumluysa)
        if (currentMagazine != null)
        {
            inventoryMags.Add(currentMagazine);
            currentMagazine = null;
        }

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


        inventoryMags.Remove(nextMag);
        currentMagazine = nextMag;

        Debug.Log(
            $"Yeni şarjör takıldı → {currentMagazine.currentAmmo}/{currentMagazine.data.capacity}"
        );

        isReloading = false;
    }

    private bool IsMagazineCompatible(MagazineInstance mag)
    {
        if (mag == null || mag.data == null || weaponData == null)
            return false;

        return weaponData.acceptedMagazines.Contains(mag.data.magazineType);
    }


    // ============================
    // MAGAZINE CHECK (HOLD R)
    // ============================
    private IEnumerator MagCheck()
    {
        if (isCheckingMag) yield break;

        if (currentMagazine == null || currentMagazine.data == null)
        {
            ammoCheckUI?.Show("Şarjör takılı değil.");
            yield break;
        }

        // Uyumluluk (sertleştirilmiş)
        if (!CanEquipMagazine(currentMagazine))
        {
            ammoCheckUI?.Show("Uyumsuz şarjör!");
            yield break;
        }

        isCheckingMag = true;

        // Ses
        if (ammoCheckSound != null)
            audioSource.PlayOneShot(ammoCheckSound);

        // Anim
        if (!string.IsNullOrEmpty(ammoCheckAnimTrigger))
            animator?.SetTrigger(ammoCheckAnimTrigger);

        yield return new WaitForSeconds(ammoCheckDuration);

        float ratio = (float)currentMagazine.currentAmmo / currentMagazine.data.capacity;

        string msg;
        if (ratio <= 0f) msg = "Boş gibi.";
        else if (ratio < 0.25f) msg = "Az mermi var.";
        else if (ratio < 0.6f) msg = "Yarısı dolu.";
        else if (ratio < 1f) msg = "Neredeyse dolu.";
        else msg = "Tam dolu.";

        ammoCheckUI?.Show(msg);

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
        float original = muzzleFlash.intensity;
        muzzleFlash.enabled = true;
        muzzleFlash.intensity = muzzleFlashIntensity;

        yield return new WaitForSeconds(muzzleFlashDuration);

        muzzleFlash.intensity = original;
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
        var data = slotManager.GetEquippedWeapon(slot);
        if (data == null) return;

        weaponData = data;
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
    }

    public void SetWeapon(WeaponData data)
    {
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
            if (mag.data.ammoType == weaponData.ammoType)
            {
                inventoryMags.Add(mag);
            }
        }

        // 🔫 OTOMATİK TAK (opsiyonel ama önerilir)
        if (inventoryMags.Count > 0)
        {
            currentMagazine = inventoryMags
                .OrderByDescending(m => m.currentAmmo)
                .First();

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
    }



}
