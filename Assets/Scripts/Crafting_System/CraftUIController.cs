using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject craftPanel;
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
        craftPanel.SetActive(false);
    }

    // --------------------------------------------------
    // Tarif Slotlarını oluştur
    // --------------------------------------------------
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
                Debug.LogError("CraftSlot prefabında CraftSlotUI yok!");
                continue;
            }

            WeaponItemData weaponItem = recipe.resultWeapon;
            if (weaponItem == null)
            {
                Debug.LogError("Tarifte resultWeapon (WeaponItemData) eksik!");
                continue;
            }

            slotUI.Setup(recipe, weaponItem.icon, weaponItem.itemName);
        }
    }

    // --------------------------------------------------
    // Tarif seçme
    // --------------------------------------------------
    public void SelectRecipe(WeaponCraftRecipe recipe)
    {
        selectedRecipe = recipe;

        WeaponItemData weaponItem = recipe.resultWeapon;
        bool existsInCaravan = CaravanInventory.Instance.HasWeapon(weaponItem);

        craftButtonText.text = existsInCaravan ? "Swap" : "Craft";
    }

    // --------------------------------------------------
    // Craft / Swap butonu
    // --------------------------------------------------
    public void OnCraftButtonPressed()
    {
        if (selectedRecipe == null)
        {
            Debug.Log("Tarif seçilmedi.");
            return;
        }

        WeaponItemData weaponItem = selectedRecipe.resultWeapon;
        bool existsInCaravan = CaravanInventory.Instance.HasWeapon(weaponItem);

        if (existsInCaravan)
        {
            SwapWithCaravan(weaponItem);
            Debug.Log($"Swap: {weaponItem.itemName}");
            selectedRecipe = null;
            return;
        }

        bool success = CraftingSystem.Instance.TryCraft(selectedRecipe);

        if (success)
        {
            Debug.Log($"Craft başarılı: {weaponItem.itemName}");
            PopulateRecipes();
            selectedRecipe = null;
        }
        else
        {
            Debug.Log($"Craft başarısız: {weaponItem.itemName}");
        }
    }

    // --------------------------------------------------
    // Swap işlemi
    // --------------------------------------------------
    private void SwapWithCaravan(WeaponItemData weaponItem)
    {
        if (weaponItem == null)
            return;

        // Hangi slotta bu silah takılmalı?
        WeaponSlotType slotType =
            WeaponSlotManager.Instance.GetSlotForWeapon(weaponItem.weaponType);

        int slotIndex = (int)slotType;

        // Mevcut silahı karavana koy
        WeaponItemData currentItem =
            WeaponSlotManager.Instance.GetWeaponItemInSlot(slotIndex);

        if (currentItem != null)
            CaravanInventory.Instance.StoreWeapon(currentItem);

        // Karavandan seçilen silahı çıkar
        var list = CaravanInventory.Instance.GetWeapons(weaponItem.weaponType);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].itemID == weaponItem.itemID)
            {
                list.RemoveAt(i);
                break;
            }
        }

        // Yeni silahı tak
        WeaponSlotManager.Instance.EquipWeaponInSlot(weaponItem, slotIndex);
        WeaponSlotManager.Instance.SwitchSlot(slotIndex);
    }
}
