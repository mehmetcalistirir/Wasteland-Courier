using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftCostUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text amountText;

    public void Setup(ItemData item, int requiredAmount, int playerAmount)
    {
        if (icon != null && item != null)
            icon.sprite = item.icon;

        if (amountText != null)
        {
            amountText.text = $"{playerAmount}/{requiredAmount}";
            amountText.color = playerAmount >= requiredAmount ? Color.white : Color.red;
        }
    }
}
