using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Bolum1");
    }

    public void OpenSettings()
    {
        Debug.Log("⚙️ Ayarlar açıldı.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
