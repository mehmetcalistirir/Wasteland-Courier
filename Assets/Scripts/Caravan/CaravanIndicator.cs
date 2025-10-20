using UnityEngine;
using UnityEngine.UI;

public class CaravanIndicator : MonoBehaviour
{
    public Transform player;
    public Transform caravan;
    public RectTransform canvasRect;
    public RectTransform indicator;

    private CanvasGroup canvasGroup;

    [Header("Animasyon Ayarları")]
    public float fadeSpeed = 5f;
    public float scaleSpeed = 5f;
    public float targetAlpha = 1f;
    public Vector3 visibleScale = Vector3.one;
    public Vector3 hiddenScale = Vector3.zero;

    void Start()
    {
        canvasGroup = indicator.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.LogError("❌ CanvasGroup eksik! Lütfen CaravanIndicator objesine ekleyin.");

        // Başlangıç durumu
        canvasGroup.alpha = 0f;
        indicator.localScale = hiddenScale;
    }

    void Update()
    {
        if (player == null || caravan == null || indicator == null || canvasRect == null) return;

        Vector3 direction = caravan.position - player.position;

        // Kamera görünürlüğü kontrolü
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(caravan.position);
        bool isCaravanVisible = viewportPos.x >= 0f && viewportPos.x <= 1f &&
                               viewportPos.y >= 0f && viewportPos.y <= 1f &&
                               viewportPos.z > 0f;

        bool shouldShow = !isCaravanVisible;

        // Fade ve Scale animasyonu
        float target = shouldShow ? targetAlpha : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, target, Time.deltaTime * fadeSpeed);
        indicator.localScale = Vector3.Lerp(indicator.localScale, shouldShow ? visibleScale : hiddenScale, Time.deltaTime * scaleSpeed);

        if (!shouldShow) return;

        // Yön gösterimi
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, angle - 90f);

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(player.position + direction.normalized * 5f);
        screenPoint = new Vector3(Mathf.Clamp01(screenPoint.x), Mathf.Clamp01(screenPoint.y), 0);

        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 uiPos = new Vector2(
            (screenPoint.x * canvasSize.x) - (canvasSize.x / 2),
            (screenPoint.y * canvasSize.y) - (canvasSize.y / 2)
        );

        indicator.anchoredPosition = uiPos;
    }
}
