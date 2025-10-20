// YSorter.cs (GERÇEKTEN DOĞRU VE BASİT HALİ)

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private static readonly float SortingOrderMultiplier = 10f; // POZİTİF YAPTIK

    [Tooltip("Sıralama için objenin pivot noktasına eklenecek dikey ofset. Ayakların olduğu yere göre ayarlayın.")]
    public float yOffset = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Y pozisyonu AZALDIKÇA (aşağı indikçe) Order in Layer ARTAR (öne gelir).
        // Bu yüzden Y'yi NEGATİF ile çarpmalıyız.
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -SortingOrderMultiplier);
    }
}