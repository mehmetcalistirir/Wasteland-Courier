using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TurretBullet : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 5;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Mermiyi yönüne göre fırlat
        rb.linearVelocity = transform.right * speed;

        // 3 saniye sonra yok olsun
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncuya veya başka mermiye çarpmasın
        if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
            return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
