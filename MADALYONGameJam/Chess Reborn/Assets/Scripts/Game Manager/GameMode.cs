using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance;

    [Header("Castle References")]
    public BaseController playerCastle;
    public BaseController enemyCastle;

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    void Awake()
    {
        Instance = this;
    }

    // ------------------------------------------------------
    // TÜM OYUN İÇİ PANELLERİ KAPAT
    // ------------------------------------------------------
    private void CloseAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Eğer başka UI panellerin varsa buraya ekleyebilirsin:
        // commandPanel.SetActive(false);
        // abilityPanel.SetActive(false);
        // questPanel.SetActive(false);
    }

    public void CheckCastleWinLose(BaseController castle)
    {
        if (castle == playerCastle && castle.owner == Team.Enemy)
            LoseGame();

        if (castle == enemyCastle && castle.owner == Team.Player)
            WinGame();
    }

    public void WinGame()
    {
        CloseAllPanels();    // 🔥 önce her şeyi kapat
        Time.timeScale = 0f;
        winPanel.SetActive(true);
    }

    public void LoseGame()
    {
        CloseAllPanels();    // 🔥 önce her şeyi kapat
        Time.timeScale = 0f;
        losePanel.SetActive(true);
    }

    public void RestartGame()
    {
        MusicManager.instance.StopMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("AnaMenu");
    }
}
