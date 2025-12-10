// Assets/Scripts/Managers/GameStateManager.cs
public static class GameStateManager
{
    public static bool IsGamePaused =>
        PauseMenu.IsPaused ||
        (UIPanelSystem.Instance != null && UIPanelSystem.Instance.IsPanelOpen());
    public static bool IsGameOver = false;

    public static void ResetGameState()
    {
        IsGameOver = false;
    }


}
