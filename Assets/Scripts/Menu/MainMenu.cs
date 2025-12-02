using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Müzik")]
    public AudioSource musicSource;
    public AudioClip menuMusic;

    [Header("UI Panelleri")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject levelPanel;

    private bool isInSettings = false;
    private bool isInLevels = false;
    private bool isInCredits = false;

    private void Start()
    {
        // 🔥 Oyun sahnesinden gelen MusicManager varsa yok et
        if (MusicManager.Instance != null)
            Destroy(MusicManager.Instance.gameObject);

        // 🔥 TimeScale sıfır kalmış olabilir. Menüde kesinlikle 1 olsun.
        Time.timeScale = 1f;

        StartCoroutine(PlayMusicWithDelay(0.25f));  

        // Devam Et butonunu aktif/pasif yap
        var continueBtn = GameObject.Find("Continue Button")?.GetComponent<UnityEngine.UI.Button>();
        if (continueBtn != null)
            continueBtn.interactable = SaveSystem.HasSave();
    }

    private System.Collections.IEnumerator PlayMusicWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (musicSource != null && menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.volume = 1f;

            // Mixer çıkışını bypass et → test için en temiz yöntem
            musicSource.outputAudioMixerGroup = null;

            musicSource.Play();
            Debug.Log("🎵 Ana Menü müziği BAŞLADI!");
        }
        else
        {
            Debug.LogError("❌ musicSource veya menuMusic atanmadı!");
        }
    }

    // -----------------------------------
    //           OYUN BAŞLATMA
    // -----------------------------------

    public void YeniOyunaBasla()
    {
        SaveSystem.DeleteSave();
        SaveBootstrap.ShouldLoadFromSave = false;
        SceneManager.LoadScene("Bolum1");
    }

    public void DevamEt()
    {
        if (!SaveSystem.HasSave())
            return;

        SaveBootstrap.ShouldLoadFromSave = true;
        SceneManager.LoadScene("Bolum1");
    }

    // -----------------------------------
    //           PANEL KONTROLÜ
    // -----------------------------------
    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        isInSettings = true;
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
        isInSettings = false;
    }

    public void OpenLevels()
    {
        mainPanel.SetActive(false);
        levelPanel.SetActive(true);
        isInLevels = true;
    }

    public void CloseLevels()
    {
        levelPanel.SetActive(false);
        mainPanel.SetActive(true);
        isInLevels = false;
    }

    public void OpenCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
        isInCredits = true;
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
        isInCredits = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
