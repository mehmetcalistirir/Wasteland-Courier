using UnityEngine;
using UnityEngine.UI;

public class CastleIndicator : MonoBehaviour
{
    public Transform player;
    public Transform targetCastle;
    public RectTransform canvasRect;
    public RectTransform indicator;

    private CanvasGroup canvasGroup;
    public float hideDistance = 5f;   // Oyuncu kaleye 5 birimden yakınsa ok görünmez


    public float fadeSpeed = 5f;
    public float scaleSpeed = 5f;
    public float screenEdgeOffset = 100f;

    void Start()
    {
        canvasGroup = indicator.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        indicator.localScale = Vector3.zero;
    }

    void Update()
    {
        float dist = Vector2.Distance(player.position, targetCastle.position);
        if (dist < hideDistance)
        {
            // Ok tamamen gizlensin
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            indicator.localScale = Vector3.Lerp(indicator.localScale, Vector3.zero, Time.deltaTime * scaleSpeed);
            return;
        }

        if (player == null || targetCastle == null) return;

        Vector3 viewport = Camera.main.WorldToViewportPoint(targetCastle.position);

        bool onScreen =
            viewport.x > 0f && viewport.x < 1f &&
            viewport.y > 0f && viewport.y < 1f &&
            viewport.z > 0f;

        bool shouldShow = !onScreen;

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, shouldShow ? 1f : 0f, Time.deltaTime * fadeSpeed);
        indicator.localScale = Vector3.Lerp(indicator.localScale, shouldShow ? Vector3.one : Vector3.zero, Time.deltaTime * scaleSpeed);

        if (!shouldShow) return;

        // Yön
        Vector3 dir = targetCastle.position - player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // Ekran pozisyonu
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetCastle.position);

        // Clamp
        screenPos.x = Mathf.Clamp(screenPos.x, screenEdgeOffset, Screen.width - screenEdgeOffset);
        screenPos.y = Mathf.Clamp(screenPos.y, screenEdgeOffset, Screen.height - screenEdgeOffset);

        // Canvas UI pozisyonu
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 uiPos);
        indicator.anchoredPosition = uiPos;
    }
}
