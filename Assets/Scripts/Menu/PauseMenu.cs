using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    public GameObject pausePanel;
    public GameObject settingsPanel;
    private bool justClosedSettings = false;


    public static bool IsPaused { get; private set; }

    private PlayerControls controls;

    private void Awake()
    {
        Instance = this;

        controls = new PlayerControls();
        controls.Gameplay.Escape.performed += ctx => OnEscapePressed();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

   private void OnEscapePressed()
{
    // ESC koruması (SettingsPanel yeni kapandıysa)
    if (justClosedSettings)
    {
        justClosedSettings = false; // Bir kere blokla
        return;
    }

    if (settingsPanel.activeSelf)
    {
        CloseSettings();
        return;
    }

    if (IsPaused)
        ResumeGame();
    else
        PauseGame();
}

    public void PauseGame()
    {
        IsPaused = true;
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        // Settings açıldığında PauseMenu gizlenir ama oyun duraklamaya devam eder
        IsPaused = true;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

   public void CloseSettings()
{
    // Sadece SettingsPanel kapanır
    settingsPanel.SetActive(false);

    // PauseMenuPanel tekrar görünür olmalı
    pausePanel.SetActive(true);

    // Oyun pause durumda kalmalı
    IsPaused = true;
    Time.timeScale = 0f;

    // ESC’nin hemen PauseMenu’yu kapatmaması için 0.2 sn koruma
    justClosedSettings = true;
    Invoke(nameof(ResetSettingsCloseFlag), 0.2f);
}


private void ResetSettingsCloseFlag()
{
    justClosedSettings = false;
}


}
