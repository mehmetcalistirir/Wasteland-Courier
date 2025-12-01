using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    public GameObject pausePanel;
    public GameObject settingsPanel;

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
        Debug.Log("ESC INPUT");

        // 🌟 1) SETTINGS PANEL AÇIKSA → SADECE ONU KAPAT
        if (settingsPanel.activeSelf)
        {
            Debug.Log("SettingsPanel kapanıyor...");
            CloseSettings();
            return;    // PauseMenu'ya ASLA dokunma
        }

        // 🌟 2) SETTINGS PANEL KAPALIYSA → NORMAL PAUSE MENÜ TOGGLE
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
        // Sadece SettingsPanel kapanır, PauseMenu'ya dönülmez
        settingsPanel.SetActive(false);
        // pausePanel.SetActive(true);  ❌ BUNU ÖZELLİKLE KOYMUYORUZ
    }
}
