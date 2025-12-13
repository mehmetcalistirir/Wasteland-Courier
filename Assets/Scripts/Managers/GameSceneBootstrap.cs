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
    Time.timeScale = 1f;

    Cursor.visible   = !hideCursorInGame;
    Cursor.lockState = lockMode;

    // SADECE input map ayarı (pause state'e dokunmaz)
    foreach (var pi in FindObjectsOfType<PlayerInput>(includeInactive: true))
    {
        if (pi.actions != null && pi.actions.FindActionMap("Gameplay") != null)
            pi.SwitchCurrentActionMap("Gameplay");
    }

    // EventSystem temizliği
    var allEventSystems = FindObjectsOfType<EventSystem>();
    for (int i = 1; i < allEventSystems.Length; i++)
        Destroy(allEventSystems[i].gameObject);
}

}
