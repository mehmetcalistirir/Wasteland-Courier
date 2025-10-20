using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WeaponBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public int damage = 10;
    private Rigidbody2D rb;

    [Header("References")]
    public Transform owner;            // PlayerWeapon atar
    public WeaponType weaponType;      // 📌 Hangi silah türünden atıldığı buraya atanacak

    [Header("Animal Scare")]
    public float fleeOnHitDuration = 3.5f;
    public float fleeOnHitMultiplier = 2.2f;

     [Header("Knockback (applied always)")]
    public float knockbackForce = 6f;      // atanacak kuvvet (bullet'e atanır)
    public float knockbackDuration = 0.18f; // kuvvet uygulanma süresi

     private int enemiesHit = 0;
    private const int maxPenetration = 2; // 2 düşmandan sonra yok olur

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Bullet")) return;

        // 🎯 Düşmana çarptıysa hasar ver ve knockback kontrolü yap
         Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemiesHit++;

            int appliedDamage = damage;
            if (weaponType == WeaponType.Sniper && enemiesHit > 1)
                appliedDamage = Mathf.RoundToInt(damage * 0.5f); // ikinci hedefe yarı hasar

            enemy.TakeDamage(appliedDamage);

            // Knockback uygula (her zaman)
            if (weaponType == WeaponType.Shotgun || weaponType == WeaponType.Sniper)
            {
                Vector2 sourcePos = transform.position;
                enemy.ApplyKnockback(sourcePos, knockbackForce, knockbackDuration);
            }

            // 🔹 Eğer sniper mermisi 2 düşmana çarpmışsa yok ol
            if (weaponType == WeaponType.Sniper && enemiesHit >= maxPenetration)
            {
                Destroy(gameObject);
                return;
            }
            // 🔹 Diğer silahlar hemen yok olur
            else if (weaponType != WeaponType.Sniper)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 🐺 Hayvanlara korku efekti
        Animal animal = collision.GetComponent<Animal>();
        if (animal != null)
        {
            Vector2 threatPos = (owner != null) ? (Vector2)owner.position : (Vector2)transform.position;
            animal.Scare(threatPos, fleeOnHitDuration, fleeOnHitMultiplier);
            animal.TakeDamage(damage);
        }

        if (!collision.isTrigger)
        Destroy(gameObject);
    }
}
