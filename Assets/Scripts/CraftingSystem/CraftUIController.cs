using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject craftPanel;  // Paneli PlayerInputRouter açacak/kapatacak
    public Transform gridParent;
    public GameObject craftSlotPrefab;
    public Button craftButton;
    public TMP_Text craftButtonText;

    [Header("Logic")]
    public WeaponCraftRecipe selectedRecipe;

    public static CraftUIController Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PopulateRecipes();
        craftPanel.SetActive(false);  // Başlangıçta kapalı
    }

    // ------------------------------
    // Tarif Slotlarını oluştur
    // ------------------------------
    public void PopulateRecipes()
    {
        foreach (Transform t in gridParent)
            Destroy(t.gameObject);

        if (CraftingSystem.Instance == null)
        {
            Debug.LogError("CraftingSystem.Instance bulunamadı!");
            return;
        }

        foreach (var recipe in CraftingSystem.Instance.recipes)
        {
            GameObject slotGO = Instantiate(craftSlotPrefab, gridParent);

            CraftSlotUI slotUI = slotGO.GetComponent<CraftSlotUI>();
            if (slotUI == null)
            {
                Debug.LogError("CraftSlot prefabında CraftSlotUI component yok!");
                continue;
            }

            WeaponData weapon = recipe.resultWeapon;
            if (weapon == null)
            {
                Debug.LogError("Tarifte resultWeapon eksik!");
                continue;
            }

            slotUI.Setup(recipe, weapon.icon, weapon.itemName);
        }
    }

    // ------------------------------
    // Tarif seçme
    // ------------------------------
    public void SelectRecipe(WeaponCraftRecipe recipe)
    {
        selectedRecipe = recipe;

        WeaponData weapon = recipe.resultWeapon;
        bool existsInCaravan = CaravanInventory.Instance.HasWeapon(weapon);

        craftButtonText.text = existsInCaravan ? "Swap" : "Craft";
    }

    // ------------------------------
    // Craft / Swap butonu
    // ------------------------------
    public void OnCraftButtonPressed()
    {
        if (selectedRecipe == null)
        {
            Debug.Log("Tarif seçilmedi.");
            return;
        }

        WeaponData weapon = selectedRecipe.resultWeapon;
        bool existsInCaravan = CaravanInventory.Instance.HasWeapon(weapon);

        if (existsInCaravan)
        {
            SwapWithCaravan(weapon);
            Debug.Log($"Swap: {weapon.itemName}");
            selectedRecipe = null;
            return;
        }

        bool success = CraftingSystem.Instance.TryCraft(selectedRecipe);

        if (success)
        {
            Debug.Log($"Craft başarılı: {weapon.itemName}");
            PopulateRecipes();
            selectedRecipe = null;
        }
        else
        {
            Debug.Log($"Craft başarısız: {weapon.itemName}");
        }
    }

    // ------------------------------
    // Swap işlemi
    // ------------------------------
    private void SwapWithCaravan(WeaponData weapon)
    {
        WeaponSlotType slotType = WeaponSlotManager.Instance.GetSlotForWeapon(weapon);
        int slotIndex = (int)slotType;

        WeaponData currentWeapon = WeaponSlotManager.Instance.slots[slotIndex];

        if (currentWeapon != null)
            CaravanInventory.Instance.StoreWeapon(currentWeapon);

        var list = CaravanInventory.Instance.GetWeapons(weapon.weaponType);

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].itemID == weapon.itemID)
            {
                list.RemoveAt(i);
                break;
            }
        }

        WeaponSlotManager.Instance.slots[slotIndex] = weapon;
        WeaponSlotManager.Instance.clip[slotIndex] = weapon.clipSize;
        WeaponSlotManager.Instance.reserve[slotIndex] = weapon.maxAmmoCapacity;

        WeaponSlotManager.Instance.SwitchSlot(slotIndex);
    }
}
