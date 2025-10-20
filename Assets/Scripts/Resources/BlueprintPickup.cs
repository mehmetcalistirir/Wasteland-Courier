using UnityEngine;

public class BlueprintPickup : MonoBehaviour
{
    [Header("Blueprint Item")]
    public ItemData blueprintSO;   // ✅ Artık string id yerine direkt ItemData

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (blueprintSO != null)
            {
                Inventory.Instance.TryAdd(blueprintSO, 1);
                Debug.Log($"📜 Blueprint eklendi: {blueprintSO.itemName}");
            }
            else
            {
                Debug.LogError("[BlueprintPickup] blueprintSO atanmadı!");
            }

            Destroy(gameObject);
        }
    }
}
