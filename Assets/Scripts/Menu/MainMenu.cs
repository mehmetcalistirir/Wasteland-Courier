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

    // Durumlar
    private bool isInSettings = false;
    private bool isInLevels = false;
    private bool isInCredits = false;

    private void Start()
    {
        StartCoroutine(PlayMusicWithDelay(0.5f));

        // ✔️ Yeni SaveSystem’e göre "Devam Et" butonunu aktif/pasif yap
        var continueBtn = GameObject.Find("Continue Button")?.GetComponent<UnityEngine.UI.Button>();
        if (continueBtn != null)
            continueBtn.interactable = SaveSystem.HasSave();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isInSettings) { CloseSettings(); return; }
            if (isInCredits) { CloseCredits(); return; }
            if (isInLevels) { CloseLevels(); return; }
        }
    }

    private System.Collections.IEnumerator PlayMusicWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (musicSource && menuMusic)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // -----------------------------
    //        OYUN BAŞLATMA
    // -----------------------------

    // ✔️ Yeni oyun = eski kaydı sil
    public void YeniOyunaBasla()
    {
        SaveSystem.DeleteSave();
        SaveBootstrap.ShouldLoadFromSave = false;
        SceneManager.LoadScene("Bolum1");
    }

    // ✔️ Kayda göre devam et
    public void DevamEt()
    {
        if (!SaveSystem.HasSave())
            return;

        SaveBootstrap.ShouldLoadFromSave = true;
        SceneManager.LoadScene("Bolum1");  // hep aynı oyun sahnesine giriyoruz
    }


    // -----------------------------
    //        PANEL SİSTEMİ
    // -----------------------------

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
        isInLevels = false;  // ❗ Daha önce burada yanlışlıkla isInCredits = false deniyordu
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
