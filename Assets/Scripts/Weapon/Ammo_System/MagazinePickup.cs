using UnityEngine;

public class MagazinePickup : MonoBehaviour
{
    public MagazineData magazineData;
    public int startAmmo = 0; // loot dengesi

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Inventory inv = Inventory.Instance;
        if (inv == null)
            return;

        // Yeni şarjör oluştur
        MagazineInstance mag = new MagazineInstance(magazineData);
        mag.currentAmmo = Mathf.Clamp(startAmmo, 0, magazineData.capacity);

        // Envantere ekle
        if (!inv.TryAddMagazine(mag))
            return;

        // PlayerWeapon al (DOĞRU YER)
        PlayerWeapon pw = other.GetComponent<PlayerWeapon>();
        if (pw != null)
        {
            pw.CollectMagazinesFromInventory();
        }

        Destroy(gameObject);
    }
}
