using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    public GameObject interactionPrompt;
    public static bool IsPlayerInRange { get; private set; }

    private void Awake()
    {
        IsPlayerInRange = false;
    }

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInRange = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (WeaponCraftingSystem.Instance != null)
            {
                WeaponCraftingSystem.Instance.CloseCraftingPanel();
                Time.timeScale = 1f; // oyun devam etsin
            }
        }
    }
}
