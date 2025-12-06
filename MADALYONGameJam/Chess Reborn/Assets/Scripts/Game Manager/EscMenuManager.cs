using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject escMenuPanel;
    public GameObject settingsPanel;

    private PlayerControls controls;
    private bool isOpen = false;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Pause.performed += ctx => OnPause();
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Start()
    {
        escMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnPause()
    {
        // Ayarlar açıksa ESC ile önce ayarlardan çık
        if (settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            escMenuPanel.SetActive(true);
            return;
        }

        ToggleMenu();
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        escMenuPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;
    }

    public void ResumeGame()
    {
        escMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isOpen = false;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        escMenuPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}
