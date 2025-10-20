using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private Enemy enemyParent;

    private void Awake()
    {
        enemyParent = GetComponentInParent<Enemy>();
        if (enemyParent == null)
            Debug.LogError("[EnemyHitbox] Parent'ta Enemy scripti yok!");
        
        // Güvenlik: Collider kesinlikle trigger olmalı
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyParent == null) return;

        if (other.CompareTag("Player"))
        {
            // Normal/Fast oyuncuya yakınken hasar
            if (enemyParent.enemyType == EnemyType.Normal || enemyParent.enemyType == EnemyType.Fast)
                enemyParent.StartPlayerDamage(other.transform);
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

        if (other.CompareTag("Player"))
            enemyParent.StopPlayerDamage();

        if (other.CompareTag("Caravan"))
            enemyParent.StopCaravanDamage();
    }

    // İstersen sürekli temas varken periyodik hasar için Enter yerine Stay de kullanılabilir:
    private void OnTriggerStay2D(Collider2D other)
    {
        if (enemyParent == null) return;

        if (other.CompareTag("Player"))
        {
            if (enemyParent.enemyType == EnemyType.Normal || enemyParent.enemyType == EnemyType.Fast)
                enemyParent.StartPlayerDamage(other.transform); // Start içinde guard zaten var
        }
        else if (other.CompareTag("Caravan"))
        {
            if (enemyParent.enemyType == EnemyType.Armored)
                enemyParent.StartCaravanDamage(other.transform);
        }
    }
}
