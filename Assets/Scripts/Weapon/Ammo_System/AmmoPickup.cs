using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public AmmoType ammoType;
    public int amount = 15;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;

        if (Vector3.Distance(player.position, transform.position) < 1.5f)
        {
            Inventory.Instance.AddAmmo(ammoType.ammoId, amount);
            Destroy(gameObject);
        }
    }
}
