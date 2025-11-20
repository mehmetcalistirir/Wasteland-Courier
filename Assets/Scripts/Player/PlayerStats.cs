using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerStats : MonoBehaviour
{

    // --- Stamina ---
    [Header("Stamina Ayarları")]
    public float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    public float staminaDrainRate = 15f;  // koşarken azalır
    public float staminaRegenWalk = 8f;   // yürürken artar
    public float staminaRegenIdle = 18f;  // dururken artar

    public float GetStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public void ModifyStamina(float amount) => currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
    public bool HasStamina() => currentStamina > 0f;
    public void ResetStamina() => currentStamina = maxStamina;

    // --- Sağlık ---
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

    // --- Hareket/Envanter ---
    [Header("Hareket/Envanter")]
    public float moveSpeed = 5f;
    public int gold = 10;

    // --- Açlık / UI ---
    [Header("Açlık")]
    public int maxHunger = 100;
    public int currentHunger;
    public float hungerDecreaseInterval = 5f;
    public int hungerDecreaseAmount = 1;
    private float hungerTimer;

    [Header("Açlık UI")]
    public TextMeshProUGUI hungerText;

    // Yemeklerin sadece AÇLIK etkileri (SAĞLIK HEAL YOK!)
    public int hungerOnRawMeatUse = 10;
    public int hungerOnCookedMeatUse = 30;
    public int hungerOnHerbUse = 0;

    // --- Açlığa bağlı Doğal İyileşme / Açlıktan Hasar ---
    [Header("Doğal İyileşme (Açlığa bağlı)")]
    public bool enableHungerRegen = true;
    public float hungerRegenThreshold = 80f;   // 80 üstü tok sayılır
    public float healthRegenRate = 3f;         // her tikte kaç HP
    public float healthRegenInterval = 1f;     // saniye
    private float healthRegenTimer = 0f;

    private float starvationTickInterval = 2f; // açlıktan hasar aralığı
    private float starvationTimer = 0f;

    // --- Ses ---
    [Header("Ses")]
    public AudioClip hurtClip;
    private AudioSource audioSource;

    // --- XP/Level ---
    public int currentXP = 0;
    public int level = 1;
    public int skillPoints = 0;
    public int xpToNextLevel = 100;
    public delegate void OnLevelUp();
    public event OnLevelUp onLevelUp;

    // ---- ItemData Referansları ----
    [Header("Item References")]
    public ItemData cookedMeatSO;
    public ItemData rawMeatSO;
    public ItemData herbSO;

    // --- (Diğer) Scriptable Referanslar (dokunulmadı) ---
    public GenericItemData stoneSO, ammo9mmSO, BluePrintSO, CookedMeatSO, DeerHideSO,
                            MeatSO, RabbitHideSO, ScrapSO, WoodSO;

    

    // --- Unity Döngüsü ---
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        AudioManager.Instance?.RouteToSFX(audioSource);
    }

    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;

        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleHunger();
        HandleHungerRegen();
        HandleStarvation();

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryConsumeFood(); // sadece AÇLIK kazanır, sağlık vermez

        // Açlık UI güncelle
        if (hungerText != null)
        {
            hungerText.text = $"Açlık: {currentHunger}/{maxHunger}";
            if (currentHunger > 60) hungerText.color = Color.green;
            else if (currentHunger > 30) hungerText.color = Color.yellow;
            else hungerText.color = Color.red;
        }
    }

    // --- Hasar / Heal ---
    public bool IsAlive() => currentHealth > 0;

    public void TakeDamage(int amount)
    {
        if (!IsAlive()) return;
        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (hurtClip != null && audioSource != null)
            audioSource.PlayOneShot(hurtClip);

        if (currentHealth <= 0) Die();

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
        Debug.Log("💀 Oyuncu öldü!");
        onDeath?.Invoke();
    }

    // --- Açlık Mekaniği ---
    void HandleHunger()
    {
        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            currentHunger = Mathf.Max(0, currentHunger - hungerDecreaseAmount);
            hungerTimer = hungerDecreaseInterval;
        }
    }

    void HandleHungerRegen()
    {
        if (!enableHungerRegen) return;
        if (currentHunger < hungerRegenThreshold) return;  // tok değil
        if (currentHealth >= maxHealth) return;            // zaten full

        healthRegenTimer += Time.deltaTime;
        if (healthRegenTimer >= healthRegenInterval)
        {
            Heal(Mathf.RoundToInt(healthRegenRate));
            healthRegenTimer = 0f;
        }
    }

    void HandleStarvation()
    {
        if (currentHunger > 0 || currentHealth <= 0) { starvationTimer = 0f; return; }

        // açlık 0 ise periyodik can kaybı
        starvationTimer += Time.deltaTime;
        if (starvationTimer >= starvationTickInterval)
        {
            TakeDamage(1);
            starvationTimer = 0f;
        }
    }

    // --- Envanter Köprü ---
    public void AddResource(ItemData item, int amount)
    {
        if (item != null)
            Inventory.Instance.TryAdd(item, amount);
    }

    public bool RemoveResource(ItemData item, int amount)
    {
        return item != null && Inventory.Instance.TryConsume(item, amount);
    }

    public int GetResourceAmount(ItemData item)
    {
        return item != null ? Inventory.Instance.GetTotalCount(item) : 0;
    }

    // --- Yemek Tüketimi (Sadece açlık ekler; SAĞLIK YOK) ---
    private void TryConsumeFood()
    {
        if (Inventory.Instance.HasEnough(cookedMeatSO, 1))
        {
            Inventory.Instance.TryConsume(cookedMeatSO, 1);
            GainHunger(hungerOnCookedMeatUse);
            Debug.Log("🍗 Pişmiş et yendi!");
            return;
        }

        if (Inventory.Instance.HasEnough(rawMeatSO, 1))
        {
            Inventory.Instance.TryConsume(rawMeatSO, 1);
            GainHunger(hungerOnRawMeatUse);
            Debug.Log("🥩 Çiğ et yendi!");
            return;
        }

        if (Inventory.Instance.HasEnough(herbSO, 1))
        {
            Inventory.Instance.TryConsume(herbSO, 1);
            GainHunger(hungerOnHerbUse);
            Debug.Log("🌿 Ot yendi!");
            return;
        }
    }

    private void GainHunger(int amount)
    {
        if (amount > 0)
            currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
    }
    public void RefreshHealthUI()
{
    onHealthChanged?.Invoke(currentHealth, maxHealth);
}

}