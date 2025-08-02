using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 3f;
    private Rigidbody2D rb;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Animal"))
            return;

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
        else if (collision.CompareTag("Animal"))
        {
            Animal animal = collision.GetComponent<Animal>();
            if (animal != null)
                animal.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
