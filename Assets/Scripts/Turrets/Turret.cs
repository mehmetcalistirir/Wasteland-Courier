using UnityEngine;

public class Turret : MonoBehaviour
{
    public float attackRange = 5f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    private float fireTimer = 0f;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && fireTimer <= 0)
            {
                Shoot(enemy.transform.position);
                fireTimer = 1f / fireRate;
                break; // İlk düşmanı vur, çık
            }
        }
    }

void Shoot(Vector3 targetPosition)
{
    Vector3 direction = (targetPosition - transform.position).normalized;

    // Yön doğrultusunda bir rotasyon hesapla (X yönüne göre döndür)
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    Quaternion rotation = Quaternion.Euler(0, 0, angle); // ⬅️ sadece bu önemli

    // Mermiyi doğru rotasyonla oluştur
    GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);
}




    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
