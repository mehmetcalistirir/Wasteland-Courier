using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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
    public MagazineItem currentMagazine;                 // Takılı şarjör
    public List<MagazineItem> inventoryMags = new();     // Yedek şarjörler

    // Reload Hold Detection
    private float reloadPressTime;
    public float reloadHoldThreshold = 0.4f;

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
    if (GameStateManager.IsGamePaused || weaponData == null || isReloading)
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

    // ============================
    // ACTUAL SHOOT LOGIC
    // ============================
    public void Shoot()
    {
        if (isReloading) return;

        if (currentMagazine == null || currentMagazine.currentAmmo <= 0)
        {
            PlayEmptyClipSound();
            Debug.Log("Boş! Ateş yok.");
            return;
        }

        currentMagazine.currentAmmo--;

        RangedAttack();
        Debug.Log($"Ateş edildi! Şarjör: {currentMagazine.currentAmmo}/{currentMagazine.capacity}");
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
        if (isReloading) yield break;

        if (inventoryMags.Count == 0)
        {
            Debug.Log("Yedek şarjör yok.");
            yield break;
        }

        isReloading = true;

        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(weaponData.reloadTime);

        // Eski şarjörü geri koy
        if (currentMagazine != null)
            inventoryMags.Add(currentMagazine);

        // En dolu şarjörü bul
        var nextMag = inventoryMags.OrderByDescending(m => m.currentAmmo).FirstOrDefault();
        if (nextMag == null)
        {
            Debug.Log("Takılabilir şarjör yok!");
            isReloading = false;
            yield break;
        }

        inventoryMags.Remove(nextMag);
        currentMagazine = nextMag;

        Debug.Log($"Yeni şarjör → {currentMagazine.currentAmmo}/{currentMagazine.capacity}");

        isReloading = false;
    }

    // ============================
    // MAGAZINE CHECK (HOLD R)
    // ============================
    IEnumerator MagCheck()
    {
        if (currentMagazine == null)
        {
            Debug.Log("Şarjör takılı değil.");
            yield break;
        }

        yield return new WaitForSeconds(0.3f);

        float ratio = (float)currentMagazine.currentAmmo / currentMagazine.capacity;

        if (ratio == 1f) Debug.Log("Tam dolu.");
        else if (ratio >= 0.7f) Debug.Log("Neredeyse dolu.");
        else if (ratio >= 0.4f) Debug.Log("Yarısı dolu.");
        else if (ratio > 0) Debug.Log("Az mermi kaldı.");
        else Debug.Log("Tamamen boş.");
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

    // Mevcut modeli sil
    if (currentModel != null)
        Destroy(currentModel);

    // Yeni modeli oluştur
    if (weaponData.prefab != null)
    {
        currentModel = Instantiate(weaponData.prefab, transform);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;

        // FirePoint'i bul
        Transform fp = currentModel.transform.Find("FirePoint");
        if (fp != null)
            firePoint = fp;
    }

    // Şarjör sistemini sıfırla
    currentMagazine = null;
    inventoryMags.Clear();

    Debug.Log($"[PlayerWeapon] Yeni silah takıldı → {weaponData.name}. Şarjörler sıfırlandı.");
}

}
