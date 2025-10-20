using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData ammoData;  // Her prefab için atanacak ScriptableObject
    public int itemCount = 1;       // Kaç tane item eklenecek (örn. 1 kutu = ammoPerItem kadar mermi)

    [Header("Pickup Settings")]
    public float pickupRange = 1.5f; // Oyuncuya yaklaşma mesafesi
    public float rotateSpeed = 60f;  // Görsel döndürme efekti (isteğe bağlı)

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Döndürme efekti (isteğe bağlı)
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        // Oyuncuya yeterince yakınsa otomatik alınsın
        if (Vector3.Distance(transform.position, player.position) <= pickupRange)
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        Debug.Log($"[Pickup] {gameObject.name} pickup çalıştı. AmmoData={(ammoData != null ? ammoData.name : "NULL")}");


        if (ammoData == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} için AmmoItemData atanmadı!");
            Destroy(gameObject);
            return;
        }

        bool added = Inventory.Instance.TryAdd(ammoData, itemCount);

        if (added)
        {
            Debug.Log($"✅ {ammoData.itemName} envantere eklendi!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("❌ Envanter dolu, ammo alınamadı!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
