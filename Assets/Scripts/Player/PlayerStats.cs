using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    // --------- AYARLAR ---------
    [Header("Use (F) Effects")]
    [Tooltip("F ile ÇİĞ et yenirken kaç HP iyileşsin?")]
    public int healOnRawMeatUse = 5;
    [Tooltip("F ile PİŞMİŞ et yenirken kaç HP iyileşsin?")]
    public int healOnCookedMeatUse = 15;
    [Tooltip("F ile HERB yenirken kaç HP iyileşsin?")]
    public int healOnHerbUse = 10;

    [Tooltip("ÇİĞ et yendiğinde kaç açlık puanı doysun?")]
    public int hungerOnRawMeatUse = 10;
    [Tooltip("PİŞMİŞ et yendiğinde kaç açlık puanı doysun?")]
    public int hungerOnCookedMeatUse = 30;
    [Tooltip("HERB yendiğinde kaç açlık puanı doysun? (İstemezsen 0 bırak)")]
    public int hungerOnHerbUse = 0;

    [Header("Health")]
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

    [Header("Move/Inventory")]
    public float moveSpeed = 5f;
    public int inventoryCapacity = 20;
    public int gold = 10;

    [Header("Hunger")]
    public int maxHunger = 100;
    public int currentHunger;
    public float hungerDecreaseInterval = 5f;
    public int hungerDecreaseAmount = 1;
    private float hungerTimer;

    [Header("Audio")]
    public AudioClip hurtClip;   // Inspector'dan atayacağın ses
    private AudioSource audioSource;

    // XP/Level
    public int currentXP = 0;
    public int level = 1;
    public int skillPoints = 0;
    public int xpToNextLevel = 100;
    public delegate void OnLevelUp();
    public event OnLevelUp onLevelUp;

    // Envanterler
    private Dictionary<string, int> resources = new Dictionary<string, int>();
    private HashSet<string> unlockedBlueprints = new HashSet<string>();
    private HashSet<WeaponPartType> collectedParts = new HashSet<WeaponPartType>();
    private Dictionary<WeaponPartType, int> weaponParts = new Dictionary<WeaponPartType, int>();

    // --------- YAŞAM DÖNGÜSÜ ---------
    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Sesleri AudioManager üzerinden miksere bağlamak istersen:
        AudioManager.Instance?.RouteToSFX(audioSource);
    }

    void Update()
    {
        HandleHunger();

        // F ile yemek yeme (Cooked -> Raw -> Herb sırasıyla dener)
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryConsumeFood();
        }
    }

    // --------- CAN / AÇLIK ---------
    public bool IsAlive() => currentHealth > 0;

    public void TakeDamage(int amount)
    {
        if (!IsAlive()) return;
        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        // 🔊 Hasar sesi oynat
        if (hurtClip != null && audioSource != null)
            audioSource.PlayOneShot(hurtClip);

        if (currentHealth <= 0) Die();

        DamagePopupManager.Instance?.SpawnPopup(transform.position, amount);
    }

    public void Heal(int amount)
    {
        if (!IsAlive()) return;
        if (amount <= 0) return;
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
            // Debug.Log("🥩 Açlık: " + currentHunger);
        }
    }

    // --------- KAYNAK / ENVANTER ---------
    public void AddResource(string type, int amount)
    {
        int total = GetTotalResourceAmount();
        if (total + amount > inventoryCapacity)
        {
            Debug.Log("🚫 Envanter dolu!");
            return;
        }

        if (!resources.ContainsKey(type)) resources[type] = 0;
        resources[type] += amount;

        // NOT: Artık pickup anında HEAL YAPMIYORUZ (Meat/Herb dahil).
        Debug.Log($"{type} toplandı! Toplam: {resources[type]}");
    }

    public bool RemoveResource(string type, int amount)
    {
        if (resources.ContainsKey(type) && resources[type] >= amount)
        {
            resources[type] -= amount;
            return true;
        }
        return false;
    }

    public int GetResourceAmount(string type) => resources.ContainsKey(type) ? resources[type] : 0;

    public int GetTotalResourceAmount()
    {
        int total = 0;
        foreach (var kv in resources) total += kv.Value;
        return total;
    }

    public void UnlockBlueprint(string blueprintId)
    {
        if (unlockedBlueprints.Add(blueprintId))
            Debug.Log($"📘 Blueprint açıldı: {blueprintId}");
    }
    public bool HasBlueprint(string id) => unlockedBlueprints.Contains(id);

    // --------- SİLAH PARÇALARI (kısaltıldı) ---------
    public void CollectWeaponPart(WeaponPartType part, int amountToCollect = 1)
    {
        if (!weaponParts.ContainsKey(part)) weaponParts[part] = 0;
        weaponParts[part] += amountToCollect;
        WeaponPartsUI.Instance?.UpdatePartText(part, weaponParts[part]);
    }
    public int GetWeaponPartCount(WeaponPartType part) => weaponParts.ContainsKey(part) ? weaponParts[part] : 0;
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

    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel) LevelUp();
    }
    void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.2f);
        skillPoints++;
        onLevelUp?.Invoke();
    }

    // --------- YEMEK YEME (F) ---------
    /// <summary>
    /// F basıldığında çağrılır. Öncelik: CookedMeat > Meat > Herb
    /// </summary>
    private void TryConsumeFood()
    {
        // 1) Pişmiş et
        if (GetResourceAmount("CookedMeat") > 0)
        {
            EatCookedMeat();
            return;
        }

        // 2) Çiğ et
        if (GetResourceAmount("Meat") > 0)
        {
            EatRawMeat();
            return;
        }

        // 3) Ot (Herb)
        if (GetResourceAmount("Herb") > 0)
        {
            EatHerb();
            return;
        }

        // Yoksa sessizce hiçbir şey yapma
        // Debug.Log("🍽️ Yenilecek bir şey yok.");
    }

    public void EatCookedMeat()
    {
        if (!RemoveResource("CookedMeat", 1)) return;

        Heal(healOnCookedMeatUse);
        GainHunger(hungerOnCookedMeatUse);
        Debug.Log($"🍗 Pişmiş et yendi! +{healOnCookedMeatUse} HP, Açlık +{hungerOnCookedMeatUse} → {currentHunger}/{maxHunger}");
    }

    private void EatRawMeat()
    {
        if (!RemoveResource("Meat", 1)) return;

        Heal(healOnRawMeatUse);
        GainHunger(hungerOnRawMeatUse);
        Debug.Log($"🥩 Çiğ et yendi! +{healOnRawMeatUse} HP, Açlık +{hungerOnRawMeatUse} → {currentHunger}/{maxHunger}");
    }

    private void EatHerb()
    {
        if (!RemoveResource("Herb", 1)) return;

        Heal(healOnHerbUse);
        GainHunger(hungerOnHerbUse);
        Debug.Log($"🌿 Ot yendi! +{healOnHerbUse} HP, Açlık +{hungerOnHerbUse} → {currentHunger}/{maxHunger}");
    }

    private void GainHunger(int amount)
    {
        if (amount <= 0) return;
        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
    }
}
