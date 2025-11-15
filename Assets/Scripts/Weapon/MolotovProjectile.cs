using System.Collections;
using UnityEngine;

public class MolotovProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject fireEffectPrefab;
    public float explosionRadius = 2.5f;
    public int impactDamage = 20;
    public int burnDamagePerSecond = 5;
    public float fireDuration = 5f;

    private bool hasExploded = false;

   private void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"🔥 Trigger tetiklendi: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

    if (hasExploded) return;

    // 💡 Sadece zemine çarpınca patla
    if (other.gameObject.layer == LayerMask.NameToLayer("GroundTrigger"))
    {
        hasExploded = true;
        Debug.Log("💥 Molotov yere çarptı, patlıyor!");
        Explode();
    }
    else
    {
        // Diğer layer'lar (Animal, Build vs.) tamamen görmezden gel
        Debug.Log($"⏭ {other.name} yoksayıldı (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
    }
}



    private void Explode()
    {
        Debug.Log("💥 Molotov yere çarptı, patlıyor!");

        // 🔥 1. Fire effect oluştur
        if (fireEffectPrefab != null)
        {
            GameObject fire = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fire, fireDuration);
            Debug.Log("🔥 Fire effect oluşturuldu!");
        }

        // 💢 2. İlk patlama hasarı
        ApplyDamage(impactDamage);

        // 🔥 3. Yanma alanı oluştur
        StartCoroutine(CreateBurnZone());

        // 🧨 4. Molotov objesini sahneden kaldır (görünmez yap)
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
    }

private void ApplyDamage(int damage)
{
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
    foreach (var hit in hits)
    {
        if (hit.CompareTag("Enemy"))
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"💥 Patlama hasarı: {damage} verildi -> {enemy.name}");
            }
        }
    }
}

    private IEnumerator CreateBurnZone()
    {
        Debug.Log("🔥 Yanma alanı oluşturuldu!");
        float elapsed = 0f;

        // Geçici alan objesi oluştur
        GameObject burnZone = new GameObject("BurnZone");
        burnZone.transform.position = transform.position;
        CircleCollider2D col = burnZone.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = explosionRadius;

        // 2D rigidbody (Trigger çalışması için şart)
        Rigidbody2D rb = burnZone.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;

        // Hasar script’i ekle
        BurnZone zone = burnZone.AddComponent<BurnZone>();
        zone.damagePerSecond = burnDamagePerSecond;
        zone.duration = fireDuration;

        // Fire bitince alanı kaldır
        Destroy(burnZone, fireDuration);

        yield return null;
    }


    private IEnumerator BurnDamageOverTime()
    {
        float elapsed = 0f;
        while (elapsed < fireDuration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemy = hit.GetComponent<Enemy>();
                    if (enemy != null) enemy.TakeDamage(burnDamagePerSecond);
                }
            }
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }
}
