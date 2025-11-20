using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryAndCraftInput : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public CraftUI craftUI;   // CraftUI scriptine referans

    private PlayerControls controls;
    private bool inventoryOpen = false;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();

        controls.Gameplay.Inventory.performed += OnInventoryPressed;
        controls.Gameplay.Craft.performed     += OnCraftPressed;
    }

    private void OnDisable()
    {
        controls.Gameplay.Inventory.performed -= OnInventoryPressed;
        controls.Gameplay.Craft.performed     -= OnCraftPressed;
        controls.Gameplay.Disable();
    }

    private void OnInventoryPressed(InputAction.CallbackContext ctx)
    {
        if (inventoryPanel == null) return;

        inventoryOpen = !inventoryOpen;
        inventoryPanel.SetActive(inventoryOpen);

        // Envanter açıldığında craft’ı kapat
        if (inventoryOpen && craftUI != null && craftUI.IsOpen)
            craftUI.ClosePanel();
    }

    private void OnCraftPressed(InputAction.CallbackContext ctx)
    {
        if (craftUI == null) return;

        if (craftUI.IsOpen)
            craftUI.ClosePanel();
        else
            craftUI.OpenPanel();

        // Craft açıldığında envanteri kapat
        if (inventoryPanel != null && craftUI.IsOpen)
        {
            inventoryOpen = false;
            inventoryPanel.SetActive(false);
        }
    }
}
