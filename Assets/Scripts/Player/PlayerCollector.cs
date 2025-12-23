using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collect Settings")]
    public float collectRange = 1.5f;

    private List<Collectible> highlighted = new List<Collectible>();

    private InputAction interactAction;


private void OnEnable()
{
    var gameplay = PlayerInputRouter.Instance
        .inputActions
        .FindActionMap("Gameplay");

    interactAction = gameplay.FindAction("Interact");

    interactAction.performed += OnInteract;
}


private void OnDisable()
{
    if (interactAction != null)
        interactAction.performed -= OnInteract;
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
private void OnInteract(InputAction.CallbackContext context)
{
    foreach (var c in highlighted)
    {
        if (c != null)
        {
            c.Collect();
            break; // sadece bir tane al
        }
    }
}


}
