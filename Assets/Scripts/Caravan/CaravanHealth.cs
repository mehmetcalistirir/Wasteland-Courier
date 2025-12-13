using UnityEngine;
using UnityEngine.UI;

public class CaravanHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public Slider healthSlider;  // UI'dan bağlanacak

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Karavanın canı: " + currentHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log("🏚️ Karavan yıkıldı! Oyun bitti.");
            GameManager.Instance.GameOver();
        }

    }

    void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = (float)currentHealth / maxHealth;
    }
}
