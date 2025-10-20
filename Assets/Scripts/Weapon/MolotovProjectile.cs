using System.Collections;
using UnityEngine;

public class MolotovProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject fireEffectPrefab;
    public float explosionRadius = 2.5f;
    public int impactDamage = 20;          // 💥 İlk çarpmada verilen hasar
    public int burnDamagePerSecond = 5;    // 🔥 Yanma hasarı (her saniye)
    public float fireDuration = 5f;        // ⏱️ Yanma süresi (sn)

    private bool hasExploded = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"💥 Molotov çarptı -> {collision.gameObject.name}");

        // 🔥 1. Fire effect oluştur
        if (fireEffectPrefab != null)
        {
            GameObject fire = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fire, fireDuration);
            Debug.Log("🔥 Fire effect oluşturuldu!");
        }
        else
        {
            Debug.LogWarning("⚠️ Fire effect prefab atanmadı!");
        }

        // 💢 2. Yakınındaki düşmanlara anlık çarpma hasarı ver
        ApplyDamage(impactDamage);

        // 🔥 3. Sürekli yanma hasarı ver
        StartCoroutine(ApplyBurnDamageOverTime());

        // 🧨 4. Molotov objesini sahneden kaldır (görünmez yap, fizik kapat)
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
    }

    // 🎯 Yakınındaki düşmanlara hasar uygulayan yardımcı fonksiyon
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
                    Debug.Log($"🔥 {enemy.name} hasar aldı: {damage}");
                }
            }
        }
    }

    // 🔥 Yanma süresince hasar uygulayan coroutine
    private IEnumerator ApplyBurnDamageOverTime()
    {
        float elapsed = 0f;
        while (elapsed < fireDuration)
        {
            ApplyDamage(burnDamagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        Debug.Log($"🔥 Yanma süresi ({fireDuration}s) bitti, molotov kaldırıldı.");
        Destroy(gameObject);
    }

    // 🎨 Scene içinde etki alanını görebilmek için
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
