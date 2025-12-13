using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Panel")]
public GameObject panel;


    [Header("Video")]
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;
    bool initialized = false;

    // -------------------------------------------------
    // UNITY
    // -------------------------------------------------

    void Start()
    {
        LoadPrefs();
        RegisterListeners();
        ApplySettings();
    }

    void OnEnable()
    {
        // Panel her açıldığında resolution güvenli şekilde doldurulsun
        InitResolutionDropdown();
    }

    // -------------------------------------------------
    // INIT
    // -------------------------------------------------

    void LoadPrefs()
    {
        masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 1f));
        musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", 1f));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 1f));

        qualityDropdown.SetValueWithoutNotify(
            PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel())
        );

        fullscreenToggle.SetIsOnWithoutNotify(
            PlayerPrefs.GetInt("Fullscreen", 1) == 1
        );
    }

    void RegisterListeners()
    {
        if (initialized) return;
        initialized = true;

        if (AudioManager.Instance != null)
        {
            masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        }

        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    // -------------------------------------------------
    // APPLY
    // -------------------------------------------------

    void ApplySettings()
    {
        SetQuality(qualityDropdown.value);
        SetFullscreen(fullscreenToggle.isOn);

        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        SetResolution(resIndex);
    }

    // -------------------------------------------------
    // RESOLUTION
    // -------------------------------------------------

    void InitResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution r = resolutions[i];
            options.Add($"{r.width} x {r.height} ({r.refreshRate}Hz)");

            if (r.width == Screen.width && r.height == Screen.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentIndex);
        resolutionDropdown.SetValueWithoutNotify(savedIndex);
        resolutionDropdown.RefreshShownValue();
    }
public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);

        // Eğer ayarlar menüsü pause menüsünden açıldıysa, geri dön
        if (PauseMenu.Instance != null && GameStateManager.IsGamePaused)
        {
            PauseMenu.Instance.CloseSettings();
        }
    }

    void SetResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length)
            return;

        Resolution r = resolutions[index];

        Screen.SetResolution(
            r.width,
            r.height,
            fullscreenToggle.isOn
                ? FullScreenMode.ExclusiveFullScreen
                : FullScreenMode.Windowed
        );

        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();

        Debug.Log($"📺 Resolution applied: {r.width}x{r.height}");
    }

    // -------------------------------------------------
    // VIDEO
    // -------------------------------------------------

    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen
            ? FullScreenMode.ExclusiveFullScreen
            : FullScreenMode.Windowed;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }
}
