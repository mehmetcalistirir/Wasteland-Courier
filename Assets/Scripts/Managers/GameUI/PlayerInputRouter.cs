using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour, PlayerControls.IGameplayActions
{
    private PlayerControls controls;

    [Header("Referans Paneller")]
    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenuPanel;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.SetCallbacks(this);
    }

    private void OnEnable() => controls.Gameplay.Enable();
    private void OnDisable() => controls.Gameplay.Disable();

    // ---------------- PANEL INPUT -------------------

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (UIPanelSystem.Instance.IsPanelOpen()) return;

        UIPanelSystem.Instance.OpenPanel(inventoryPanel);
    }

    public void OnCraft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (UIPanelSystem.Instance.IsPanelOpen()) return;

        UIPanelSystem.Instance.OpenPanel(craftPanel);
    }

    public void OnTrade(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (UIPanelSystem.Instance.IsPanelOpen()) return;

        UIPanelSystem.Instance.OpenPanel(tradePanel);
    }

    public void OnEscape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (UIPanelSystem.Instance.IsPanelOpen())
        {
            UIPanelSystem.Instance.CloseCurrentPanel();
        }
        else
        {
            UIPanelSystem.Instance.OpenPanel(pauseMenuPanel);
        }
    }

    // ------------- ARAYÜZ ZORUNLU FONKSIYONLAR --------------

    public void OnMove(InputAction.CallbackContext ctx) { }
    public void OnSprint(InputAction.CallbackContext ctx) { }
    public void OnMap(InputAction.CallbackContext ctx) { }
    public void OnReload(InputAction.CallbackContext ctx) { }
    public void OnWeapon1(InputAction.CallbackContext ctx) { }
    public void OnWeapon2(InputAction.CallbackContext ctx) { }
    public void OnWeapon3(InputAction.CallbackContext ctx) { }
    public void OnMelee(InputAction.CallbackContext ctx) { }
    public void OnADS(InputAction.CallbackContext ctx) { }
    public void OnCaravanWeapons(InputAction.CallbackContext ctx) { }
}
