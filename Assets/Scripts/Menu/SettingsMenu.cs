using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    // ---------------- AUDIO ----------------
    [Header("Audio")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    // ---------------- VIDEO ----------------
    [Header("Video")]
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;

    private bool initialized = false;
    private int lastResolutionIndex = -1;

    // Sabit (retro / pixel-art uyumlu) çözünürlük listesi
    private readonly Vector2Int[] resolutions =
    {
        new(320,200), new(320,240), new(400,300), new(512,384),
        new(640,400), new(640,480), new(800,600), new(1024,768),
        new(1152,864), new(1280,600), new(1280,720),
        new(1280,768), new(1280,800), new(1280,960),
        new(1280,1024), new(1360,768), new(1366,768),
        new(1400,1050), new(1440,900), new(1600,900),
        new(1680,1050), new(1920,1080)
    };

    // -------------------------------------------------
    // UNITY
    // -------------------------------------------------

    private void Awake()
    {
        RegisterListeners();
    }

    private void OnEnable()
    {
        StartCoroutine(SetupMenuDeferred());
    }

    private IEnumerator SetupMenuDeferred()
{
    yield return null;

    InitResolutionDropdown();
    InitQualityDropdown(); // 🔥 EKLENDİ
    LoadPrefs();
}


    // -------------------------------------------------
    // INIT
    // -------------------------------------------------

    void RegisterListeners()
    {
        if (initialized) return;
        initialized = true;

        masterSlider.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetMasterVolume(v);
            PlayerPrefs.SetFloat("MasterVolume", v);
        });

        musicSlider.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetMusicVolume(v);
            PlayerPrefs.SetFloat("MusicVolume", v);
        });

        sfxSlider.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetSFXVolume(v);
            PlayerPrefs.SetFloat("SFXVolume", v);
        });

        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    // -------------------------------------------------
    // LOAD PREFS
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

        // 🔥 ÇÖZÜNÜRLÜK
        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        if (resIndex >= 0 && resIndex < resolutions.Length)
        {
            resolutionDropdown.SetValueWithoutNotify(resIndex);
            ApplyResolution(resIndex); // ❗ MUTLAKA
        }
    }


    // -------------------------------------------------
    // RESOLUTION
    // -------------------------------------------------

    void InitResolutionDropdown()
    {
        Debug.Log("InitResolutionDropdown ÇAĞRILDI");

        if (resolutionDropdown == null)
        {
            Debug.LogError("❌ resolutionDropdown NULL");
            return;
        }

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].x} x {resolutions[i].y}");
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        lastResolutionIndex = savedIndex;
        resolutionDropdown.SetValueWithoutNotify(savedIndex);
    }


    void SetResolution(int index)
    {
        if (index == lastResolutionIndex) return;
        lastResolutionIndex = index;

        ApplyResolution(index);
    }

    void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;

        Vector2Int r = resolutions[index];

        Screen.SetResolution(
    r.x,
    r.y,
    fullscreenToggle.isOn
        ? FullScreenMode.FullScreenWindow
        : FullScreenMode.Windowed
);

        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();

        Debug.Log($"📺 Resolution applied: {r.x}x{r.y}");
    }

    // -------------------------------------------------
    // VIDEO
    // -------------------------------------------------

    void SetFullscreen(bool value)
    {
        Screen.fullScreenMode = value
            ? FullScreenMode.ExclusiveFullScreen
            : FullScreenMode.Windowed;

        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();
    }

    // -------------------------------------------------
    // PANEL
    // -------------------------------------------------

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);

        if (PauseMenu.Instance != null && GameStateManager.IsGamePaused)
            PauseMenu.Instance.CloseSettings();
    }

    void InitQualityDropdown()
{
    if (qualityDropdown == null)
    {
        Debug.LogError("❌ qualityDropdown NULL");
        return;
    }

    qualityDropdown.ClearOptions();

    List<string> options = new List<string>();

    foreach (string q in QualitySettings.names)
        options.Add(q);

    qualityDropdown.AddOptions(options);

    int savedQuality = PlayerPrefs.GetInt(
        "QualityLevel",
        QualitySettings.GetQualityLevel()
    );

    qualityDropdown.SetValueWithoutNotify(savedQuality);
    qualityDropdown.RefreshShownValue();
}


}
