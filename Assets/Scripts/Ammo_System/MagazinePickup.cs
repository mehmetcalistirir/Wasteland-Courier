using UnityEngine;

public class MagazinePickup : MonoBehaviour
{
    public MagazineData magazineData;

    [Header("Start Ammo Range")]
    public int minStartAmmo = 0;
    public int maxStartAmmo = 0;

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

        int randomAmmo = Random.Range(minStartAmmo, maxStartAmmo + 1);
        mag.currentAmmo = Mathf.Clamp(randomAmmo, 0, magazineData.capacity);

        Inventory.Instance.TryAddMagazine(mag);

        pw?.CollectMagazinesFromInventory();

        Destroy(gameObject);
    }
}
