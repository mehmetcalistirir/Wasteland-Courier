using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerStats : MonoBehaviour
{
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
    }

    public void ResetStamina() => currentStamina = maxStamina;
    public bool HasStamina() => currentStamina > 0;


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


    // ------------------------------
    //     HAREKET / ENVANTER
    // ------------------------------
    [Header("Hareket/Envanter")]
    public float moveSpeed = 5f;
    public int gold = 10;


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
    //       XP / LEVEL
    // ------------------------------
    public int currentXP = 0;
    public int level = 1;
    public int skillPoints = 0;
    public int xpToNextLevel = 100;

    public delegate void OnLevelUp();
    public event OnLevelUp onLevelUp;


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
    }

    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;

        currentStamina = maxHunger;      // başlangıçta max stamina = açlık
        maxStamina = maxHunger;

        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
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
    }


    // ============================================================
    //                 HUNGER SYSTEM
    // ============================================================

    void HandleHungerSystem()
    {
        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            currentHunger = Mathf.Max(0, currentHunger - hungerDecreaseAmount);
            hungerTimer = hungerDecreaseInterval;
        }
    }


    // ============================================================
    //     MAX STAMINA = CURRENT HUNGER  (PEAK SISTEMI)
    // ============================================================

    void UpdateStaminaByHunger()
    {
        maxStamina = currentHunger;

        if (currentStamina > maxStamina)
            currentStamina = maxStamina;
    }


    // ============================================================
    //       STAMINA REGEN RATE = HUNGER LEVEL
    // ============================================================

    void UpdateStaminaRegen()
    {
        if (isRunning) return;  // koşarken regen yok

        float hungerPercent = (float)currentHunger / maxHunger;
        float regen = staminaRegenIdle;

        if (hungerPercent >= 0.70f)
            regen *= 1.5f;          // çok tok → hızlı regen
        else if (hungerPercent >= 0.40f)
            regen *= 1f;            // normal
        else if (hungerPercent >= 0.20f)
            regen *= 0.5f;          // yorgun
        else
            regen *= 0.2f;          // bitkin

        ModifyStamina(regen * Time.deltaTime);
    }


    // ============================================================
    //       HEALTH = HUNGER STATE
    // ============================================================

    void UpdateHealthByHunger()
    {
        if (currentHealth <= 0) return;

        float hungerPercent = (float)currentHunger / maxHunger;

        // CAN YENİLEME
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

        // CAN KAYBETME (açlık çok düşük)
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
            starvationTimer = 0f;
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
            currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
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

}
