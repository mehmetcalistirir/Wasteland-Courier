using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CreditsAutoScroll : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;

    [Header("Ayarlar")]
    public float speed = 20f;      // px/sn
    public float topHold = 0.5f;   // başlamadan önce bekleme
    public float bottomHold = 1f;  // en altta bekleme (kapanmadan önce)

    [Header("Bitiş Davranışı")]
    public bool closePanelOnFinish = false;
    public GameObject panelToClose;             // credits paneli
    public UnityEvent onFinished;               // bittiğinde tetiklenir

    private RectTransform contentRT;
    private RectTransform viewportRT;
    private bool running;

    void OnEnable()
    {
        if (scrollRect == null || scrollRect.content == null) return;

        // Viewport boş bırakılmışsa ilk çocuk olarak ayarla
        if (scrollRect.viewport == null)
            scrollRect.viewport = scrollRect.transform.GetChild(0).GetComponent<RectTransform>();

        contentRT  = scrollRect.content;
        viewportRT = scrollRect.viewport;

        StartCoroutine(ScrollRoutine());
    }

    System.Collections.IEnumerator ScrollRoutine()
    {
        running = true;

        // Başta en üste getir
        scrollRect.verticalNormalizedPosition = 1f;

        // Layout otursun
        yield return new WaitForSeconds(topHold);
        yield return null; // 1 frame

        // İçerik görünür alandan kısa ise çık
        float h = Mathf.Max(0f, contentRT.rect.height - viewportRT.rect.height);
        if (h <= 1f) { Finish(); yield break; }

        // Akış: 1 -> 0
        while (running && contentRT != null)
        {
            // hız (px/sn) -> normalized adım
            float step = (speed * Time.unscaledDeltaTime) / h;
            scrollRect.verticalNormalizedPosition -= step;

            if (scrollRect.verticalNormalizedPosition <= 0f)
            {
                scrollRect.verticalNormalizedPosition = 0f;
                // En alta ulaştık => bekle ve bitir
                if (bottomHold > 0f) yield return new WaitForSeconds(bottomHold);
                Finish();
                yield break;
            }

            yield return null;
        }
    }

    void Finish()
    {
        running = false;
        onFinished?.Invoke();

        if (closePanelOnFinish && panelToClose != null)
            panelToClose.SetActive(false);
    }

    void OnDisable() { running = false; }
}
