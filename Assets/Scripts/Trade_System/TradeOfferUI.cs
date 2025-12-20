using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeOfferUI : MonoBehaviour
{
    public TradeRecipe recipe;
    public Button tradeButton;

    [Header("UI")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI costText;

    private TradeUIController ui;

    public void Setup(
        TradeRecipe r,
        TradeUIController controller
    )
    {
        recipe = r;
        ui = controller;

        // Sonuç item
        resultText.text =
            $"{r.resultItem.itemName} x{r.resultAmount}";

        // Cost listesi
        costText.text = "";
        foreach (var cost in r.costs)
        {
            costText.text +=
                $"{cost.item.itemName} x{cost.amount}\n";
        }

        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(() =>
            ui.OnTradeButtonClicked(recipe)
        );
    }
}
