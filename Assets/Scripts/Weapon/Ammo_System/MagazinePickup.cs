using UnityEngine;

public class MagazinePickup : MonoBehaviour
{
    public MagazineData magazineData;
    public int startAmmo = 0; // loot dengesi için

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        MagazineInstance mag = new MagazineInstance(magazineData);
        mag.currentAmmo = Mathf.Clamp(startAmmo, 0, magazineData.capacity);

        Inventory.Instance.TryAddMagazine(mag);
        Destroy(gameObject);
    }
}
