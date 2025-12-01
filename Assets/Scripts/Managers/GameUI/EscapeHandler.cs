using UnityEngine;

public class EscapeHandler : MonoBehaviour, PlayerControls.IGameplayActions
{
    private PlayerControls controls;

    public GameObject inventoryPanel;
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenu;

    private bool AnyPanelOpen =>
        (inventoryPanel && inventoryPanel.activeSelf) ||
        (craftPanel && craftPanel.activeSelf) ||
        (tradePanel && tradePanel.activeSelf);

    private void Awake()
    {
        Debug.Log("EscapeHandler Awake ÇALIŞTI");
        controls = new PlayerControls();
        controls.Gameplay.SetCallbacks(this);   // 🚀 En önemli satır
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    // 🚀 PlayerControls içindeki ESC tuşuna basınca burası otomatik tetiklenir
    public void OnEscape(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return; // sadece "performed" anında çalışsın

        HandleEscape();
    }

    private void HandleEscape()
    {
        // 1) Panel açıksa sadece paneli kapatsın
        if (AnyPanelOpen)
        {
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (craftPanel) craftPanel.SetActive(false);
            if (tradePanel) tradePanel.SetActive(false);

            Time.timeScale = 1f;
            return;
        }

        // 2) Panel yoksa → Pause Toggle
        bool isActive = pauseMenu.activeSelf;
        pauseMenu.SetActive(!isActive);
        Time.timeScale = isActive ? 1f : 0f;
    }

    // Kullanmadığın input callback'leri boş bırakılacak:
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
}
