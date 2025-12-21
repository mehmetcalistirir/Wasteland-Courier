using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CaravanInteraction : MonoBehaviour
{
    public bool playerInRange { get; private set; }

    private void Awake()
    {
        // Güvenlik: collider trigger olmak zorunda
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.GetComponentInParent<PlayerMovement>() != null)
    {
        playerInRange = true;
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.GetComponentInParent<PlayerMovement>() != null)
    {
        playerInRange = false;
        PlayerInputRouter.Instance?.ForceCloseCraft();
    }
}

}
