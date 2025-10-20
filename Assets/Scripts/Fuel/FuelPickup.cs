using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddFuel(1);
            Destroy(gameObject);
        }
    }
}
