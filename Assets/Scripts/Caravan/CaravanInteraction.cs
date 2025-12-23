using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CaravanInteraction : MonoBehaviour
{
    public bool playerInRange { get; private set; }

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        // Craft panel AÇIKKEN prompt gösterme
        if (playerInRange && !PlayerInputRouter.Instance.craftPanel.activeSelf)
        {
            InteractionPromptUI.Instance?.Show("Craft Paneli");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerInRange = true;
            InteractionPromptUI.Instance?.Show("Craft Paneli");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerInRange = false;
            InteractionPromptUI.Instance?.Hide();
            PlayerInputRouter.Instance?.ForceCloseCraft();
        }
    }
}
