using UnityEngine;

public class MagazinePickup : MonoBehaviour
{
    public MagazineData magazineData;
    public int startAmmo = 0;

    private PlayerWeapon pw;

    private void Awake()
    {
        pw = FindObjectOfType<PlayerWeapon>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (magazineData == null) return;

        MagazineInstance mag = new MagazineInstance(magazineData);
        mag.currentAmmo = Mathf.Clamp(startAmmo, 0, magazineData.capacity);

        Inventory.Instance.TryAddMagazine(mag);

        pw?.CollectMagazinesFromInventory(); // ✅ pickup sonrası liste güncellensin

        Destroy(gameObject);
    }
}
