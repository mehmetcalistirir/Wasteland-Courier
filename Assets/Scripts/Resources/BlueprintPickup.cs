using UnityEngine;

public class BlueprintPickup : MonoBehaviour
{
    public string blueprintId;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.UnlockBlueprint(blueprintId);
            }

            Destroy(gameObject);
        }
    }
}
