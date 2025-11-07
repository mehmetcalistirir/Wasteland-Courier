using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private Enemy enemyParent;

    private void Awake()
    {
        enemyParent = GetComponentInParent<Enemy>();
        if (enemyParent == null)
            Debug.LogError("[EnemyHitbox] Parent'ta Enemy scripti yok!");

        // Güvenlik: Collider trigger olmalı
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyParent == null) return;

        if (other.CompareTag("Player"))
        {
            // Yeni sistem: temas ettiğinde saldırıyı başlat
            if (enemyParent.enemyType == EnemyType.Normal || enemyParent.enemyType == EnemyType.Fast)
            {
                // Düşman oyuncuya değdiğinde artık direkt saldırı coroutine'i çağrılıyor
                if (!enemyParent.isAttacking)
                    enemyParent.StartCoroutine(enemyParent.AttackPlayerRoutine());
            }
        }
        else if (other.CompareTag("Caravan"))
        {
            if (enemyParent.enemyType == EnemyType.Armored)
                enemyParent.StartCaravanDamage(other.transform);
            else if (enemyParent.enemyType == EnemyType.Exploder)
                enemyParent.Explode(); // Patlayıcıysa temasta patlat
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (enemyParent == null) return;

        if (other.CompareTag("Caravan"))
            enemyParent.StopCaravanDamage();
    }

    // İstersen sürekli temas varken de saldırı denemesi yapılabilir:
    private void OnTriggerStay2D(Collider2D other)
    {
        if (enemyParent == null) return;

        if (other.CompareTag("Player"))
        {
            if ((enemyParent.enemyType == EnemyType.Normal || enemyParent.enemyType == EnemyType.Fast) && !enemyParent.isAttacking)
            {
                // Temas devam ederken hâlâ saldırmıyorsa tekrar saldır
                enemyParent.StartCoroutine(enemyParent.AttackPlayerRoutine());
            }
        }
        else if (other.CompareTag("Caravan"))
        {
            if (enemyParent.enemyType == EnemyType.Armored)
                enemyParent.StartCaravanDamage(other.transform);
        }
    }
}
