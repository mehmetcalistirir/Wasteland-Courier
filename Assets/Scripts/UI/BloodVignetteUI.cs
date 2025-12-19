using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodVignetteUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image bloodOverlay;
    private float baseAlpha;   // cana bağlı kalıcı
    private float flashAlpha;  // geçici hasar efekti

    [Header("Health Input")]
    [Tooltip("0..1 arası sağlık yüzdesini buraya veriyoruz.")]
    [Range(0f, 1f)] public float health01 = 1f; // demo için inspector'dan oynatabilirsin

    [Header("Intensity")]
    [Tooltip("Health azaldıkça alpha nasıl artsın? X: health(0..1), Y: alpha(0..1)")]

    [SerializeField]
private AnimationCurve alphaByHealth =
    new AnimationCurve(
        new Keyframe(1.0f, 0.00f),
        new Keyframe(0.70f, 0.3f), // full HP
        new Keyframe(0.60f, 0.4f),
        new Keyframe(0.40f, 0.5f),
        new Keyframe(0.25f, 0.6f),
        new Keyframe(0.10f, 0.7f),
        new Keyframe(0.0f, 1f)  // DİKKAT: max değer
    );



    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.55f;
    [SerializeField] private float smooth = 10f;

    [Header("Low HP Pulse (COD hissi)")]
    [SerializeField, Range(0f, 1f)] private float pulseStartHealth = 0.22f;
    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float pulseAmount = 0.07f;

    [Header("Damage Flash")]
    [SerializeField] private float damageFlashAdd = 0.18f;
    [SerializeField] private float damageFlashTime = 0.08f;

    private float currentAlpha;
    private Coroutine flashCo;

    void Reset()
    {
        if (!bloodOverlay) bloodOverlay = GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (!bloodOverlay) return;

        float h = Mathf.Clamp01(health01);

        float target = Mathf.Clamp01(alphaByHealth.Evaluate(h));


        // Düşük candayken küçük nabız/pulse
        if (h <= pulseStartHealth)
        {
            float t = (pulseStartHealth - h) / Mathf.Max(pulseStartHealth, 0.0001f); // 0..1
            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f); // 0..1
            target += pulse * pulseAmount * t;
        }

        target = Mathf.Clamp01(target);

        baseAlpha = target;

        // final alpha = base + flash
        float finalAlpha = Mathf.Clamp01(baseAlpha + flashAlpha);

        currentAlpha = Mathf.Lerp(
            currentAlpha,
            finalAlpha,
            1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime)
        );


        var c = bloodOverlay.color;
        c.a = currentAlpha;
        bloodOverlay.color = c;
    }

    /// <summary>
    /// Hasar aldığında kısa bir "kan sıçraması" / flash eklemek için çağır.
    /// </summary>
    public void OnDamageFlash()
    {
        if (!bloodOverlay) return;
        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        float start = flashAlpha;
        float peak = Mathf.Clamp01(start + damageFlashAdd);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(damageFlashTime, 0.001f);
            flashAlpha = Mathf.Lerp(start, peak, t);
            yield return null;
        }

        // geri sönme
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(damageFlashTime, 0.001f);
            flashAlpha = Mathf.Lerp(peak, 0f, t);
            yield return null;
        }

        flashAlpha = 0f;
        flashCo = null;
    }

}
