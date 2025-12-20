using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collect Settings")]
    public float collectRange = 1.5f;

    private List<Collectible> highlighted = new List<Collectible>();
    private PlayerControls controls;

    private void Awake()
    {
        // Input Actions instance
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Collect.performed += OnCollect;
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Collect.performed -= OnCollect;
        controls.Gameplay.Disable();
    }

    private void Update()
    {
        UpdateHighlight();
    }

    // ---------------- HIGHLIGHT ----------------
    private void UpdateHighlight()
    {
        // Eski vurguları kapat
        foreach (var c in highlighted)
            if (c) c.Highlight(false);

        highlighted.Clear();

        // Yenileri bul
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
    }

    // ---------------- COLLECT ----------------
    private void OnCollect(InputAction.CallbackContext context)
    {
        foreach (var c in highlighted)
        {
            if (c != null)
            {
                c.Collect();
                break; // sadece en yakın / ilk item
            }
        }
    }

}
