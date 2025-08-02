// WeaponAim.cs (360 DERECE FARE TAKİBİ - BASİTLEŞTİRİLMİŞ HALİ)

using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Silahın ve FirePoint'in dayanacağı ana pivot noktası. Genellikle silahın kendisidir.")]
    public Transform weaponPivot; 
    
    // weaponVisual referansı artık doğrudan pivot'u kullanacağımız için gereksiz.
    // public Transform weaponVisual; 

    private SpriteRenderer weaponSpriteRenderer;

    private void Awake()
    {
        // Sprite Renderer'ı pivot'un altındaki çocuk objelerden bul.
        if (weaponPivot != null)
        {
            weaponSpriteRenderer = weaponPivot.GetComponentInChildren<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("WeaponAim: Weapon Pivot referansı atanmamış!", this.gameObject);
        }
    }

    // LateUpdate, karakterin hareketinden sonra çalışarak en doğru pozisyonu alır.
    private void LateUpdate()
    {
        if (Camera.main == null || weaponPivot == null) return;
        
        // 1. Fare pozisyonunu al ve nişan alma yönünü hesapla.
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        
        // Pivot noktasından fareye doğru olan yönü hesapla.
        Vector3 aimDirection = (worldPosition - weaponPivot.position).normalized;

        // 2. Yön vektöründen açıyı hesapla.
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        // 3. Silah pivotunu doğrudan bu açıya göre döndür.
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        // 4. Karakterin baktığı yöne göre görseli düzelt (flip).
        float facingDirection = PlayerMovement.FacingDirection;
        if (weaponSpriteRenderer != null)
        {
            // Eğer karakter sola bakıyorsa (parent'ın scale.x'i -1 ise),
            // bu ayna etkisini iptal etmek için sprite'ı DİKEYDE (flipY) çevir.
            weaponSpriteRenderer.flipX = (facingDirection < 0);
            weaponSpriteRenderer.flipY = (facingDirection < 0);
        }
    }
}