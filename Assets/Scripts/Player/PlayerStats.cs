using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Breathing Sound")]
public AudioClip heavyBreathingLoop;

[Tooltip("Stamina bu değerin ALTINA düşerse nefes sesi başlar")]
public float breathingStaminaThreshold = 25f;

public float breathingVolume = 0.8f;
public float breathingFadeSpeed = 2f;

private AudioSource breathingSource;
private bool isBreathingActive = false;
    // ------------------------------
    //   STAMINA (Açlığa Bağlı)
    // ------------------------------
    [Header("Stamina Ayarları")]
    public float maxStamina = 100f;            // ARTIK: maxHunger ile eşitlenecek
    [SerializeField] private float currentStamina;
    public float staminaDrainRate = 15f;       // koşarken azalır
    public float staminaRegenIdle = 18f;       // dururken artar

    private bool isRunning = false;            // Movement scriptinden bağlanacak (setter fonk ekledim)
    private bool isMoving = false;

    public float GetStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;

    public void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        UpdateStaminaUI();
    }

    public void ResetStamina() => currentStamina = maxStamina;
    public bool HasStamina() => currentStamina > 0;

    [Header("Açlık Koşu Çarpanı")]
    public float runningHungerMultiplier = 1.5f; // koşarken açlık 2 hızlı azalır
    [Header("Stamina UI")]
    public Slider staminaSlider;




    // ------------------------------
    //            SAĞLIK
    // ------------------------------
    [Header("Sağlık")]
    public int maxHealth = 100;
    public int currentHealth;
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    [Header("UI")]
    [SerializeField] private PlayerHealthUI healthUI;
    public delegate void OnHealthChanged(int current, int max);
    public event OnHealthChanged onHealthChanged;

    [Header("Blood Vignette")]
    [SerializeField] private BloodVignetteUI bloodVignetteUI;


    // ------------------------------
    //     HAREKET / ENVANTER
    // ------------------------------
    [Header("Hareket/Envanter")]
    public float moveSpeed = 5f;


    // ------------------------------
    //            AÇLIK
    // ------------------------------
    [Header("Açlık")]
    public int maxHunger = 100;
    public int currentHunger;
    public float hungerDecreaseInterval = 5f;
    public int hungerDecreaseAmount = 1;
    private float hungerTimer;

    [Header("Açlık UI")]
    public TextMeshProUGUI hungerText;

    public int hungerOnRawMeatUse = 10;
    public int hungerOnCookedMeatUse = 30;
    public int hungerOnHerbUse = 0;


    // ------------------------------
    //   AÇLIK → SAĞLIK/YORGUNLUK
    // ------------------------------
    [Header("Açlık Bazlı Sağlık Sistemi")]
    public float highHungerThresholdPercent = 0.70f;   // %70 üstü can yeniler
    public float lowHungerThresholdPercent = 0.10f;    // %10 altı can düşer

    public float healthRegenRate = 1f;         // saniyede +1
    public float healthRegenInterval = 1f;
    private float healthRegenTimer;

    private float starvationTickInterval = 1.5f;   // açlık bitince can azalması
    private float starvationTimer;



    // ------------------------------
    //        SES
    // ------------------------------
    [Header("Ses")]
    public AudioClip hurtClip;
    private AudioSource audioSource;


    // ------------------------------
    // ITEM REFERANSLARI
    // ------------------------------
    [Header("Item References")]
    public ItemData cookedMeatSO;
    public ItemData rawMeatSO;
    public ItemData herbSO;
    public float staminaRegenWalk = 8f;



    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        AudioManager.Instance?.RouteToSFX(audioSource);
        breathingSource = gameObject.AddComponent<AudioSource>();
breathingSource.clip = heavyBreathingLoop;
breathingSource.loop = true;
breathingSource.playOnAwake = false;
breathingSource.spatialBlend = 0f; // 2D
breathingSource.volume = 0f;

