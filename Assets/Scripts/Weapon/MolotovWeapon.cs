using UnityEngine;

public class MolotovWeapon : PlayerWeapon
{
    [Header("Molotov Settings")]
    public GameObject molotovProjectilePrefab;
    public Transform throwPoint;
    public float throwForce = 10f;

    public void Shoot()
    {
        if (molotovProjectilePrefab == null || throwPoint == null)
            return;

        GameObject molotov = Instantiate(molotovProjectilePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody2D rb = molotov.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(throwPoint.up * throwForce, ForceMode2D.Impulse);

        Debug.Log("Molotov fırlatıldı!");

        // Dayanıklılık gideri
        base.Shoot();
    }
}
