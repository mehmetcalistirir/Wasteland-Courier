using UnityEngine;
using System.Collections.Generic;
using TMPro; // UI için

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Crafting")]
    public List<WeaponBlueprint> availableBlueprints; // Oyunda mevcut tüm tarifler

    [Header("UI References")]
    public GameObject inventoryPanel; // Envanter ve Craft'ýn ana paneli
    public Transform partsContainer; // Parçalarýn gösterileceði UI container
    public GameObject inventoryPartPrefab; // Bir parçayý temsil eden UI prefab'ý

    // Mevcut parçalarý ve sayýlarýný tutan sözlük (Dictionary)
    private Dictionary<WeaponPartType, int> collectedParts = new Dictionary<WeaponPartType, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Baþlangýçta paneli gizle
        inventoryPanel.SetActive(false);
        // Tüm part enumlarýný envantere 0 adet olarak ekle
        foreach (WeaponPartType partType in System.Enum.GetValues(typeof(WeaponPartType)))
        {
            if (!collectedParts.ContainsKey(partType))
            {
                collectedParts.Add(partType, 0);
            }
        }
    }

    void Update()
    {
        // Envanteri aç/kapat (örneðin 'I' tuþu ile)
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            if (inventoryPanel.activeSelf)
            {
                UpdateInventoryUI();
            }
        }
    }

    public void AddPart(WeaponPartType partType, int amount)
    {
        if (collectedParts.ContainsKey(partType))
        {
            collectedParts[partType] += amount;
        }
        else
        {
            collectedParts.Add(partType, amount);
        }

        Debug.Log($"{amount} adet {partType} eklendi. Toplam: {collectedParts[partType]}");
        UpdateInventoryUI(); // UI'ý anýnda güncelle
    }

    // Bu fonksiyon, envanter panelindeki görselleri günceller.
    public void UpdateInventoryUI()
    {
        if (!inventoryPanel.activeSelf) return;

        // Önce eski UI objelerini temizle
        foreach (Transform child in partsContainer)
        {
            Destroy(child.gameObject);
        }

        // Sonra mevcut parçalar için yeni UI objeleri oluþtur
        foreach (var part in collectedParts)
        {
            if (part.Value > 0) // Sadece 0'dan fazla olanlarý göster
            {
                GameObject partUI = Instantiate(inventoryPartPrefab, partsContainer);
                // Not: partUI objesinin üzerinde part adýný ve sayýsýný yazan bir script olmalý.
                // Örneðin: partUI.GetComponent<InventorySlotUI>().Setup(part.Key, part.Value);
                // Þimdilik basitçe ismini yazdýralým
                partUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{part.Key}: {part.Value}";
            }
        }
    }

    // Craft iþlemi
    public void TryCraftWeapon(WeaponBlueprint blueprint)
    {
        // Gerekli tüm parçalara sahip miyiz?
        foreach (var requirement in blueprint.requiredParts)
        {
            if (!collectedParts.ContainsKey(requirement.partType) || collectedParts[requirement.partType] < requirement.amount)
            {
                Debug.Log($"Craft BAÞARISIZ: Yeterli {requirement.partType} yok.");
                return; // Bir parça bile eksikse, iþlemi durdur.
            }
        }

        // Tüm parçalar varsa, craft iþlemi baþarýlý!
        Debug.Log($"Craft BAÞARILI: {blueprint.weaponName} üretildi!");

        // 1. Parçalarý envanterden düþ
        foreach (var requirement in blueprint.requiredParts)
        {
            collectedParts[requirement.partType] -= requirement.amount;
        }

        // 2. Silahý WeaponSlotManager'da aç
        WeaponSlotManager.Instance.UnlockWeapon(blueprint.weaponSlotIndexToUnlock);

        // 3. UI'ý güncelle
        UpdateInventoryUI();
    }
}