using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeOfferUI : MonoBehaviour
{
    public TradeRecipe recipe;
    public Button tradeButton;

    [Header("UI")]
    public Image resultIcon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [Header("Costs")]
    public Transform costContainer;
    public GameObject costRowPrefab;

    private TradeUIController ui;

    public void Setup(
        TradeRecipe r,
        TradeUIController controller
    )
    {
        recipe = r;
        ui = controller;

        // 🖼 RESULT ICON
        resultIcon.sprite = r.resultItem.icon;

        // 📝 TEXT
        nameText.text = $"{r.resultItem.itemName} x{r.resultAmount}";
        descriptionText.text = r.description;

        // 🧱 COST LIST
        SetupCosts();

        // 🔘 BUTTON
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(() =>
            ui.OnTradeButtonClicked(recipe)
        );

        // 🔒 Aktiflik
        tradeButton.interactable =
            TradeSystem.Instance.CanTrade(recipe);
    }

    private void SetupCosts()
    {
        foreach (Transform child in costContainer)
            Destroy(child.gameObject);

        foreach (var cost in recipe.costs)
        {
            GameObject row =
                Instantiate(costRowPrefab, costContainer);

            CostRowUI rowUI = row.GetComponent<CostRowUI>();

            int owned =
                Inventory.Instance.GetItemCountByID(
                    cost.item.itemID);

            rowUI.Setup(
                cost.item.icon,
                cost.item.itemName,
                owned,
                cost.amount
            );
        }
    }
}
