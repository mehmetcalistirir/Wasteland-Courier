using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



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


    [Header("Food Healing / Hunger")]
    public int healOnRawMeatUse = 5;
    public int healOnCookedMeatUse = 15;
    public int healOnHerbUse = 10;

    public int hungerOnRawMeatUse = 10;
    public int hungerOnCookedMeatUse = 30;
    public int hungerOnHerbUse = 0;

    [Header("Health")]
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

    [Header("Move/Inventory")]
    public float moveSpeed = 5f;
    public int gold = 10;

    [Header("Hunger")]
    public int maxHunger = 100;
    public int currentHunger;
    public float hungerDecreaseInterval = 5f;
    public int hungerDecreaseAmount = 1;
    private float hungerTimer;

    [Header("Audio")]
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

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryConsumeFood();
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
            Heal(healOnCookedMeatUse);
            GainHunger(hungerOnCookedMeatUse);
            Debug.Log("🍗 Pişmiş et yendi!");
            return;
        }

        if (Inventory.Instance.HasEnough(rawMeatSO, 1))
        {
            Inventory.Instance.TryConsume(rawMeatSO, 1);
            Heal(healOnRawMeatUse);
            GainHunger(hungerOnRawMeatUse);
            Debug.Log("🥩 Çiğ et yendi!");
            return;
        }

        if (Inventory.Instance.HasEnough(herbSO, 1))
        {
            Inventory.Instance.TryConsume(herbSO, 1);
            Heal(healOnHerbUse);
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
