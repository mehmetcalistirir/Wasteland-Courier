using UnityEngine;

public class CaravanInteraction : MonoBehaviour
{
    [Header("Ayarlar")]
    public float interactDistance = 2f;

    [Header("Durum")]
    public bool playerInRange = false;   // CraftInput burayı kontrol edecek

    private Transform player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(player.position, transform.position);

        // Oyuncu karavana yakın mı?
        playerInRange = dist <= interactDistance;
    }
}
