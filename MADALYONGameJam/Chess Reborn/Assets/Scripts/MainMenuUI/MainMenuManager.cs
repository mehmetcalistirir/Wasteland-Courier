using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("UI Elements To Hide")]
    public GameObject[] uiElements; // Settings dışında kalan UI öğeleri

    void Start()
    {
        settingsPanel.SetActive(false);
        ShowMainUI(true);
    }

    // ---------------------------------------------------------
    // Tüm UI elemanlarını aç/kapat
    // ---------------------------------------------------------
    private void ShowMainUI(bool state)
    {
        foreach (GameObject obj in uiElements)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }

    // ---------------------------------------------------------
    // Settings Aç
    // ---------------------------------------------------------
    public void OpenSettings()
    {
        ShowMainUI(false);         // diğer UI'lar kapanır
        settingsPanel.SetActive(true);
    }

    // ---------------------------------------------------------
    // Settings Kapat
    // ---------------------------------------------------------
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        ShowMainUI(true);          // diğer UI'lar geri gelir
    }

    // ---------------------------------------------------------
    // Play
    // ---------------------------------------------------------
    public void StartGame()
    {
        SceneManager.LoadScene("Oyun Sahnesi");
    }

    // ---------------------------------------------------------
    // Quit
    // ---------------------------------------------------------
    public void QuitGame()
    {
        Debug.Log("Oyun kapatılıyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
