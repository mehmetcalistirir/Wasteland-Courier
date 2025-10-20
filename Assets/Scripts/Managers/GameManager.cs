using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SaveSystem.MarkLevelComplete(currentIndex);

        // 🧠 Oyuncunun en son oynadığı bölümü hatırla
        PlayerPrefs.SetInt("LastLevel", currentIndex + 1);
        PlayerPrefs.Save();

        // 🎬 Sonraki sahneye geç
        SceneManager.LoadScene(currentIndex + 1);
    }

    public void GameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;

            GameStateManager.IsGameOver = true;

            // Oyuncu inputunu kapat
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var input = player.GetComponent<PlayerInput>();
                if (input != null) input.enabled = false;

                var controller = player.GetComponent<PlayerMovement>();
                if (controller != null) controller.enabled = false;
            }
        }
    }


    public void RestartGame()
    {
        GameStateManager.IsGameOver = false;
        GameStateManager.ResetGameState();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Menü sahnenin ismi bu olmalı
    }
}
