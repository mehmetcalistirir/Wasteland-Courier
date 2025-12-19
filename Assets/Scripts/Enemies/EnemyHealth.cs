using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public Image hpFillImage;

    private Enemy enemy;
    private EnemyLootDropper lootDropper;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        lootDropper = GetComponent<EnemyLootDropper>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UpdateHP();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHP()
    {
        if (hpFillImage)
            hpFillImage.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        lootDropper?.DropLoot(transform.position);
        enemy.OnDeath(); // Enemy’ye haber ver
        Destroy(gameObject);
    }
}
