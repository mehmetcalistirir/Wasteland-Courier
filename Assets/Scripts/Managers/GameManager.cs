using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int requiredFuel = 4;
    private int currentFuel = 0;

    public GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddFuel(int amount)
    {
        currentFuel += amount;
        Debug.Log($"⛽ Yakıt toplandı: {currentFuel}/{requiredFuel}");
    }

    public bool HasAllFuel()
    {
        return currentFuel >= requiredFuel;
    }

    public void LoadNextScene()
    {
        Debug.Log("🚚 Tüm yakıtlar toplandı, sonraki sahneye geçiliyor...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void GameOver()
    {
        Debug.Log("🏚️ Oyun Bitti!");
        Time.timeScale = 0f; // Oyunu durdur
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Menü sahnenin ismi bu olmalı
    }
}
