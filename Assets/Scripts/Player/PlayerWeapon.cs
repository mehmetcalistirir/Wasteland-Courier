using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    private GameObject currentModel;

    private bool isAiming = false;
    private bool isReloading = false;

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

    private AudioSource audioSource;
    private Animator animator;

    private int clip = 0;
    private int reserve = 0;
    private int clipSize = 0;

    private float lastAutoFireTime = 0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

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

    private void Update()
    {
        if (weaponData == null || PauseMenu.IsPaused || isReloading)
            return;

        // ---- Shoot Input (Klasik fare tıklaması) ----
        bool mouseHeld = Mouse.current.leftButton.isPressed;
        bool mousePressedOnce = Mouse.current.leftButton.wasPressedThisFrame;

        if (weaponData.isAutomatic)
        {
            if (mouseHeld)
                AutoFire();
        }
        else
        {
            if (mousePressedOnce)
                Shoot();
        }

        // ---- ADS Input ----
        if (Mouse.current.rightButton.wasPressedThisFrame)
            StartADS();

        if (Mouse.current.rightButton.wasReleasedThisFrame)
            StopADS();
    }

    // ===============================
    // AUTO FIRE
    // ===============================
    private void AutoFire()
    {
        float fireDelay = 1f / weaponData.fireRate;

        if (Time.time - lastAutoFireTime >= fireDelay)
        {
            lastAutoFireTime = Time.time;
            Shoot();
        }
    }

    // ===============================
    // SHOOTING
    // ===============================
    public void Shoot()
    {
        if (isReloading || PauseMenu.IsPaused)
            return;

        if (clip <= 0)
        {
            PlayEmptyClipSound();
            return;
        }

        RangedAttack();
    }

    private void RangedAttack()
    {
        clip--;
        SyncAmmoToSlot();

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
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

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

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);

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

    // ===============================
    // ADS
    // ===============================
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

    // ===============================
    // MELEE
    // ===============================
    public void MeleeAttack()
    {
        Physics2D.OverlapCircleAll(firePoint.position, weaponData.attackRange, enemyLayer);
    }

    // ===============================
    // RELOAD
    // ===============================
    public IEnumerator Reload()
    {
        if (isReloading || reserve <= 0 || clip >= clipSize)
            yield break;

        isReloading = true;

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = clipSize - clip;
        int loadAmount = Mathf.Min(needed, reserve);

        clip += loadAmount;
        reserve -= loadAmount;

        SyncAmmoToSlot();
        isReloading = false;
    }

    // ===============================
    // AMMO / SLOT SYNC
    // ===============================
    private void SyncAmmoToSlot()
    {
        slotManager?.SetAmmo(slotManager.activeSlotIndex, clip, reserve);
    }

    public void LoadFromSlot(int slot)
    {
        if (slotManager == null) return;

        WeaponData data = slotManager.GetEquippedWeapon(slot);
        if (data == null) return;

        weaponData = data;

        clipSize = weaponData.clipSize;

        var ammo = slotManager.GetAmmo(slot);
        clip = ammo.clip;
        reserve = ammo.reserve;
    }

    public void SetWeapon(WeaponData data, int clipAmount, int reserveAmount)
    {
        weaponData = data;

        clipSize = data.clipSize;
        clip = clipAmount;
        reserve = reserveAmount;

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
    }

    // ===============================
    // MUZZLE FLASH
    // ===============================
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

        muzzleFlash.intensity = muzzleFlashIntensity;
        muzzleFlash.enabled = true;

        yield return new WaitForSeconds(muzzleFlashDuration);

        muzzleFlash.enabled = false;
        muzzleFlash.intensity = original;
    }

    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null)
            audioSource.PlayOneShot(emptyClipSound);
    }
}
