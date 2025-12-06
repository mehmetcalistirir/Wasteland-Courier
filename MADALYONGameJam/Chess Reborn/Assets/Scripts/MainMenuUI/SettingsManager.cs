using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;

    void Start()
    {
        // Oyun başladığında ayarlar paneli kapalı olsun
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}
