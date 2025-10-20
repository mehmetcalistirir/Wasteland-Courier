// MainMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip menuMusic;

    [Header("UI Panelleri")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    // 🔹 Yeni:
    public GameObject creditsPanel;
    public GameObject levelPanel;

    private bool isInSettings = false;
    private bool isInLevels = false;

    // 🔹 Yeni:
    private bool isInCredits = false;


    public void YeniOyunaBasla()
    {
        PlayerPrefs.DeleteAll(); // tüm ilerlemeyi sıfırla
        SceneManager.LoadScene("Bolum1");
    }

    public void DevamEt()
    {
        int lastLevel = PlayerPrefs.GetInt("LastLevel", 1);
        SceneManager.LoadScene("Bolum" + lastLevel);
    }

    private void Start()
    {
        StartCoroutine(PlayMusicWithDelay(0.5f));
        // Eğer hiç kayıt yoksa "Devam Et" butonunu pasifleştir
        if (!PlayerPrefs.HasKey("LastLevel"))
        {
            GameObject.Find("Continue Button").GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }

    private void Update()
    {
        // Ayarlardayken ESC -> geri
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

        if (musicSource != null && menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StartGame()
    {
        GameStateManager.ResetGameState(); // <-- oyun durumu sıfırlanır
        Time.timeScale = 1f;
        SceneManager.LoadScene("Bolum1");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null && mainPanel != null)
        {
            mainPanel.SetActive(false);
            settingsPanel.SetActive(true);
            isInSettings = true;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && mainPanel != null)
        {
            settingsPanel.SetActive(false);
            mainPanel.SetActive(true);
            isInSettings = false;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenLevels()
    {
        if (levelPanel != null && mainPanel != null)
        {
            mainPanel.SetActive(false);
            levelPanel.SetActive(true);
            isInLevels = true;
        }
    }

    public void CloseLevels()
    {
        if (levelPanel != null && mainPanel != null)
        {
            levelPanel.SetActive(false);
            mainPanel.SetActive(true);
            isInCredits = false;
        }
    }

    // 🔹 Credits aç/kapat
    public void OpenCredits()
    {
        if (creditsPanel != null && mainPanel != null)
        {
            mainPanel.SetActive(false);
            creditsPanel.SetActive(true);
            isInCredits = true;
        }
    }

    public void CloseCredits()
    {
        if (creditsPanel != null && mainPanel != null)
        {
            creditsPanel.SetActive(false);
            mainPanel.SetActive(true);
            isInCredits = false;
        }
    }
}
