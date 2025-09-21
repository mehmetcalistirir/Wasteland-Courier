using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image fillImage;
    public PlayerStats playerStats;

    void Start()
    {
        if (playerStats != null)
        {
            playerStats.onHealthChanged += UpdateHealthBar;
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.onHealthChanged -= UpdateHealthBar;
        }
    }

    void UpdateHealthBar(int current, int max)
    {
        fillImage.fillAmount = Mathf.Clamp01((float)current / max);
    }
}
