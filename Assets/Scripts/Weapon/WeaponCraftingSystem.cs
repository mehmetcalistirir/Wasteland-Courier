// WeaponCraftingSystem.cs (TEMİZLENMİŞ VE TAM HALİ)

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WeaponCraftingSystem : MonoBehaviour
{
    public static WeaponCraftingSystem Instance { get; private set; }

    [Header("Crafting Data")]
    public List<WeaponBlueprint> availableBlueprints;

    [Header("UI References")]
    public GameObject craftingPanel;
    public Transform requirementsContainer;
    public GameObject requirementLinePrefab;
    public TextMeshProUGUI craftPromptText;
    
    // UI elemanlarını tutmak için bir liste (önceki hatalı kodda eksikti)
    private List<BlueprintUI> blueprintUIElements = new List<BlueprintUI>(); 
    private PlayerStats playerStats;
    private WeaponBlueprint selectedBlueprint;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null) Debug.LogError("Sahnede PlayerStats bulunamadı!");

        InitializeBlueprintList(); // UI listesini başlangıçta oluştur.
        if (craftingPanel != null) craftingPanel.SetActive(false);
        ClearDetailInfo();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame && CraftingStation.IsPlayerInRange)
        {
            ToggleCraftingPanel();
        }

        if (craftingPanel.activeSelf && selectedBlueprint != null && CanCraft(selectedBlueprint))
        {
            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            {
                TryCraftWeapon();
            }
        }
    }
    
    // UI listesini sadece bir kere, oyun başında oluşturur.
    private void InitializeBlueprintList()
    {
        // Not: Bu sistem, UI elemanlarını kodla oluşturur. Eğer statik bir listeniz varsa,
        // bu fonksiyonu Inspector'dan referansları alacak şekilde düzenleyebilirsiniz.
        // Şimdilik dinamik oluşturma varsayıyoruz.
        // foreach (var blueprint in availableBlueprints) { ... }
    }

    public void ToggleCraftingPanel()
    {
        bool isActive = !craftingPanel.activeSelf;
        craftingPanel.SetActive(isActive);

        if (isActive)
        {
            UpdateAllBlueprintUI(); // Panel açıldığında UI'ı güncelle
        }
        else
        {
            selectedBlueprint = null;
            ClearDetailInfo();
        }
    }

    public void SelectBlueprint(WeaponBlueprint blueprint)
    {
        Debug.Log($"<color=yellow>SEÇİLDİ:</color> {blueprint.weaponName} tarifi inceleniyor.");
        selectedBlueprint = blueprint;
        UpdateDetailPanel();
    }
    
    // Paneli açtığında veya bir craft yaptığında tüm UI elemanlarını günceller.
    public void UpdateAllBlueprintUI()
    {
        // Bu fonksiyonun çalışması için blueprintUIElements listesinin dolu olması gerekir.
        // Eğer UI elemanlarını Inspector'dan manuel olarak atıyorsanız, bu listeyi de manuel doldurmalısınız.
        foreach (var uiElement in blueprintUIElements)
        {
            uiElement.UpdateStatus();
        }
    }

    private void UpdateDetailPanel()
    {
        if (selectedBlueprint == null)
        {
            ClearDetailInfo();
            return;
        }

        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        Debug.Log($"{selectedBlueprint.weaponName} için {selectedBlueprint.requiredParts.Count} adet parça gereksinimi var.");
        foreach (var partReq in selectedBlueprint.requiredParts)
        {
            int currentAmount = playerStats.GetWeaponPartCount(partReq.partType);
            AddRequirementLine(partReq.partType.ToString(), currentAmount, partReq.amount);
        }

        if (craftPromptText != null)
        {
            craftPromptText.gameObject.SetActive(CanCraft(selectedBlueprint));
        }
    }

    private void ClearDetailInfo()
    {
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        if (craftPromptText != null) craftPromptText.gameObject.SetActive(false);
    }

    private void AddRequirementLine(string itemName, int current, int required)
    {
        if (required > 0 && requirementLinePrefab != null)
        {
            GameObject lineObject = Instantiate(requirementLinePrefab, requirementsContainer);
            lineObject.GetComponent<RequirementLineUI>().Setup(itemName, current, required);
        }
    }

    public bool CanCraft(WeaponBlueprint blueprint)
    {
        if (blueprint == null || playerStats == null) return false;

        // CaravanInventory'de depolanmış mı diye kontrol et.
        if (CaravanInventory.Instance != null && CaravanInventory.Instance.IsWeaponStored(blueprint))
        {
            return false; // Zaten üretilmiş ve depoda.
        }

        foreach (var requirement in blueprint.requiredParts)
        {
            if (playerStats.GetWeaponPartCount(requirement.partType) < requirement.amount)
            {
                return false;
            }
        }
        return true;
    }

    // Sadece TEK BİR TryCraftWeapon fonksiyonu olmalı.
    public void TryCraftWeapon()
    {
        if (CanCraft(selectedBlueprint))
        {
            playerStats.ConsumeWeaponParts(selectedBlueprint.requiredParts);
            
            // Silahı Karavan'ın deposuna gönder.
            CaravanInventory.Instance.StoreWeapon(selectedBlueprint);
            
            UpdateDetailPanel();
            UpdateAllBlueprintUI();
            
            Debug.Log($"Craft BAŞARILI: {selectedBlueprint.weaponName} üretildi ve depoya gönderildi!");
        }
    }
}