using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public static bool IsPaused { get; private set; }

    private PlayerInput playerInput;
    private bool isInSettings = false;

    public static PauseMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
{
    if (Keyboard.current.escapeKey.wasPressedThisFrame)
    {
        // Trade panel açıksa → kapat
        if (NPCInteraction.IsTradeOpen)
        {
            NPCInteraction.Instance.CloseTradePanel();
            Time.timeScale = 1f;
            return;
        }

        // Ayarlardayken ESC → pause menüsüne dön
        if (isInSettings)
        {
            CloseSettings();
            return;
        }

        // Pause toggle
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }
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
        isInSettings = false;
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
        isInSettings = true;
        Debug.Log("⚙️ Ayarlar paneli açıldı.");
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        isInSettings = false;
        Debug.Log("⬅️ Ayarlardan çıkıldı, Pause menüsüne dönüldü.");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
