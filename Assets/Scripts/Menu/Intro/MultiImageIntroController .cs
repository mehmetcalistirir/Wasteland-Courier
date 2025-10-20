using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiImageIntroController : MonoBehaviour
{
    [System.Serializable]
    public class IntroStep
    {
        public Image image;             // Görsel
        public AudioClip soundEffect;   // O anda çalacak ses
    }

    public IntroStep[] steps;             // Tüm adımlar
    public float fadeDuration = 1f;
    public float displayTime = 2f;
    public string nextSceneName = "MainMenu";

    public AudioSource audioSource;      // Ses oynatıcı

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // Başta tüm görselleri kapat
        foreach (var step in steps)
        {
            step.image.gameObject.SetActive(false);
            SetAlpha(step.image, 0f);
        }

        foreach (var step in steps)
        {
            step.image.gameObject.SetActive(true);

            if (step.soundEffect != null && audioSource != null)
            {
                yield return new WaitForSeconds(1f); // 1 saniye bekle
                audioSource.clip = step.soundEffect;
                audioSource.Play();
            }


            yield return StartCoroutine(FadeImage(step.image, 0f, 1f));
            yield return new WaitForSeconds(displayTime);
            yield return StartCoroutine(FadeImage(step.image, 1f, 0f));

            step.image.gameObject.SetActive(false);
        }

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
