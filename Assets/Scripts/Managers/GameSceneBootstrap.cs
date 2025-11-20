// GameSceneBootstrap.cs (örnek)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameSceneBootstrap : MonoBehaviour
{
    public bool hideCursorInGame = true;
    public CursorLockMode lockMode = CursorLockMode.Confined;

    void Awake()
    {
        // Oyun akışı açık olsun
        Time.timeScale = 1f;

        // Cursor ayarı
        Cursor.visible   = !hideCursorInGame;
        Cursor.lockState = lockMode;

        // 🔧 HATA VEREN SATIRI SİL:
        // PauseMenu.IsPaused = false;

        // ✅ Yerine şunu kullan:
        var pm = PauseMenu.Instance ?? FindObjectOfType<PauseMenu>();
        if (pm != null) pm.ResumeGame();

        // (İsteğe bağlı) Diğer panelleri de kapat
        if (NPCInteraction.Instance != null)       NPCInteraction.Instance.CloseTradePanel();

        // PlayerInput action map'i Gameplay'e zorla (kullanıyorsan)
        foreach (var pi in FindObjectsOfType<PlayerInput>(includeInactive: true))
        {
            if (pi.actions != null && pi.actions.FindActionMap("Gameplay") != null)
                pi.SwitchCurrentActionMap("Gameplay");
        }

        // EventSystem çakışmasını önle
        var allEventSystems = FindObjectsOfType<EventSystem>();
        for (int i = 1; i < allEventSystems.Length; i++)
            Destroy(allEventSystems[i].gameObject);
    }
}
