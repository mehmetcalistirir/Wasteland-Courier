using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour
{
    public static PlayerInputRouter Instance;

    private PlayerControls controls;

    [Header("Panels")]
    public GameObject craftPanel;
    public GameObject tradePanel;
    public GameObject pauseMenuPanel;

    [Header("References")]
    public CaravanInteraction caravan;

    // Aktif NPC (trigger içinde olan)
    private NPCTradeInteract activeTradeNPC;

    // ======================================================
    // LIFECYCLE
    // ======================================================
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Inventory.performed += OnInventory;
        controls.Gameplay.Interact.performed += OnInteract;
        controls.Gameplay.Escape.performed += OnEscape;

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Inventory.performed -= OnInventory;
        controls.Gameplay.Interact.performed -= OnInteract;
        controls.Gameplay.Escape.performed -= OnEscape;

        controls.Gameplay.Disable();
    }

    // ======================================================
    // INPUT HANDLERS
    // ======================================================

    // INVENTORY (I)
    private void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (IsAnyBlockingPanelOpen()) return;

        if (PipBoyController.Instance == null) return;
        if (PipBoyController.Instance.IsOpen) return;

        InteractionPromptUI.Instance?.Hide();
        PipBoyController.Instance.Open(0);
    }

    // INTERACT (E)
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Interact basıldı → prompt her zaman kapanır
        InteractionPromptUI.Instance?.Hide();

        // 1️⃣ Trade açıksa → kapat
        if (tradePanel != null && tradePanel.activeSelf)
        {
            tradePanel.SetActive(false);
            ResumeGame();
            return;
        }

        // 2️⃣ Craft açıksa → kapat
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            ResumeGame();
            return;
        }

        // 3️⃣ NPC Trade (öncelikli)
        if (activeTradeNPC != null && activeTradeNPC.playerInRange)
        {
            OpenTrade();
            return;
        }

        // 4️⃣ Caravan Craft
        if (caravan != null && caravan.playerInRange)
        {
            ToggleCraft();
            return;
        }
    }

    // ESC
    private void OnEscape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (GameStateManager.IsGameOver) return;

        // Önce açık panelleri kapat
        if (tradePanel != null && tradePanel.activeSelf)
        {
            tradePanel.SetActive(false);
            ResumeGame();
            return;
        }

        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            ResumeGame();
            return;
        }

        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            PauseMenu.Instance.HidePause();
            ResumeGame();
            return;
        }

        // Hiçbiri açık değilse pause aç
        PauseMenu.Instance.ShowPause();
        GameStateManager.SetPaused(true);
    }

    // ======================================================
    // TRADE
    // ======================================================
    private void OpenTrade()
    {
        if (activeTradeNPC == null) return;

        if (tradePanel != null)
            tradePanel.SetActive(true);

        activeTradeNPC.OpenTrade();
        GameStateManager.SetPaused(true);
    }

    public void SetActiveTradeNPC(NPCTradeInteract npc)
    {
        activeTradeNPC = npc;
    }

    // ======================================================
    // CRAFT
    // ======================================================
    private void ToggleCraft()
    {
        if (craftPanel == null) return;

        bool open = !craftPanel.activeSelf;
        craftPanel.SetActive(open);
        GameStateManager.SetPaused(open);
    }

    public void ForceCloseCraft()
    {
        if (craftPanel != null && craftPanel.activeSelf)
        {
            craftPanel.SetActive(false);
            ResumeGame();
        }
    }

    // ======================================================
    // HELPERS
    // ======================================================
    private bool IsAnyBlockingPanelOpen()
    {
        return (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            || (tradePanel != null && tradePanel.activeSelf)
            || (craftPanel != null && craftPanel.activeSelf);
    }

    private void ResumeGame()
    {
        GameStateManager.SetPaused(false);
        Time.timeScale = 1f;
    }

    public PlayerControls GetControls()
    {
        return controls;
    }

    // ======================================================
    // UNUSED INPUTS (Input System gereği)
    // ======================================================
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
