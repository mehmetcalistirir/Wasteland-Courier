using UnityEngine;

public class CraftUIController : MonoBehaviour
{
    [Header("UI Refs")]
    public Transform craftContainer;
    public GameObject craftSlotPrefab;

    private void OnEnable()
    {
        RefreshUI();
        Inventory.Instance.OnChanged += RefreshUI;
    }

    private void OnDisable()
    {
        Inventory.Instance.OnChanged -= RefreshUI;
    }

    // ------------------------------------------------
    // 🔄 UI Yenile
    // ------------------------------------------------
    public void RefreshUI()
    {
        // 1️⃣ Temizle
        for (int i = craftContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(craftContainer.GetChild(i).gameObject);
        }

        // 2️⃣ Tarifleri bas
        foreach (var recipe in CraftingSystem.Instance.recipes)
        {
            GameObject go = Instantiate(
                craftSlotPrefab,
                craftContainer
            );

            CraftSlotUI slotUI = go.GetComponent<CraftSlotUI>();
            slotUI.Setup(recipe, this);

            bool canCraft = CraftingSystem.Instance.CanCraft(recipe);

            slotUI.craftButton.interactable = canCraft;

            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = canCraft ? 1f : 0.4f;
        }
    }

    // ------------------------------------------------
    // 🔴 Button burayı çağırır
    // ------------------------------------------------
    public void OnCraftButtonClicked(WeaponCraftRecipe recipe)
    {
        bool success = CraftingSystem.Instance.TryCraft(recipe);

        if (success)
            RefreshUI();
    }
}

