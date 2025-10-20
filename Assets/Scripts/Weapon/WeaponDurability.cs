using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WeaponDurability : MonoBehaviour
{
    [Header("Durability Settings")]
    public int maxDurability = 100;
    private int currentDurability;

    [Header("UI")]
    public GameObject durabilityBarPrefab;
    private GameObject barInstance;
    private Image fillImage;
    public int Current => currentDurability;

    
    [Header("Events")]
    public UnityEvent onBroken;   // slot yöneticisine haber


    void Start()
    {
        currentDurability = maxDurability;

        if (durabilityBarPrefab != null)
        {
            barInstance = Instantiate(durabilityBarPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            barInstance.transform.SetParent(transform, true);

            Transform fill = barInstance.transform.Find("Background/Fill");
            if (fill != null) fillImage = fill.GetComponent<Image>();

            UpdateBar();
        }
    }

    public void SetDurability(int v) {
    currentDurability = Mathf.Clamp(v, 0, maxDurability);
    UpdateBar();
}

    public void LoseDurability(int amount)
    {
        currentDurability -= Mathf.Max(0, amount);
        UpdateBar();

        if (currentDurability <= 0)
        {
            onBroken?.Invoke();

            var wsm = WeaponSlotManager.Instance;
            if (wsm != null) wsm.HandleWeaponBroken(gameObject);
            else Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            currentDurability = Mathf.Clamp(maxDurability, 0, maxDurability);
        UpdateBar();
    }

    public void RefillToMax()
{
    currentDurability = maxDurability;   // currentDurability senin değişkenin
    UpdateBar();                         // bar doldurma fonksiyonun
}



    private void UpdateBar()
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01((float)currentDurability / maxDurability);
    }
}
