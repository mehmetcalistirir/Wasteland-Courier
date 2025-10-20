using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCollector : MonoBehaviour
{
    public float collectRange = 1.5f;
    private List<Resource> highlightedResources = new List<Resource>();

    void Update()
    {
        // Önceki vurguları kaldır
        foreach (var res in highlightedResources)
        {
            if (res != null)
                res.Highlight(false);
        }
        highlightedResources.Clear();

        // Yeni yakındaki kaynakları bul
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRange);
        foreach (var hit in hits)
        {
            Resource res = hit.GetComponent<Resource>();
            if (res != null)
            {
                res.Highlight(true);
                highlightedResources.Add(res);
            }
        }

        // E tuşu ile toplama
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E tuşuna basıldı!");
            foreach (var res in highlightedResources)
            {
                if (res != null)
                {
                    res.Collect();
                    break;
                }
            }
        }
    }
}
