using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum CraftRecipeType
{
    Weapon,
    Item
}


public class CraftSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Transform costContainer;
    public GameObject costRowPrefab;
    public Button craftButton;

    [HideInInspector] public CraftRecipeType recipeType;

    private WeaponCraftRecipe weaponRecipe;
    private ItemCraftRecipe itemRecipe;
    private CraftUIController controller;

    // ------------------------------------------------
    // 🔫 WEAPON SETUP
    // ------------------------------------------------
    public void Setup(WeaponCraftRecipe recipe, CraftUIController controller)
    {
        recipeType = CraftRecipeType.Weapon;
        weaponRecipe = recipe;
        itemRecipe = null;
        this.controller = controller;

        WeaponItemData weapon = recipe.resultWeapon;

        // 🖼 ICON
        iconImage.sprite = weapon.icon;

        // 📝 TEXT
        nameText.text = weapon.itemName;
        descriptionText.text = recipe.description;

        SetupCosts(recipe.costs);

        SetupButton();
    }

    // ------------------------------------------------
    // 📦 ITEM SETUP
    // ------------------------------------------------
    public void Setup(ItemCraftRecipe recipe, CraftUIController controller)
    {
        recipeType = CraftRecipeType.Item;
        itemRecipe = recipe;
        weaponRecipe = null;
        this.controller = controller;

        // 🖼 ICON
        iconImage.sprite = recipe.resultItem.icon;

        // 📝 TEXT
        nameText.text = recipe.resultItem.itemName;
        descriptionText.text = recipe.description;

        SetupCosts(recipe.costs);

        SetupButton();
    }

    // ------------------------------------------------
    // 🧱 COST UI
    // ------------------------------------------------
    private void SetupCosts(System.Collections.Generic.List<ItemCost> costs)
    {
        foreach (Transform child in costContainer)
            Destroy(child.gameObject);

        foreach (var cost in costs)
        {
            GameObject row = Instantiate(costRowPrefab, costContainer);
            CostRowUI rowUI = row.GetComponent<CostRowUI>();

            int owned = Inventory.Instance.GetItemCountByID(
                cost.item.itemID);

            rowUI.Setup(
                cost.item.icon,
                cost.item.itemName,
                owned,
                cost.amount
            );
        }
    }

    // ------------------------------------------------
    // 🔘 BUTTON
    // ------------------------------------------------
    private void SetupButton()
    {
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftPressed);
    }

    private void OnCraftPressed()
    {
        bool success = false;

        switch (recipeType)
        {
            case CraftRecipeType.Weapon:
                success = CraftingSystem.Instance.TryCraft(weaponRecipe);
                break;

            case CraftRecipeType.Item:
                success = CraftingSystem.Instance.TryCraftItem(itemRecipe);
                break;
        }

        if (success)
            controller.RefreshUI();
    }
}