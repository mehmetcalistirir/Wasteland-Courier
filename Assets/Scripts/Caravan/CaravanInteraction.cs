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
        Debug.Log("Caravan → Player MENZİLE GİRDİ");
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.GetComponentInParent<PlayerMovement>() != null)
    {
        playerInRange = false;
        Debug.Log("Caravan → Player MENZİLDEN ÇIKTI");
        PlayerInputRouter.Instance?.ForceCloseCraft();
    }
}

}
