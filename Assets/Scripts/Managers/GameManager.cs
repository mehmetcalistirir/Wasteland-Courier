using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int requiredFuel = 4;
    private int currentFuel = 0;

    public GameObject gameOverPanel;

    private void Start()
{
    var player = GameObject.FindWithTag("Player");
    if (player != null)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.onDeath += GameOver;   // 🔥 Ölünce GameOver tetikleniyor
            Debug.Log("✔ Player death event GameManager'a bağlandı.");
        }
        else
        {
            Debug.LogError("❌ PlayerStats component bulunamadı!");
        }
    }
    else
    {
        Debug.LogError("❌ 'Player' tag'ına sahip obje bulunamadı!");
    }
}

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "MainMenu")
        return;

    // Yeni Sahne → DayNightCycle var mı?
    var cycle = FindObjectOfType<DayNightCycle>();
    if (cycle != null)
    {
        Debug.Log("🔥 SceneLoaded → ResetCycle() çağrılıyor!");
        cycle.ResetCycle();
    }
    else
    {
        Debug.LogError("❌ DayNightCycle SAHNEDE YOK!");
    }
}


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
             SceneManager.sceneLoaded += OnSceneLoaded;

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
    Debug.Log("🏁 Ana menüye dönülüyor...");

    // Oyun hızını sıfırla
    Time.timeScale = 1f;

    // GameState reset
    GameStateManager.IsGameOver = false;
    GameStateManager.ResetGameState();

    // 🔥 SAHNEDEKİ TÜM SESLERİ DURDUR (Bolum1 dahil)
    StopAllSceneAudio();

    // Ana Menü sahnesine geç
    SceneManager.LoadScene("MainMenu");
}

private void StopAllSceneAudio()
{
    AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();

    foreach (AudioSource audio in allAudioSources)
    {
        audio.Stop();
        audio.enabled = false;   // 🔥 Müzik tekrar başlamasın
    }

    Debug.Log("🔇 Bolum1 içindeki TÜM sesler durduruldu!");
}



}
