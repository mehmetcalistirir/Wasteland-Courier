// WeaponBullet.cs (DÜZELTİLMİŞ VE FİZİK UYUMLU HALİ)

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Bu script'in olduğu objede Rigidbody2D olmasını zorunlu kılar.
public class WeaponBullet : MonoBehaviour
{
    public float speed = 20f; // Hızı biraz artıralım, daha gerçekçi olur.
    public int damage = 10;
    private Rigidbody2D rb;

    public Transform owner;  // PlayerWeapon atar

public float fleeOnHitDuration = 3.5f;
public float fleeOnHitMultiplier = 2.2f;

    void Awake()
    {
        // Rigidbody2D referansını en başta al, daha performanslıdır.
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Mermiye ileri doğru (kendi sağına) anlık bir hız ver.
        // Bu, Update içinde sürekli hareket ettirmekten daha doğru ve performanslıdır.
        rb.linearVelocity = transform.right * speed;

        // Mermi ekranda kaybolursa diye 3 saniye sonra kendini yok etsin.
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player") || collision.CompareTag("Bullet")) return;

    Enemy enemy = collision.GetComponent<Enemy>();
    if (enemy != null) enemy.TakeDamage(damage);

    Animal animal = collision.GetComponent<Animal>();
    if (animal != null)
    {
        // 📌 Çarpma noktasından (veya oyuncudan) uzağa kaç
        Vector2 threatPos = (owner != null) ? (Vector2)owner.position : (Vector2)transform.position;
        animal.Scare(threatPos, fleeOnHitDuration, fleeOnHitMultiplier);

        animal.TakeDamage(damage);
    }

    Destroy(gameObject);
}

}