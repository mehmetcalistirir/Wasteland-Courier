using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [Header("Global Light")]
    public Light2D globalLight;

    [Header("Time Of Day")]
    [Range(0, 24)]
    public float timeOfDay = 8f;     // 0–24 arası saat
    public float daySpeed = 0.25f;   // Günün akış hızı

    [Header("Colors")]
    public Color morningColor = new Color(1f, 0.78f, 0.63f);   // Sabah
    public Color noonColor    = new Color(1f, 1f, 0.9f);       // Öğle
    public Color afternoonColor = new Color(1f, 0.86f, 0.7f);  // İkindi
    public Color eveningColor = new Color(1f, 0.63f, 0.47f);   // Akşam
    public Color nightColor   = new Color(0.16f, 0.23f, 0.47f); // Gece

    [Header("Intensity")]
    public float morningIntensity   = 0.8f;
    public float noonIntensity      = 1.0f;
    public float afternoonIntensity = 0.7f;
    public float eveningIntensity   = 0.5f;
    public float nightIntensity     = 0.25f;

    public float transitionSpeed = 2f;

    private Color targetColor;
    private float targetIntensity;

    // DayNightCycle burayı kullanıyor
    private bool isDay = true;

    private void Awake()
    {
        if (globalLight == null)
            globalLight = GetComponent<Light2D>();

        // Güvenli başlangıç
        SetDay(true);
    }

    private void Update()
    {
        if (globalLight == null) return;

        if (isDay)
        {
            // Sadece gündüz vakti saat akıyor
            timeOfDay += Time.deltaTime * daySpeed;

            // Gündüz aralığı: 06–21
            if (timeOfDay < 6f)  timeOfDay = 6f;
            if (timeOfDay > 21f) timeOfDay = 6f;

            // Sabah: 06–09
            if (timeOfDay >= 6f && timeOfDay < 9f)
            {
                targetColor = morningColor;
                targetIntensity = morningIntensity;
            }
            // Öğle: 09–15
            else if (timeOfDay >= 9f && timeOfDay < 15f)
            {
                targetColor = noonColor;
                targetIntensity = noonIntensity;
            }
            // İkindi: 15–18
            else if (timeOfDay >= 15f && timeOfDay < 18f)
            {
                targetColor = afternoonColor;
                targetIntensity = afternoonIntensity;
            }
            // Akşam: 18–21
            else if (timeOfDay >= 18f && timeOfDay < 21f)
            {
                targetColor = eveningColor;
                targetIntensity = eveningIntensity;
            }
        }
        else
        {
            // Gece modunda sabit değer
            targetColor = nightColor;
            targetIntensity = nightIntensity;
        }

        // Yumuşak geçiş
        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * transitionSpeed);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
    }

    /// <summary>
    /// DayNightCycle burayı çağırıyor.
    /// true = gündüz, false = gece
    /// </summary>
    public void SetDay(bool day)
    {
        isDay = day;

        if (!isDay)
        {
            // Geceye geçerken hedefleri direkt geceye çek
            targetColor = nightColor;
            targetIntensity = nightIntensity;
        }
        else
        {
            // Gündüze geçerken zamanı sabaha zorlayabilirsin
            if (timeOfDay < 6f || timeOfDay > 21f)
                timeOfDay = 6f; // sabah başlasın
        }
    }
}
