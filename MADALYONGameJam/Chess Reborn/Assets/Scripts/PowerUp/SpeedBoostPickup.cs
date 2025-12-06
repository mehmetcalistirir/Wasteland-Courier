using UnityEngine;

public class SpeedBoostPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerAbilities ab = col.GetComponent<PlayerAbilities>();

            PlayerAbilityUI.instance.AddAbilityButton(
                AbilityType.SpeedBoost,
                ab.ActivateSpeedBoost
            );

            Destroy(gameObject);
        }
    }
}
