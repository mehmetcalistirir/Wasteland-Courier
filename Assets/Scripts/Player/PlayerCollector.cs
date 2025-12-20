using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCollector : MonoBehaviour
{
    public float collectRange = 1.5f;
    private List<Collectible> highlighted = new List<Collectible>();

    void Update()
    {
        // eski vurguları kapat
        foreach (var c in highlighted)
            if (c) c.Highlight(false);
        highlighted.Clear();

        // yenileri bul
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRange);
        foreach (var hit in hits)
        {
            var c = hit.GetComponent<Collectible>();
            if (c != null)
            {
                c.Highlight(true);
                highlighted.Add(c);
            }
        }

        // E ile toplama
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            foreach (var c in highlighted)
            {
                if (c != null)
                {
                    c.Collect();
                    break;
                }
            }
        }
    }
}
