using System.Collections;
using TMPro;
using UnityEngine;

public class AmmoCheckUI : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text text;
    public float showTime = 1.2f;

    Coroutine co;

    private void Awake()
    {
        HideInstant();
    }

    public void Show(string msg)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowRoutine(msg));
    }

    IEnumerator ShowRoutine(string msg)
    {
        text.text = msg;
        group.alpha = 1f;
        group.blocksRaycasts = false;
        group.interactable = false;

        yield return new WaitForSeconds(showTime);

        HideInstant();
    }

    void HideInstant()
    {
        if (group != null) group.alpha = 0f;
        if (text != null) text.text = "";
    }
}
