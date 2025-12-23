using UnityEngine;
using UnityEngine.InputSystem;

public class MapToggle : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject mapPanel;
    public RectTransform mapRect;
    public RectTransform playerMarker;

    [Header("Oyuncu ve Ayarlar")]
    public Transform player;
    public float mapScale = 1f;

    private bool isMapVisible = false;
    private Vector3 mapBottomLeft;

    private InputAction mapAction;


private void OnEnable()
{
    var gameplay = PlayerInputRouter.Instance
        .inputActions
        .FindActionMap("Gameplay");

    mapAction = gameplay.FindAction("Map");

    mapAction.performed += OnMapPressed;
    mapAction.canceled += OnMapReleased;

    gameplay.Enable();

    if (mapPanel != null)
        mapPanel.SetActive(false);
}


private void OnDisable()
{
    if (mapAction == null) return;

    mapAction.performed -= OnMapPressed;
    mapAction.canceled -= OnMapReleased;
}


    private void OnMapPressed(InputAction.CallbackContext context)
    {
        if (mapPanel == null) return;

        mapPanel.SetActive(true);
        isMapVisible = true;
        Debug.Log("🗺️ Harita açıldı (Tab basıldı)");
    }

    private void OnMapReleased(InputAction.CallbackContext context)
    {
        if (mapPanel == null) return;

        mapPanel.SetActive(false);
        isMapVisible = false;
        Debug.Log("❌ Harita kapandı (Tab bırakıldı)");
    }

    private void Update()
    {
        if (isMapVisible)
            UpdatePlayerMarker();
    }

    private void UpdatePlayerMarker()
    {
        if (player == null || playerMarker == null || mapRect == null) return;

        // 2D pozisyon hesaplama
        Vector3 worldOffset = player.position;
        Vector2 mapPos = new Vector2(worldOffset.x, worldOffset.y) * mapScale;

        Vector3[] corners = new Vector3[4];
        mapRect.GetWorldCorners(corners);
        mapBottomLeft = corners[0];

        Vector2 anchored = new Vector2(
            mapPos.x + (mapRect.rect.width / 2),
            mapPos.y + (mapRect.rect.height / 2)
        );

        playerMarker.anchoredPosition = anchored;
    }
}
