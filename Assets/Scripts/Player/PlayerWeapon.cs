using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    // -----------------------------
    // FIELDS
    // -----------------------------
    private GameObject currentModel;
    private PlayerControls controls;
    private bool isAiming = false;

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

    [Header("Animal Flee Settings")]
    public float gunshotHearingRadius = 12f;
    public float gunshotFleeDuration = 3f;
    public float gunshotFleeMultiplier = 1.8f;

    private AudioSource audioSource;
    private Animator animator;

    private int clip = 0;
    private int reserve = 0;
    private int clipSize = 0;

    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    private WeaponSlotManager slotManager;

    // -----------------------------
    // UNITY LIFECYCLE
    // -----------------------------
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        controls = new PlayerControls();   // MapToggle tarzı input

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;
    }

    void Start()
    {
        slotManager = WeaponSlotManager.Instance;

        if (slotManager != null)
        {
            LoadFromSlot(slotManager.activeSlotIndex);
        }
        else
        {
            Debug.LogError("[PlayerWeapon] WeaponSlotManager.Instance = null!");
        }
    }

    void OnEnable()
    {
        isReloading = false;
        if (muzzleFlash != null)
            muzzleFlash.enabled = false;

        controls.Gameplay.Enable();

        // Input event bağlama
        controls.Gameplay.Shoot.performed   += OnShoot;
        controls.Gameplay.Reload.performed  += OnReload;

        controls.Gameplay.Weapon1.performed += OnWeapon1;
        controls.Gameplay.Weapon2.performed += OnWeapon2;
        controls.Gameplay.Weapon3.performed += OnWeapon3;

        controls.Gameplay.Melee.performed   += OnMelee;

        controls.Gameplay.ADS.performed     += OnADSPerformed;
        controls.Gameplay.ADS.canceled      += OnADSCanceled;
    }

    void OnDisable()
    {
        controls.Gameplay.Shoot.performed   -= OnShoot;
        controls.Gameplay.Reload.performed  -= OnReload;

        controls.Gameplay.Weapon1.performed -= OnWeapon1;
        controls.Gameplay.Weapon2.performed -= OnWeapon2;
        controls.Gameplay.Weapon3.performed -= OnWeapon3;

        controls.Gameplay.Melee.performed   -= OnMelee;

        controls.Gameplay.ADS.performed     -= OnADSPerformed;
        controls.Gameplay.ADS.canceled      -= OnADSCanceled;

        controls.Gameplay.Disable();

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;
    }

    // -----------------------------
    // INPUT CALLBACKS
    // -----------------------------
    private void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Shoot();
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!isReloading && reserve > 0 && weaponData != null && weaponData.clipSize > 0)
            StartCoroutine(Reload());
    }

    private void OnWeapon1(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (WeaponSlotManager.Instance != null)
            WeaponSlotManager.Instance.SwitchSlot(0);
    }

    private void OnWeapon2(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (WeaponSlotManager.Instance != null)
            WeaponSlotManager.Instance.SwitchSlot(1);
    }

    private void OnWeapon3(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (WeaponSlotManager.Instance != null)
            WeaponSlotManager.Instance.SwitchSlot(2);
    }

    private void OnMelee(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        MeleeAttack();
    }

    private void OnADSPerformed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        StartADS();
    }

    private void OnADSCanceled(InputAction.CallbackContext ctx)
    {
        StopADS();
    }

    // -----------------------------
    // SHOOT
    // -----------------------------
    public void Shoot()
    {
        Debug.Log("SHOOT CALLED");

        if (PauseMenu.IsPaused || isReloading || Time.time < nextTimeToFire)
            return;

        if (weaponData == null)
        {
            Debug.LogWarning("[PlayerWeapon] weaponData null, Shoot iptal.");
            return;
        }

        if (weaponData.clipSize <= 0)
            MeleeAttack();
        else
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

        if (Time.time < nextTimeToFire)
            return;

        nextTimeToFire = Time.time + cooldown;

        // reduce ammo
        clip--;
        SyncAmmoToSlot();

        // sound
        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // emit sound (enemies)
        SoundEmitter.EmitSound(transform.position, 7f);

        // animals flee
        try
        {
            AnimalSoundEmitter.EmitSound(
                (Vector2)(firePoint ? firePoint.position : transform.position),
                gunshotHearingRadius,
                gunshotFleeDuration,
                gunshotFleeMultiplier
            );
        }
        catch { }

        // animator
        if (animator != null)
            animator.SetTrigger("Shoot");

        // bullets
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

        var b = bullet.GetComponent<WeaponBullet>();
        if (b != null)
        {
            b.damage = weaponData.damage;
            b.owner = this.transform;
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

            var b = bullet.GetComponent<WeaponBullet>();
            if (b != null)
            {
                b.damage = weaponData.damage;
                b.owner = this.transform;
                b.weaponType = weaponData.weaponType;
                b.knockbackForce = weaponData.knockbackForce;
                b.knockbackDuration = weaponData.knockbackDuration;
            }
        }
    }

    // -----------------------------
    // ADS
    // -----------------------------
    private void StartADS()
    {
        isAiming = true;
        if (animator != null)
            animator.SetBool("ADS", true);
        // İstersen kamera zoom burada
    }

    private void StopADS()
    {
        isAiming = false;
        if (animator != null)
            animator.SetBool("ADS", false);
    }

    // -----------------------------
    // MELEE
    // -----------------------------
    private void MeleeAttack()
    {
        if (weaponData == null) return;

        nextTimeToFire = Time.time + (1f / weaponData.fireRate);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            firePoint.position,
            weaponData.attackRange,
            enemyLayer
        );

        // TODO: enemy damage logic
    }

    // -----------------------------
    // RELOAD
    // -----------------------------
    public IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (weaponData == null || weaponData.clipSize <= 0) yield break;
        if (reserve <= 0) yield break;

        isReloading = true;

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = clipSize - clip;
        int toLoad = Mathf.Min(needed, reserve);

        clip += toLoad;
        reserve -= toLoad;

        SyncAmmoToSlot();
        isReloading = false;
    }

    // -----------------------------
    // AMMO SYNC
    // -----------------------------
    private void SyncAmmoToSlot()
    {
        if (slotManager == null) return;

        int slot = slotManager.activeSlotIndex;
        slotManager.SetAmmo(slot, clip, reserve);
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

    // -----------------------------
    // MUZZLE FLASH
    // -----------------------------
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
        muzzleFlashCo = null;
    }

    // -----------------------------
    // UTILS
    // -----------------------------
    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null)
            audioSource.PlayOneShot(emptyClipSound);
    }

    // WeaponSlotManager → handler.SetWeapon ile çağrılır
    public void SetWeapon(WeaponData data, int clipAmount, int reserveAmount)
    {
        // 1) Data ata
        weaponData = data;
        Debug.Log($"[PlayerWeapon] SetWeapon → {data.itemName}");

        // 2) Ammo ayarla
        clipSize = data.clipSize;
        clip = clipAmount;
        reserve = reserveAmount;

        // 3) Eski modeli sil
        if (currentModel != null)
            Destroy(currentModel);

        // 4) Yeni prefabı oluştur
        if (weaponData.prefab != null)
        {
            currentModel = Instantiate(weaponData.prefab, transform);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            // Prefab içinden FirePoint bul (varsa)
            Transform fp = currentModel.transform.Find("FirePoint");
            if (fp != null)
                firePoint = fp;
        }
        else
        {
            Debug.LogError("❌ WeaponData.prefab boş! Silah spawn edilemedi!");
        }
    }

    public int GetCurrentAmmoInClip() => clip;
    public bool IsReloading() => isReloading;
}
