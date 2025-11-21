using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    private GameObject currentModel;

    private bool isShootingHeld = false;
    private bool isAiming = false;
    private bool isReloading = false;

    private PlayerControls controls;
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

    private float nextTimeToFire = 0f;

    // ---------------------------------------------------------
    // UNITY
    // ---------------------------------------------------------
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

    private void OnEnable()
    {
        controls.Gameplay.Enable();

        controls.Gameplay.Shoot.performed += OnShootPerformed;
        controls.Gameplay.Shoot.canceled += OnShootCanceled;

        controls.Gameplay.Reload.performed += OnReload;

        controls.Gameplay.Weapon1.performed += ctx => slotManager?.SwitchSlot(0);
        controls.Gameplay.Weapon2.performed += ctx => slotManager?.SwitchSlot(1);
        controls.Gameplay.Weapon3.performed += ctx => slotManager?.SwitchSlot(2);

        controls.Gameplay.Melee.performed += OnMelee;

        controls.Gameplay.ADS.performed += ctx => StartADS();
        controls.Gameplay.ADS.canceled += ctx => StopADS();

        isReloading = false;
    }

    private void OnDisable()
    {
        controls.Gameplay.Shoot.performed -= OnShootPerformed;
        controls.Gameplay.Shoot.canceled -= OnShootCanceled;

        controls.Gameplay.Reload.performed -= OnReload;
        controls.Gameplay.Melee.performed -= OnMelee;

        controls.Gameplay.Disable();
    }

    private void Update()
    {
        Debug.Log($"held={isShootingHeld} auto={weaponData?.isAutomatic}");
        if (weaponData == null) return;

        // Otomatik ateş
        if (weaponData.isAutomatic && isShootingHeld)
            Shoot();
    }

    // ---------------------------------------------------------
    // INPUT EVENTS
    // ---------------------------------------------------------
    private void OnShootPerformed(InputAction.CallbackContext ctx)
    {
        if (weaponData == null) return;
        isShootingHeld = true;

        if (!weaponData.isAutomatic)
            Shoot();
    }

    private void OnShootCanceled(InputAction.CallbackContext ctx)
    {
        if (weaponData == null) return;
        isShootingHeld = false;
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (isReloading || weaponData == null) return;
        if (reserve <= 0 || clip >= clipSize) return;

        StartCoroutine(Reload());
    }

    private void OnMelee(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        MeleeAttack();
    }

    // ---------------------------------------------------------
    // SHOOTING
    // ---------------------------------------------------------
    public void Shoot()
    {
        if (PauseMenu.IsPaused || isReloading || Time.time < nextTimeToFire)
            return;

        if (weaponData == null)
        {
            Debug.LogWarning("WeaponData NULL!");
            return;
        }

        if (weaponData.clipSize <= 0)
        {
            MeleeAttack();
            return;
        }

        RangedAttack();
    }

    private void RangedAttack()
    {
        if (clip <= 0)
        {
            PlayEmptyClipSound();
            return;
        }

        float cooldown =
            weaponData.isShotgun ? weaponData.shotgunCooldown :
            weaponData.isSniper ? weaponData.sniperCooldown :
            (1f / weaponData.fireRate);

        nextTimeToFire = Time.time + cooldown;

        clip--;
        SyncAmmoToSlot();

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        if (animator != null)
            animator.SetTrigger("Shoot");

        if (bulletPrefab != null && firePoint != null)
        {
            if (weaponData.isShotgun)
                FireShotgunPellets();
            else
                FireSingleBullet();

            TryMuzzleFlash();
        }
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

    // ---------------------------------------------------------
    // ADS
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // MELEE
    // ---------------------------------------------------------
    private void MeleeAttack()
    {
        nextTimeToFire = Time.time + (1f / weaponData.fireRate);
        Physics2D.OverlapCircleAll(firePoint.position, weaponData.attackRange, enemyLayer);
    }

    // ---------------------------------------------------------
    // RELOAD
    // ---------------------------------------------------------
    private IEnumerator Reload()
    {
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

    // ---------------------------------------------------------
    // SLOT / MODEL SYNC
    // ---------------------------------------------------------
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
        isShootingHeld = false;
        weaponData = data;
        clipSize = data.clipSize;
        clip = clipAmount;
        reserve = reserveAmount;
        isShootingHeld = false;

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
        else
        {
            Debug.LogError("Weapon prefab is NULL!");
        }
    }
    public void ResetShootHold()
{
    isShootingHeld = false;
}


    // ---------------------------------------------------------
    // MUZZLE FLASH
    // ---------------------------------------------------------
    private void TryMuzzleFlash()
    {
        if (muzzleFlash == null || firePoint == null)
            return;

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
