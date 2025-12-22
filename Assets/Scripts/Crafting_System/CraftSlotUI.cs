using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Transform costContainer;
    public GameObject costRowPrefab;
    public Button craftButton;

    private WeaponCraftRecipe recipe;
    private CraftUIController controller;

    public void Setup(WeaponCraftRecipe recipe, CraftUIController controller)
    {
        
        this.recipe = recipe;
        this.controller = controller;

        WeaponItemData weapon = recipe.resultWeapon;

        // 🖼 ICON
        iconImage.sprite = weapon.icon;

        // 📝 TEXT
        nameText.text = weapon.itemName;
        descriptionText.text = recipe.description;

        // 🧱 COST LIST
        foreach (Transform child in costContainer)
            Destroy(child.gameObject);

        foreach (var cost in recipe.costs)
        {
            GameObject row = Instantiate(costRowPrefab, costContainer);
            CostRowUI rowUI = row.GetComponent<CostRowUI>();

            int owned = Inventory.Instance.GetItemCountByID(cost.item.itemID);

            rowUI.Setup(
                cost.item.icon,
                cost.item.itemName,
                owned,
                cost.amount
            );
        }

        // 🔘 BUTTON
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftPressed);
    }

    void OnCraftPressed()
    {
        CraftingSystem.Instance.TryCraft(recipe);
        controller.RefreshUI();
    }
}
