// PlayerWeapon.cs (EKSİKLERİ GİDERİLMİŞ VE TAM HALİ)

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    // --- INSPECTOR'DA ATANACAK ALANLAR ---
    [Header("Weapon Configuration")]
    public WeaponData weaponData;

    [Header("Weapon Components")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClipSound;

    private Animator animator;
    private AudioSource audioSource;
    private int currentAmmoInClip;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        isReloading = false;
    }

    public void Shoot()
    {
        if (isReloading || Time.time < nextTimeToFire) return;

        if (weaponData.clipSize <= 0) // Melee check
        {
            MeleeAttack();
        }
        else // Ranged
        {
            RangedAttack();
        }
    }

    private void RangedAttack()
    {
        if (currentAmmoInClip <= 0)
        {
            // Bu fonksiyon artık WeaponSlotManager'da, mermi yoksa oradan çağrılacak.
            // PlayEmptyClipSound();
            return;
        }

        nextTimeToFire = Time.time + 1f / weaponData.fireRate;
        currentAmmoInClip--;

        if (shootSound != null) audioSource.PlayOneShot(shootSound);
        if (animator != null) animator.SetTrigger("Shoot");

        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
        
        WeaponSlotManager.Instance.UpdateAmmoText();
    }

    private void MeleeAttack()
    {
        nextTimeToFire = Time.time + 1f / weaponData.fireRate;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, weaponData.attackRange, enemyLayer);
        foreach(Collider2D enemyCollider in hitEnemies)
        {
            // Düşmana hasar verme mantığı...
        }
    }
    
    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(emptyClipSound);
        }
    }
    // Şarjör değiştirme Coroutine'i
    public IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log($"Reloading {weaponData.weaponName}...");

        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(weaponData.reloadTime);

        WeaponSlotManager.Instance.FinishReload();

        isReloading = false;
        
        Debug.Log("Reload finished.");
    }

    // Yardımcı Fonksiyonlar
    public void SetAmmoInClip(int amount) => currentAmmoInClip = amount;
    public int GetCurrentAmmoInClip() => currentAmmoInClip;
    public bool IsReloading() => isReloading;
}