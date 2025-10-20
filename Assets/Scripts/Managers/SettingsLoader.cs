using UnityEngine;

public class SettingsLoader : MonoBehaviour
{
    void Awake()
    {
        // Bu objeyi sahne değişse bile yok etme
        DontDestroyOnLoad(gameObject);

        // Müzik sesi
        AudioListener.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // Grafik kalitesi
        QualitySettings.SetQualityLevel(
            PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel())
        );

        // Tam ekran
        Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }
}
