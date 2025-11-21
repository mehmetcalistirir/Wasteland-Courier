using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftUIController : MonoBehaviour
{
    public GameObject craftPanel;
    public Transform gridParent;
    public GameObject craftSlotPrefab;

    public WeaponRecipe selectedRecipe;
    public static CraftUIController Instance;

    private void Start()
    {
        Instance = this;

        PopulateRecipes();
        craftPanel.SetActive(false);
    }

    public void PopulateRecipes()
    {
        foreach (Transform t in gridParent)
            Destroy(t.gameObject);

        foreach (var recipe in CraftingSystem.Instance.recipes)
        {
            GameObject slotGO = Instantiate(craftSlotPrefab, gridParent);

            // 🔹 Artık buton event'ini CraftSlotUI yönetecek
            CraftSlotUI slotUI = slotGO.GetComponent<CraftSlotUI>();
            if (slotUI == null)
            {
                Debug.LogError("CraftSlot prefabında CraftSlotUI component yok!");
                continue;
            }

            slotUI.Setup(
                recipe,
                recipe.weaponItem.icon,
                recipe.weaponItem.itemName
            );
        }
    }

    public void SelectRecipe(WeaponRecipe recipe)
    {
        selectedRecipe = recipe;
        Debug.Log("Seçilen tarif: " + recipe.weaponItem.itemName);
    }

    public void TryCraft(WeaponRecipe recipe)
    {
        if (CraftingSystem.Instance.TryCraft(recipe))
            Debug.Log("Craft başarılı!");
        else
            Debug.Log("Craft başarısız!");
    }

    public void Open()
    {
        craftPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        craftPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
