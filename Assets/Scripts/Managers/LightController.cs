using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    public Light2D globalLight;
    public Color dayColor = new Color(1f, 0.95f, 0.8f);  // Gündüz sarısı
    public Color nightColor = new Color(0.2f, 0.25f, 0.4f); // Gece mavisi

    public float dayIntensity = 1f;
    public float nightIntensity = 0.4f;

    public float transitionSpeed = 1f;

    private Color targetColor;
    private float targetIntensity;

    void Start()
{
    // Oyun başlarken ışığın bir başlangıç değeri olsun
    SetDay(true); // veya DayNightCycle varsa onunla entegre
}


    public void SetDay(bool isDay)
    {
        targetColor = isDay ? dayColor : nightColor;
        targetIntensity = isDay ? dayIntensity : nightIntensity;
    }

    void Update()
    {
        if (globalLight == null) return;

        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * transitionSpeed);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
    }
}
