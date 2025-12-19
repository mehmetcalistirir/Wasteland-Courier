using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public Transform costRoot;
    public GameObject costEntryPrefab;

    private WeaponCraftRecipe recipe;

    public void Setup(WeaponCraftRecipe recipe, Sprite weaponIcon, string name)
    {
        this.recipe = recipe;

        icon.sprite = weaponIcon;
        nameText.text = name;

        RefreshCosts();
    }

    private void RefreshCosts()
    {
        // Eski maliyet entry'lerini temizle
        foreach (Transform t in costRoot)
            Destroy(t.gameObject);

        foreach (var cost in recipe.costs)
        {
            GameObject costGO = Instantiate(costEntryPrefab, costRoot);
            CraftCostUI ui = costGO.GetComponent<CraftCostUI>();

            int playerAmount = Inventory.Instance.GetItemCount(cost.item);

            ui.Setup(cost.item, cost.amount, playerAmount);
        }
    }

    // UI butonu için
    public void OnSelectPressed()
    {
        CraftUIController.Instance.SelectRecipe(recipe);
    }
}
