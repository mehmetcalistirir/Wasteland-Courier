using UnityEngine;

public class BarrierPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerAbilities ab = col.GetComponent<PlayerAbilities>();

            PlayerAbilityUI.instance.AddAbilityButton(
                AbilityType.PlaceBarrier,
                ab.StartPlacingBarrier
            );

            Destroy(gameObject);
        }
    }
}
