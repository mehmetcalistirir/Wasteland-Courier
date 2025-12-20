using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Müzik")]
    public AudioSource musicSource;
    public AudioClip menuMusic;

    [Header("UI Panelleri")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    void Start()
    {
        // Menüde oyun müziği kalmasın
        if (MusicManager.Instance != null)
            Destroy(MusicManager.Instance.gameObject);

        Time.timeScale = 1f;

        PlayMenuMusic();

        InitContinueButton();
    }

    // ---------------- MUSIC ----------------

    void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null)
        {
            Debug.LogError("❌ Menü müziği atanmadı!");
            return;
        }

        musicSource.clip = menuMusic;
        musicSource.loop = true;

        // 1. ADIM: Mixer grubunu AudioManager'dan alıp atayın
        if (AudioManager.Instance != null)
        {
            musicSource.outputAudioMixerGroup = AudioManager.Instance.musicGroup;
        }

        // 2. ADIM: AudioSource volume değerini 1 yapın. 
        // Ses seviyesini artık Mixer (desibel olarak) yönetecek.
        musicSource.volume = 1f;

        musicSource.Play();
    }

    // ---------------- BUTTONS ----------------

    void InitContinueButton()
    {
        var btn = GameObject.Find("Continue Button")
            ?.GetComponent<UnityEngine.UI.Button>();

        if (btn != null)
            btn.interactable = SaveSystem.HasSave();
    }

    public void YeniOyunaBasla()
    {
        SaveSystem.DeleteSave();
        SaveBootstrap.ShouldLoadFromSave = false;
        PlayerPrefs.SetString("SceneToLoad", "Bolum1");
        SceneManager.LoadScene("LoadingScene");
    }

    public void DevamEt()
    {
        if (!SaveSystem.HasSave()) return;

        SaveBootstrap.ShouldLoadFromSave = true;

        PlayerPrefs.SetString("SceneToLoad", "Bolum1");
        SceneManager.LoadScene("LoadingScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ---------------- PANELS ----------------

    public void OpenSettings()
    {
        OpenPanel(settingsPanel);
    }



    public void OpenCredits()
    {
        OpenPanel(creditsPanel);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    public void CloseSubPanel(GameObject panel)
    {
        panel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void OpenPanel(GameObject panel)
    {
        mainPanel.SetActive(false);
        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();
    }
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

}
