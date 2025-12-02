using UnityEngine;

public class EscapeHandler : MonoBehaviour, PlayerControls.IGameplayActions
{
    private PlayerControls controls;

    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenu;   // artık kullanılmayacak ama referansı kalsın

    private bool AnyPanelOpen =>
        (inventoryPanel && inventoryPanel.activeSelf) ||
        (craftPanel && craftPanel.activeSelf) ||
        (tradePanel && tradePanel.activeSelf);

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.SetCallbacks(this);
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    public void OnEscape(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
{
    if (!ctx.performed) return;

    HandleEscape();
}

    private void HandleEscape()
    {
        // 1) Eğer Inventory / Craft / Trade açık ise → sadece onları kapat
        if (AnyPanelOpen)
        {
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (craftPanel) craftPanel.SetActive(false);
            if (tradePanel) tradePanel.SetActive(false);

            Time.timeScale = 1f;
            return;
        }

        // ❌ 2) PauseMenu'yu artık EscapeHandler KESİNLİKLE YÖNETMİYOR ❌
        // PauseMenu tamamen PauseMenu.cs tarafından kontrol edilecek.
    }

    // kullanılmayan callbacks:
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnMap(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnInventory(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnCraft(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnReload(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnWeapon1(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnWeapon2(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnWeapon3(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnMelee(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnADS(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnShoot(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
    public void OnCaravanWeapons(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {}
}
