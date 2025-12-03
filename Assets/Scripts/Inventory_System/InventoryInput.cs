using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInput : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public CraftUIController craftUI;   // Envanter açıldığında craft kapanacak

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
    }

    private void OnDisable()
    {
        controls.Gameplay.Inventory.performed -= OnInventoryPressed;
        controls.Gameplay.Disable();
    }

    private void OnInventoryPressed(InputAction.CallbackContext ctx)
{
    if (inventoryPanel == null) return;

    // Eğer craft açık ise önce craft'ı kapat
    if (craftUI != null && craftUI.craftPanel.activeSelf)
        craftUI.Close();

    inventoryOpen = !inventoryOpen;
    inventoryPanel.SetActive(inventoryOpen);
}

}
