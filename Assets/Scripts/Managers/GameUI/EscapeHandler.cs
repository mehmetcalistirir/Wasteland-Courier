using UnityEngine;
using UnityEngine.InputSystem;

public class EscapeHandler : MonoBehaviour, PlayerControls.IGameplayActions
{
    private PlayerControls controls;

    [Header("UI Panels")]
    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.SetCallbacks(this);
    }

    private void OnEnable() => controls.Gameplay.Enable();
    private void OnDisable() => controls.Gameplay.Disable();

    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Önce TRADE kapansın
        if (tradePanel != null && tradePanel.activeSelf)
        {
            tradePanel.SetActive(false);
            return;
        }

        // Inventory kapansın
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
            return;
        }

        // Craft kapansın
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            return;
        }

        // ❗ Başka panel açma!
        // ESC sadece kapatma görevi görür.
    }

    // Gerekli boş interface metotları
    public void OnMove(InputAction.CallbackContext ctx) {}
    public void OnSprint(InputAction.CallbackContext ctx) {}
    public void OnMap(InputAction.CallbackContext ctx) {}
    public void OnInventory(InputAction.CallbackContext ctx) {}
    public void OnCraft(InputAction.CallbackContext ctx) {}
    public void OnReload(InputAction.CallbackContext ctx) {}
    public void OnWeapon1(InputAction.CallbackContext ctx) {}
    public void OnWeapon2(InputAction.CallbackContext ctx) {}
    public void OnWeapon3(InputAction.CallbackContext ctx) {}
    public void OnMelee(InputAction.CallbackContext ctx) {}
    public void OnADS(InputAction.CallbackContext ctx) {}
    public void OnCaravanWeapons(InputAction.CallbackContext ctx) {}
    public void OnInteract(InputAction.CallbackContext ctx) {}
}
