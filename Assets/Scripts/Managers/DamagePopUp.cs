using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    private RectTransform rect;
    private float floatSpeed = 40f;
    private float fadeDuration = 1.2f;
    private float elapsed = 0f;

    private Color startColor = Color.red;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Setup(int damage)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        textMesh.text = damage.ToString();
        textMesh.color = startColor;
    }

    void Update()
    {
        // 🔥 UI UZAYINDA YUKARI KAY
        rect.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

        elapsed += Time.deltaTime;

        float t = elapsed / fadeDuration;
        textMesh.color = Color.Lerp(startColor, Color.clear, t);

        if (elapsed >= fadeDuration)
            Destroy(gameObject);
    }
}
