using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text titleText;
    public Button button;

    [Header("Data")]
    public WeaponCraftRecipe recipe;

    // ---------------------------------------------------------
    //  Slotu tarif verisiyle hazırla
    // ---------------------------------------------------------
    public void Setup(WeaponCraftRecipe recipe, Sprite icon, string title)
    {
        this.recipe = recipe;

        iconImage.sprite = icon;
        titleText.text = title;

        // Tıklama event’lerini temizle → ekle
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            CraftUIController.Instance.SelectRecipe(recipe);
        });
    }
}
