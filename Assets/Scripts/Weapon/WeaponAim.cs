using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class WeaponAim : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Silahın ve altındaki her şeyin döneceği ana pivot. Genellikle silahın kendisidir.")]
    public Transform weaponPivot;

    [Tooltip("Bu silaha ait Light 2D objesi.")]
    public Light2D flashlight;

    [Tooltip("Merminin çıkacağı namlu ucu transformu.")]
    public Transform firePoint;

    private SpriteRenderer weaponSpriteRenderer;

    // FirePoint'in başlangıç lokal pozisyonu
    private Vector3 firePointDefaultLocalPos;

    private void Awake()
    {
        if (weaponPivot != null)
            weaponSpriteRenderer = weaponPivot.GetComponentInChildren<SpriteRenderer>();

        if (firePoint != null)
            firePointDefaultLocalPos = firePoint.localPosition; // örn: (0.05, 0.01, 0)
    }

    private void OnDisable()
    {
        if (flashlight != null) flashlight.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (flashlight != null) flashlight.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (GameStateManager.IsGamePaused ||GameStateManager.IsGameOver) return;
        if (Camera.main == null || weaponPivot == null) return;

        // Fare hedefi
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 aimDirection = (worldPosition - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Silahı döndür
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        // Sprite flip (senin mantığın)
        float facingDirection = PlayerMovement.FacingDirection;
        bool flip = (facingDirection < 0);

        if (weaponSpriteRenderer != null)
        {
            // Hem X hem Y flip
            weaponSpriteRenderer.flipX = flip;
            weaponSpriteRenderer.flipY = flip;
        }

        // Fener yönü
        if (flashlight != null)
            flashlight.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // FirePoint pozisyonu ve yönü
        if (firePoint != null)
        {
            // Hem X hem Y işaretini facing'e göre değiştir
            float sx = flip ? -1f : 1f;
            float sy = flip ? -1f : 1f;

            firePoint.localPosition = new Vector3(
                firePointDefaultLocalPos.x * sx,
                firePointDefaultLocalPos.y * sy,
                firePointDefaultLocalPos.z
            );

            // Rotasyonu pivot'tan miras alsın
            firePoint.localRotation = Quaternion.identity;
        }
    }
}
