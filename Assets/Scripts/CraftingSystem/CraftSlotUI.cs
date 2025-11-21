using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text title;
    public Button button;

    private WeaponRecipe recipe;

    public void Setup(WeaponRecipe recipe, Sprite iconSprite, string titleText)
    {
        this.recipe = recipe;
        icon.sprite = iconSprite;
        title.text = titleText;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // Tarif seç
        CraftUIController.Instance.SelectRecipe(recipe);

        // Direkt craft et
        CraftUIController.Instance.TryCraft(recipe);
    }
}
