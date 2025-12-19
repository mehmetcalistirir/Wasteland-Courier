using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections; // Coroutine için eklendi

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

    private Resolution[] resolutions;
    private bool initialized = false;

    // -------------------------------------------------
    // UNITY
    // -------------------------------------------------

    void Awake()
    {
        // Dinleyicileri (Listeners) sadece bir kez kaydetmek en güvenlisidir
        RegisterListeners();
    }

    void OnEnable()
    {
        // Sahne yeniden yüklendiğinde (Restart) UI ve Manager'ların 
        // tam uyanması için 1 kare bekleyerek yükleme yapıyoruz.
        StartCoroutine(SetupMenuDeferred());
    }

    private IEnumerator SetupMenuDeferred()
    {
        yield return null; // 1 frame bekle (Execution Order sorunlarını çözer)

        LoadPrefs();
        InitResolutionDropdown();
        RefreshVisuals(); // Görsel kaybolma sorununu çözer
    }

    // -------------------------------------------------
    // INIT & LOAD
    // -------------------------------------------------

    public void LoadPrefs()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("⚠️ AudioManager bulunamadı, yükleme geciktiriliyor...");
            return;
        }

        // Değerleri alırken varsayılan 1f (Tam ses) kullanıyoruz
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // SetValueWithoutNotify: Slider değerini değiştirir ama AudioManager'ı 
        // tetiklemez. Böylece açılışta PlayerPrefs üzerine 0 yazılmaz.
        masterSlider.SetValueWithoutNotify(master);
        musicSlider.SetValueWithoutNotify(music);
        sfxSlider.SetValueWithoutNotify(sfx);

        qualityDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel()));
        fullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Fullscreen", 1) == 1);

        Debug.Log($"✅ Ayarlar Başarıyla Yüklendi: Master {master}");
    }

    private void RefreshVisuals()
{
    if (panel != null)
    {
        // UI'yı anında yeniden çizmeye zorlar
        panel.SetActive(false);
        panel.SetActive(true); 
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
    }
}

    void RegisterListeners()
    {
        if (initialized) return;
        initialized = true;

        // Dinleyiciler AudioManager üzerinden sesleri anlık günceller
        masterSlider.onValueChanged.AddListener(val => {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(val);
        });
        musicSlider.onValueChanged.AddListener(val => {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
        });
        sfxSlider.onValueChanged.AddListener(val => {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);
        });

        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    // -------------------------------------------------
    // RESOLUTION & VIDEO
    // -------------------------------------------------

    void InitResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution r = resolutions[i];
            options.Add($"{r.width} x {r.height} ({r.refreshRateRatio.value:F0}Hz)");

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
