using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public GameObject panel;
    public Slider masterSlider;

    public Slider musicSlider;
    public Slider sfxSlider;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    private void Start()
    {
        // Ayarları yükle
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        masterSlider.onValueChanged.AddListener(SetMasterVolume);

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        ApplySettings();

        // Dinleyiciler
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);

        // Eğer ayarlar menüsü pause menüsünden açıldıysa, geri dön
        if (PauseMenu.Instance != null && PauseMenu.IsPaused)
        {
            PauseMenu.Instance.CloseSettings();
        }
    }

    void SetMasterVolume(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
    }


    void SetMusicVolume(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    void SetSFXVolume(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void ApplySettings()
    {
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetQuality(qualityDropdown.value);
        SetFullscreen(fullscreenToggle.isOn);
    }
}
