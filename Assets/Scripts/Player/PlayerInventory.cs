using UnityEngine;
using System.Collections.Generic;
using TMPro; // UI i�in
using UnityEngine.InputSystem;


public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Crafting")]
    public List<WeaponBlueprint> availableBlueprints; // Oyunda mevcut t�m tarifler

    [Header("UI References")]
    public GameObject inventoryPanel; // Envanter ve Craft'�n ana paneli
    public Transform partsContainer; // Par�alar�n g�sterilece�i UI container
    public GameObject inventoryPartPrefab; // Bir par�ay� temsil eden UI prefab'�

    // Mevcut par�alar� ve say�lar�n� tutan s�zl�k (Dictionary)
    private Dictionary<WeaponPartType, int> collectedParts = new Dictionary<WeaponPartType, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ba�lang��ta paneli gizle
        inventoryPanel.SetActive(false);
        // T�m part enumlar�n� envantere 0 adet olarak ekle
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
        void Update()
{
    if (Keyboard.current.kKey.wasPressedThisFrame)
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        if (inventoryPanel.activeSelf)
        {
            UpdateInventoryUI();
        }
    }
}

    }

    private Sprite GetIconForPart(WeaponPartType partType)
    {
        // İsimlerine göre "Resources/Icons" klasöründen yükleyebiliriz (Icons/Wood, Icons/Stone vs.)
        string iconName = partType.ToString(); // Örn: "Wood"
        Sprite icon = Resources.Load<Sprite>($"Icons/{iconName}");

        if (icon == null)
            Debug.LogWarning($"Icon bulunamadı: {iconName}");

        return icon;
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
        UpdateInventoryUI(); // UI'� an�nda g�ncelle
    }

    // Bu fonksiyon, envanter panelindeki g�rselleri g�nceller.
    public void UpdateInventoryUI()
{
    if (!inventoryPanel.activeSelf || partsContainer == null || inventoryPartPrefab == null)
    {
        Debug.LogWarning("Envanter UI eksik referans: partsContainer veya prefab atanmadı.");
        return;
    }

    foreach (Transform child in partsContainer)
        Destroy(child.gameObject);

    foreach (var part in collectedParts)
    {
        if (part.Value > 0)
        {
            GameObject partUI = Instantiate(inventoryPartPrefab, partsContainer);

            PartSlotUI slot = partUI.GetComponent<PartSlotUI>();
            if (slot != null)
            {
                // Part sprite'ını buradan alıyoruz.
                Sprite icon = GetIconForPart(part.Key);
                slot.Setup(icon, part.Value);
            }
        }
    }
}



    // Craft i�lemi
    public void TryCraftWeapon(WeaponBlueprint blueprint)
    {
        // Gerekli t�m par�alara sahip miyiz?
        foreach (var requirement in blueprint.requiredParts)
        {
            if (!collectedParts.ContainsKey(requirement.partType) || collectedParts[requirement.partType] < requirement.amount)
            {
                Debug.Log($"Craft BA�ARISIZ: Yeterli {requirement.partType} yok.");
                return; // Bir par�a bile eksikse, i�lemi durdur.
            }
        }

        // T�m par�alar varsa, craft i�lemi ba�ar�l�!
        Debug.Log($"Craft BA�ARILI: {blueprint.weaponName} �retildi!");

        // 1. Par�alar� envanterden d��
        foreach (var requirement in blueprint.requiredParts)
        {
            collectedParts[requirement.partType] -= requirement.amount;
        }

        // 2. Silah� WeaponSlotManager'da a�
        WeaponSlotManager.Instance.UnlockWeapon(blueprint.weaponSlotIndexToUnlock);

        // 3. UI'� g�ncelle
        UpdateInventoryUI();
    }
}