AudioManager.Instance?.RouteToSFX(breathingSource);

    }

    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;

        currentStamina = maxHunger;      // başlangıçta max stamina = açlık
        maxStamina = maxHunger;

        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        onHealthChanged += UpdateBloodVignette;
        UpdateBloodVignette(currentHealth, maxHealth);

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

    }

    void Update()
    {
        HandleHungerSystem();
        UpdateStaminaByHunger();
        UpdateStaminaRegen();
        UpdateHealthByHunger();


        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryConsumeFood();

        UpdateHungerUI();
        UpdateStaminaUI();
        if (heavyBreathingLoop == null || breathingSource == null)
        return;

    // 🔴 STAMINA DÜŞÜK → NEFES BAŞLASIN
    if (currentStamina <= breathingStaminaThreshold)
    {
        if (!isBreathingActive)
        {
            isBreathingActive = true;
            breathingSource.volume = 0f;

            if (!breathingSource.isPlaying)
                breathingSource.Play();
        }
    }
    // 🟢 STAMINA YETERLİ → NEFES KESİLSİN
    else
    {
        isBreathingActive = false;
    }

    // 🎚️ FADE
    if (isBreathingActive)
    {
        breathingSource.volume = Mathf.MoveTowards(
            breathingSource.volume,
            breathingVolume,
            breathingFadeSpeed * Time.deltaTime
        );
    }
    else
    {
        breathingSource.volume = Mathf.MoveTowards(
            breathingSource.volume,
            0f,
            breathingFadeSpeed * Time.deltaTime
        );

        if (breathingSource.volume <= 0.01f && breathingSource.isPlaying)
            breathingSource.Stop();
    }

    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider == null) return;

        // Full ise gizle
        if (currentStamina >= maxStamina)
        {
            staminaSlider.gameObject.SetActive(false);
        }
        else
        {
            staminaSlider.gameObject.SetActive(true);
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }




    // ============================================================
    //                 HUNGER SYSTEM
    // ============================================================

    void HandleHungerSystem()
    {
        float interval = hungerDecreaseInterval;

        // Koşuyorsa açlık daha hızlı azalır
        if (isRunning)
            interval /= runningHungerMultiplier;

        hungerTimer -= Time.deltaTime;

        if (hungerTimer <= 0f)
        {
            currentHunger = Mathf.Max(0, currentHunger - hungerDecreaseAmount);
            hungerTimer = interval;
        }
    }




    // ============================================================
    //     MAX STAMINA = CURRENT HUNGER  (PEAK SISTEMI)
    // ============================================================

    void UpdateStaminaByHunger()
    {
        maxStamina = currentHunger;

        // currentStamina fazla kaldıysa otomatik düşür
        if (currentStamina > maxStamina)
            currentStamina = maxStamina;
    }



    // ============================================================
    //       STAMINA REGEN RATE = HUNGER LEVEL
    // ============================================================

    void UpdateStaminaRegen()
    {
        float hungerPercent = (float)currentHunger / maxHunger;

        // Açlık → regen katsayısı
        float hungerMult;
        if (hungerPercent >= 0.70f)
            hungerMult = 1.5f;   // tok → hızlı regen
        else if (hungerPercent >= 0.40f)
            hungerMult = 1f;     // normal
        else if (hungerPercent >= 0.20f)
            hungerMult = 0.5f;   // yorgun
        else
            hungerMult = 0.2f;   // bitkin

        // KOŞARKEN → sadece STAMINA AZALIR
        if (isRunning && isMoving)
        {
            ModifyStamina(-staminaDrainRate * Time.deltaTime);
            return;
        }

        // YÜRÜRKEN → az regen
        if (isMoving)
        {
            float regen = staminaRegenWalk * hungerMult;
            ModifyStamina(regen * Time.deltaTime);
        }
        // HİÇ HAREKET ETMEZKEN → daha fazla regen
        else
        {
            float regen = staminaRegenIdle * hungerMult;
            ModifyStamina(regen * Time.deltaTime);
        }
    }




    // ============================================================
    //       HEALTH = HUNGER STATE
    // ============================================================

    void UpdateHealthByHunger()
    {
        if (currentHealth <= 0) return;

        float hungerPercent = (float)currentHunger / maxHunger;

        // Tok → Can yenilenir
        if (hungerPercent >= highHungerThresholdPercent)
        {
            healthRegenTimer += Time.deltaTime;

            if (healthRegenTimer >= healthRegenInterval && currentHealth < maxHealth)
            {
                Heal(Mathf.RoundToInt(healthRegenRate));
                healthRegenTimer = 0f;
            }

            return;
        }


        // Çok aç → Can düşer
        if (hungerPercent <= lowHungerThresholdPercent)
        {
            starvationTimer += Time.deltaTime;

            if (starvationTimer >= starvationTickInterval)
            {
                TakeDamage(1);
                starvationTimer = 0f;
            }
        }
        else
        {
            starvationTimer = 0f; // güvenli reset
        }
    }




    // ============================================================
    //                   FOOD CONSUME
    // ============================================================

    private void TryConsumeFood()
    {
        if (Inventory.Instance.HasEnough(cookedMeatSO, 1))
        {
            Inventory.Instance.TryConsume(cookedMeatSO, 1);
            GainHunger(hungerOnCookedMeatUse);
            return;
        }

        if (Inventory.Instance.HasEnough(rawMeatSO, 1))
        {
            Inventory.Instance.TryConsume(rawMeatSO, 1);
            GainHunger(hungerOnRawMeatUse);
            return;
        }

        if (Inventory.Instance.HasEnough(herbSO, 1))
        {
            Inventory.Instance.TryConsume(herbSO, 1);
            GainHunger(hungerOnHerbUse);
            return;
        }
    }

    private void GainHunger(int amount)
    {
        if (amount > 0)
            currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
    }



    // ============================================================
    //                      UI UPDATE
    // ============================================================

    void UpdateHungerUI()
    {
        if (hungerText == null) return;

        hungerText.text = $"Açlık: {currentHunger}/{maxHunger}";

        if (currentHunger > 60) hungerText.color = Color.green;
        else if (currentHunger > 30) hungerText.color = Color.yellow;
        else hungerText.color = Color.red;
    }


    // ============================================================
    //          SAĞLIK FONKSİYONLARI
    // ============================================================

    public bool IsAlive() => currentHealth > 0;

    public void TakeDamage(int amount)
{
    if (!IsAlive()) return;
    if (Time.time - lastDamageTime < damageCooldown) return;

    lastDamageTime = Time.time;
    currentHealth = Mathf.Max(0, currentHealth - amount);

    onHealthChanged?.Invoke(currentHealth, maxHealth);

    // 🔴 KAN FLASH
    bloodVignetteUI?.OnDamageFlash();

    if (hurtClip != null)
        audioSource?.PlayOneShot(hurtClip);

    if (currentHealth <= 0)
        Die();

    DamagePopupManager.Instance?.SpawnPopup(transform.position, amount);
}


    public void Heal(int amount)
    {
        if (!IsAlive() || amount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("💀 Player died");
        onDeath?.Invoke();
    }

    void OnDestroy()
{
    onHealthChanged -= UpdateBloodVignette;
}

    // ============================================================
    //      Dış Scriptlerden Hareket Bilgisi Alma
    // ============================================================

    public void SetMovementState(bool moving, bool running)
    {
        isMoving = moving;
        isRunning = running;
    }


    public void RefreshHealthUI()
    {
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void UpdateBloodVignette(int current, int max)
{
    if (bloodVignetteUI == null) return;

    bloodVignetteUI.health01 = (float)current / max;
}



}
