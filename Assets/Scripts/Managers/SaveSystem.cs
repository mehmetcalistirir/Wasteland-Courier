using UnityEngine;

public static class SaveSystem
{
    public static void MarkLevelComplete(int levelIndex)
    {
        PlayerPrefs.SetInt($"Level_{levelIndex}_Completed", 1);
        PlayerPrefs.Save();
    }

    public static bool IsLevelCompleted(int levelIndex)
    {
        return PlayerPrefs.GetInt($"Level_{levelIndex}_Completed", 0) == 1;
    }
}
