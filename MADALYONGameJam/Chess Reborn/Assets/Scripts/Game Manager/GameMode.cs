using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance;

    [Header("Castle References")]
    public BaseController playerCastle;   // ✔ EKLENDİ
    public BaseController enemyCastle;    // ✔ EKLENDİ

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckCastleWinLose(BaseController castle)
    {
        // OYUN KAYIP
        if (castle == playerCastle && castle.owner == Team.Enemy)
        {
            LoseGame();
        }

        // OYUN KAZANÇ
        if (castle == enemyCastle && castle.owner == Team.Player)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        Time.timeScale = 0f;
        winPanel.SetActive(true);
    }

    public void LoseGame()
    {
        Time.timeScale = 0f;
        losePanel.SetActive(true);
    }

   public void RestartGame()
{
    // 🔥 MÜZİĞİ ANINDA KES
    MusicManager.instance.StopMusic();

    // 🔥 SONRA SAHNEYİ YENİDEN YÜKLE
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
    );
}


    public void GoMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("AnaMenu");
    }
}
