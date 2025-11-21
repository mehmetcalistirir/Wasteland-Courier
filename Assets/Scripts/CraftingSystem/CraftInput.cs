using UnityEngine;
using UnityEngine.InputSystem;

public class CraftInput : MonoBehaviour
{
    public CraftUIController craftUI;
    public GameObject inventoryPanel; // Craft açıldığında envanter kapatılacak

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Craft.performed += OnCraftPressed;
    }

    private void OnDisable()
    {
        controls.Gameplay.Craft.performed -= OnCraftPressed;
        controls.Gameplay.Disable();
    }

    private void OnCraftPressed(InputAction.CallbackContext ctx)
    {
        if (craftUI == null) return;

        if (craftUI.craftPanel.activeSelf)
            craftUI.Close();
        else
            craftUI.Open();

        // Craft açılınca envanter kapanır
        if (inventoryPanel != null && craftUI.craftPanel.activeSelf)
            inventoryPanel.SetActive(false);
    }
}
