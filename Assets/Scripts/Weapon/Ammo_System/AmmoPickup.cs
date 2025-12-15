using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public AmmoTypeData ammoType;
    public int amount = 15;
    public float pickupRange = 1.5f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player || ammoType == null || Inventory.Instance == null) return;

        if (Vector3.Distance(player.position, transform.position) <= pickupRange)
        {
            Inventory.Instance.AddAmmo(ammoType, amount);
            Destroy(gameObject);
        }
    }
}

