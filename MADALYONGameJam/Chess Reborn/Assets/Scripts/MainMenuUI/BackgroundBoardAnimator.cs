using UnityEngine;
using UnityEngine.UI;

public class BackgroundBoardAnimator : MonoBehaviour
{
    [Header("Zoom Ayarları")]
    public float zoomSpeed = 0.2f;          // Yaklaşma/Uzaklaşma hızı
    public float zoomAmount = 0.15f;        // Ne kadar zoom yapacak
    private float zoomDirection = 1f;       // 1 = zoom-in, -1 = zoom-out
    private Vector3 originalScale;

    [Header("Kayma Ayarları")]
    public float driftSpeedX = 0.08f;       // Sağa doğru kayma hızı
    public float driftSpeedY = -0.04f;      // Aşağı doğru kayma hızı

    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    void Update()
    {
        AnimateZoom();
        AnimateDrift();
    }

    void AnimateZoom()
    {
        float targetScale = 1f + zoomAmount * zoomDirection;
        rect.localScale = Vector3.Lerp(rect.localScale, originalScale * targetScale, Time.deltaTime * zoomSpeed);

        // Hedef skalaya yaklaşınca yön değiştir
        if (Mathf.Abs(rect.localScale.x - originalScale.x * targetScale) < 0.01f)
        {
            zoomDirection *= -1f; // zoom in/out arasında geçiş yap
        }
    }

    void AnimateDrift()
    {
        rect.anchoredPosition += new Vector2(
            driftSpeedX * Time.deltaTime * 100f,
            driftSpeedY * Time.deltaTime * 100f
        );

        // Sonsuz loop efekti için konum reset
        if (rect.anchoredPosition.x > 2000f) rect.anchoredPosition = Vector2.zero;
        if (rect.anchoredPosition.y < -2000f) rect.anchoredPosition = Vector2.zero;
    }
}
