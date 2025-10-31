using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;




public class PlayerStats : MonoBehaviour
{

    // En üste alan ekle
    private Dictionary<WeaponPartType, int> weaponParts = new Dictionary<WeaponPartType, int>();

    public int GetWeaponPartCount(WeaponPartType part)
    {
        return weaponParts.ContainsKey(part) ? weaponParts[part] : 0;
    }

    public void ConsumeWeaponParts(List<PartRequirement> partsToConsume)
    {
        foreach (var p in partsToConsume)
        {
            if (weaponParts.ContainsKey(p.partType) && weaponParts[p.partType] >= p.amount)
            {
                weaponParts[p.partType] -= p.amount;
                WeaponPartsUI.Instance?.UpdatePartText(p.partType, weaponParts[p.partType]);
            }
        }
    }






    public int hungerOnRawMeatUse = 10;
    public int hungerOnCookedMeatUse = 30;
    public int hungerOnHerbUse = 0;

    [Header("Stamina Ayarları")]
    public float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    public float staminaDrainRate = 15f;  // koşarken azalır
    public float staminaRegenWalk = 8f;   // yürürken artar
    public float staminaRegenIdle = 18f;  // dururken artar

    [Header("Açlık UI")]
    public TextMeshProUGUI hungerText;

    [Header("Saglik")]
    public int maxHealth = 100;
    public int currentHealth;
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    public GenericItemData stoneSO;
    public GenericItemData ammo9mmSO;

    public GenericItemData BluePrintSO;

    public GenericItemData CookedMeatSO;

    public GenericItemData DeerHideSO;

    public GenericItemData MeatSO;

    public GenericItemData RabbitHideSO;

    public GenericItemData ScrapSO;

    public GenericItemData WoodSO;

    public WeaponItemData machinegunSO;

    public WeaponItemData pistolSO;

    public WeaponItemData shotgunSO;

    public WeaponItemData sniperSO;

    public WeaponItemData throwingSpearSO;

    public WeaponItemData bowSO;

    public WeaponItemData meeleSpearSO;

    public WeaponItemData meeleSwordSO;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    [Header("UI")]
    [SerializeField] private PlayerHealthUI healthUI;
    public delegate void OnHealthChanged(int current, int max);
    public event OnHealthChanged onHealthChanged;

    [Header("Hareket/Envanter")]
    public float moveSpeed = 5f;
    public int gold = 10;

    [Header("Aclik")]
    public int maxHunger = 100;
    public int currentHunger;
    public float hungerDecreaseInterval = 5f;
    public int hungerDecreaseAmount = 1;
    private float hungerTimer;

    [Header("Doğal İyileşme (Açlığa bağlı)")]
    public bool enableHungerRegen = true;
    public float hungerRegenThreshold = 80f;   // 80 üstü tok sayılır
    public float healthRegenRate = 3f;         // saniyede kaç HP yenilenecek
    public float healthRegenInterval = 1f;     // kaç saniyede bir yenileme olur
    private float healthRegenTimer = 0f;


    [Header("Ses")]
    public AudioClip hurtClip;
    private AudioSource audioSource;

    // XP/Level
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

    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        currentStamina = maxStamina;
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        AudioManager.Instance?.RouteToSFX(audioSource);
    }

    void Update()
    {
        HandleHunger();
        HandleHungerRegen();
        HandleStarvation();


        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryConsumeFood();

        // --- Açlık UI Güncellemesi ---
        if (hungerText != null)
        {
            hungerText.text = $"Açlık: {currentHunger}/{maxHunger}";

            // Renk geçişi: açlığa göre değişsin
            if (currentHunger > 60)
                hungerText.color = Color.green;
            else if (currentHunger > 30)
                hungerText.color = Color.yellow;
            else
                hungerText.color = Color.red;
        }

    }
    public float GetStamina()
    {
        return currentStamina;
    }
    public float GetMaxStamina()
    {
        return maxStamina;
    }
    public void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
    }
    public bool HasStamina()
    {
        return currentStamina > 0f;
    }
    public void ResetStamina()
    {
        currentStamina = maxStamina;
    }

    public void CollectWeaponPart(WeaponPartType part, int amountToCollect = 1)
    {
        if (!weaponParts.ContainsKey(part))
            weaponParts[part] = 0;

        weaponParts[part] += amountToCollect;
        WeaponPartsUI.Instance?.UpdatePartText(part, weaponParts[part]);
    }


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
        if (currentHunger < hungerRegenThreshold) return; // tok değil
        if (currentHealth >= maxHealth) return; // zaten full sağlık

        healthRegenTimer += Time.deltaTime;
        if (healthRegenTimer >= healthRegenInterval)
        {
            Heal(Mathf.RoundToInt(healthRegenRate)); // mevcut Heal fonksiyonunu kullan
            healthRegenTimer = 0f;
        }
    }

    void HandleStarvation()
    {
        if (currentHunger > 0) return;
        if (currentHealth <= 0) return;

        // Açlıktan yavaş can kaybı
        healthRegenTimer += Time.deltaTime;
        if (healthRegenTimer >= 2f) // 2 saniyede bir
        {
            TakeDamage(1);
            healthRegenTimer = 0f;
        }
    }



    // ==== Inventory Köprü ====
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

    // ==== Yemek ====
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
}
