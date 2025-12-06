using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiImageIntroController : MonoBehaviour
{
    [System.Serializable]
    public class IntroStep
    {
        public Image image;             // Gösterilecek görsel
        public AudioClip soundEffect;   // Çalınacak ses (isteğe bağlı)
        public float beforeDelay = 1f;  // Görsel görünmeden önce bekleme süresi
        public float displayTime = 2f;  // Görsel ekranda ne kadar kalacak
    }

    public IntroStep[] steps;

    [Header("Fade Ayarları")]
    public float fadeDuration = 1f;

    [Header("Sahne Ayarları")]
    public string nextSceneName = "MainMenu";

    [Header("Ses Kaynağı")]
    public AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1) Başlarken tüm görselleri görünmez yap
        foreach (var step in steps)
        {
            if (step.image != null)
            {
                SetAlpha(step.image, 0f);
                step.image.gameObject.SetActive(false);
            }
        }

        // 2) Sırasıyla intro adımlarını çalıştır
        foreach (var step in steps)
        {
            if (step.image == null)
                continue;

            // Başlamadan önce bekleme süresi
            if (step.beforeDelay > 0f)
                yield return new WaitForSeconds(step.beforeDelay);

            // Görseli aktif hale getir
            step.image.gameObject.SetActive(true);

            // Ses çal
            if (step.soundEffect != null && audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = step.soundEffect;
                audioSource.Play();
            }

            // Fade-in
            yield return StartCoroutine(FadeImage(step.image, 0f, 1f));

            // Görsel ekranda sabit kalsın
            yield return new WaitForSeconds(step.displayTime);

            // Fade-out
            yield return StartCoroutine(FadeImage(step.image, 1f, 0f));

            step.image.gameObject.SetActive(false);
        }

        // 3) Tüm intro bitti → Sahne değiştir
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeImage(Image img, float startAlpha, float endAlpha)
    {
        Color color = img.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            img.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Son değer
        color.a = endAlpha;
        img.color = color;
    }

    private void SetAlpha(Image img, float alpha)
    {
        Color color = img.color;
        color.a = alpha;
        img.color = color;
    }
}
