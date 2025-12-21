using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftSlotUI : MonoBehaviour
{
    public WeaponCraftRecipe recipe;
    public Button craftButton;

    [Header("UI")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI costText;

    private CraftUIController ui;

    public void Setup(
        WeaponCraftRecipe r,
        CraftUIController controller
    )
    {
        recipe = r;
        ui = controller;

        // 🎯 Sonuç
        resultText.text = r.resultWeapon.itemName;

        // 📦 Cost listesi (ÖNEMLİ KISIM)
        costText.text = "";
        foreach (var cost in r.costs)
        {
            int have = Inventory.Instance.GetItemCountByID(cost.item.itemID);
            costText.text +=
                $"{cost.item.itemName} {have}/{cost.amount}\n";
        }

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() =>
            ui.OnCraftButtonClicked(recipe)
        );
    }
}
