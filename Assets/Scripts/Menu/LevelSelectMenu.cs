using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;

            // ✅ DÜZELTİLDİ
            bool isUnlocked = (i == 0) || SaveSystem.IsLevelCompleted(i - 1);

            levelButtons[i].interactable = isUnlocked;
            levelButtons[i].onClick.AddListener(() => LoadLevel(index));
        }
    }


    void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene($"Bolum{levelIndex + 1}");
    }
}
