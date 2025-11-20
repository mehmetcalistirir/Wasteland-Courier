// Assets/Scripts/Managers/GameStateManager.cs
public static class GameStateManager
{
    public static bool IsGamePaused =>
        PauseMenu.IsPaused ||
        NPCInteraction.IsTradeOpen;
    public static bool IsGameOver = false;

    public static void ResetGameState()
    {
        IsGameOver = false;
    }


}
