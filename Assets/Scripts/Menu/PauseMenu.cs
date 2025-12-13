using UnityEngine;
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject pausePanel;
    public GameObject settingsPanel;



    public void ShowPause()
    {
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void HidePause()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
