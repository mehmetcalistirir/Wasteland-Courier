using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public float fakeLoadSpeed = 0.7f;

    public TMP_Text loadingText;
    public float interval = 0.4f;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }


    private void OnEnable()
    {
        StartCoroutine(AnimateText());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }


    IEnumerator AnimateText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(interval);
        }
    }
    IEnumerator LoadSceneAsync()
    {
        string sceneToLoad = PlayerPrefs.GetString("SceneToLoad", "Bolum1");

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (!op.isDone)
        {
            // Gerçek progress (0–0.9 arası gelir)
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);

            // Fake progress ile yumuşatma
            fakeProgress = Mathf.MoveTowards(
                fakeProgress,
                realProgress,
                Time.deltaTime * fakeLoadSpeed
            );

            progressBar.value = fakeProgress;

            // Yükleme tamamlandıysa
            if (fakeProgress >= 0.99f && op.progress >= 0.9f)
            {
                progressBar.value = 1f;
                yield return new WaitForSeconds(0.3f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
