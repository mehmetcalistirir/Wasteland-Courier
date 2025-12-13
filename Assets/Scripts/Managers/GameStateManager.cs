using UnityEngine;
public static class GameStateManager
{
    public static bool IsGamePaused { get; private set; }
    public static bool IsGameOver { get; set; }

    public static void SetPaused(bool paused)
    {
        IsGamePaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public static void ResetGameState()
    {
        IsGameOver = false;
        SetPaused(false);
    }
}
