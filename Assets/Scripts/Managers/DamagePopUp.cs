using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    private float floatSpeed = 25f;
    private float fadeDuration = 1.2f;
    private float elapsed = 0f;
    private Color startColor = Color.red;

    public void Setup(int damage)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        if (textMesh == null)
        {
            Debug.LogError("❌ TextMeshProUGUI bileşeni yok!");
            return;
        }

        textMesh.text = damage.ToString();
        textMesh.color = startColor;
    }

    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        elapsed += Time.deltaTime;

        if (textMesh != null)
        {
            float t = elapsed / fadeDuration;
            textMesh.color = Color.Lerp(startColor, Color.clear, t);
        }

        if (elapsed >= fadeDuration)
            Destroy(gameObject);
    }
}
