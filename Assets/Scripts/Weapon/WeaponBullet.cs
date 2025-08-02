// WeaponBullet.cs (DÜZELTİLMİŞ VE FİZİK UYUMLU HALİ)

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Bu script'in olduğu objede Rigidbody2D olmasını zorunlu kılar.
public class WeaponBullet : MonoBehaviour
{
    public float speed = 20f; // Hızı biraz artıralım, daha gerçekçi olur.
    public int damage = 10;
    private Rigidbody2D rb;

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
        // Oyuncuya veya diğer mermilere çarpmasını engellemek için kontrol ekleyebiliriz (isteğe bağlı).
        if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
        {
            return; // Hiçbir şey yapma
        }

        // Düşman bileşeni var mı kontrol et. Düşmanların "Enemy" tag'ine sahip olduğunu varsayıyoruz.
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Çarptıktan sonra mermiyi yok et.
        // Bu, merminin birden fazla düşmana hasar vermesini engeller.
        Destroy(gameObject);
    }
}