// PlayerWeapon.cs (MUZZLE FLASH + SHOTGUN + HAYVAN KAÇIRMA ENTEGRE)
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using WeaponInstance = CaravanInventory.WeaponInstance;  // en başa ekle

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{

    [Header("Durability")]
    [SerializeField] private int durabilityCostPerShot = 1;
    [SerializeField] private int durabilityCostPerMelee = 1;

    private WeaponDurability durability;


    // --- INSPECTOR'DA ATANACAK ALANLAR ---
    [Header("Weapon Configuration")]
    public WeaponData weaponData;   // WeaponData içinde: isShotgun, pelletsPerShot, pelletSpreadAngle, shotgunCooldown beklenir.

    [Header("Weapon Components")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;

    [Header("Muzzle Flash")]
    [Tooltip("Namlu ucunda anlık parlayan Light2D.")]
    public Light2D muzzleFlash;
    [Tooltip("Parlama süresi (saniye).")]
    public float muzzleFlashDuration = 0.05f;
    [Tooltip("Parlama sırasında kullanılacak yoğunluk.")]
    public float muzzleFlashIntensity = 3f;

    private Coroutine muzzleFlashCo;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClipSound;

    private Animator animator;
    private AudioSource audioSource;
    private int currentAmmoInClip;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    [Header("Noise Settings")]
    [Tooltip("Silah sesiyle hayvanların duyacağı menzil.")]
    public float gunshotHearingRadius = 12f;
    [Tooltip("Hayvanların kaçma süresi (sn).")]
    public float gunshotFleeDuration = 3f;
    [Tooltip("Kaçış hız çarpanı.")]
    public float gunshotFleeMultiplier = 1.8f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        if (muzzleFlash != null) muzzleFlash.enabled = false;

        // Durability component'i bu GO'da değilse parent'ta da arayalım
        durability = GetComponent<WeaponDurability>() ?? GetComponentInParent<WeaponDurability>();
    }

    private void ApplyDurabilityOnUse(bool isRanged)
    {
        if (durability == null) return;                 // component yoksa sessizce geç
        durability.LoseDurability(isRanged ? durabilityCostPerShot : durabilityCostPerMelee);
    }


    private void OnEnable()
    {
        isReloading = false;
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    private void OnDisable()
    {
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    public void Shoot()
    {
        if (PauseMenu.IsPaused || isReloading || Time.time < nextTimeToFire) return;

        if (weaponData.clipSize <= 0) // Melee
            MeleeAttack();
        else
            RangedAttack();
    }



    private void RangedAttack()
    {
        if (currentAmmoInClip <= 0) return;

        int cost = weaponData.isShotgun ? Mathf.Max(2, durabilityCostPerShot) : durabilityCostPerShot;
        durability.LoseDurability(cost);


        // Shotgun'da sabit bekleme; diğerlerinde fireRate
       float cooldown =
    weaponData.isShotgun ? weaponData.shotgunCooldown :
    weaponData.isSniper  ? weaponData.sniperCooldown  :
    (1f / weaponData.fireRate);

    nextTimeToFire = Time.time + cooldown;   // <<— BUNU EKLE



        // Klasik shotgun davranışı: tetikte kaç saçma çıkarsa çıksın 1 mermi eksilt
        currentAmmoInClip--;

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // Düşmanları sese çek
        SoundEmitter.EmitSound(transform.position, 7f);

        // Hayvanları ürküt (projede mevcutsa)
        try
        {
            AnimalSoundEmitter.EmitSound(
                firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position,
                gunshotHearingRadius,
                gunshotFleeDuration,
                gunshotFleeMultiplier
            );
        }
        catch { /* AnimalSoundEmitter yoksa hatayı yut. */ }

        if (animator != null) animator.SetTrigger("Shoot");

        if (bulletPrefab != null && firePoint != null)
        {
            if (weaponData.isShotgun)
                FireShotgunPellets();
            else
                FireSingleBullet();

            // Muzzle flash
            TryMuzzleFlash();
        }

        WeaponSlotManager.Instance.UpdateAmmoText();

        ApplyDurabilityOnUse(true);

    }

    private void FireSingleBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var bulletScript = bullet.GetComponent<WeaponBullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = weaponData.damage;
            bulletScript.owner = this.transform;
            bulletScript.weaponType = weaponData.weaponType;
            bulletScript.knockbackForce = weaponData.knockbackForce;
            bulletScript.knockbackDuration = weaponData.knockbackDuration;
        }
    }

    private void FireShotgunPellets()
    {
        int pellets = Mathf.Max(weaponData.pelletsPerShot, 1);
        float spread = weaponData.pelletSpreadAngle;

        for (int i = 0; i < pellets; i++)
        {
            float t = (pellets == 1) ? 0f : i / (pellets - 1f);
            float angle = Mathf.Lerp(-spread, spread, t);

            Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, 0f, angle);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);

            var bulletScript = bullet.GetComponent<WeaponBullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = weaponData.damage;
                bulletScript.owner = this.transform;
                bulletScript.weaponType = weaponData.weaponType;
                bulletScript.knockbackForce = weaponData.knockbackForce;
                bulletScript.knockbackDuration = weaponData.knockbackDuration;
            }
        }
    }


    private void MeleeAttack()
    {
        nextTimeToFire = Time.time + 1f / weaponData.fireRate;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, weaponData.attackRange, enemyLayer);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // Düşmana hasar verme mantığı...
        }

        ApplyDurabilityOnUse(true);

    }

    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(emptyClipSound);
    }

    // Şarjör değiştirme Coroutine'i
    public IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log($"Reloading {weaponData.weaponName}...");

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        WeaponSlotManager.Instance.FinishReload();
        isReloading = false;

        Debug.Log("Reload finished.");
    }

    private void TryMuzzleFlash()
    {
        if (muzzleFlash == null || firePoint == null) return;

        // MuzzleLight'ı tam namlu ucuna taşı
        muzzleFlash.transform.position = firePoint.position;

        if (muzzleFlashCo != null) StopCoroutine(muzzleFlashCo);
        muzzleFlashCo = StartCoroutine(MuzzleFlashRoutine());
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        float originalIntensity = muzzleFlash.intensity;

        muzzleFlash.intensity = muzzleFlashIntensity;
        muzzleFlash.enabled = true;

        yield return new WaitForSeconds(muzzleFlashDuration);

        muzzleFlash.enabled = false;
        muzzleFlash.intensity = originalIntensity;
        muzzleFlashCo = null;
    }

    // Yardımcı Fonksiyonlar
    public void SetAmmoInClip(int amount) => currentAmmoInClip = amount;
    public int GetCurrentAmmoInClip() => currentAmmoInClip;
    public bool IsReloading() => isReloading;
}
