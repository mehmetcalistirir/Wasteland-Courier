using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class WeaponAim : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Silahın döneceği ana pivot (weapon prefab root).")]
    public Transform weaponPivot;

    [Tooltip("PlayerWeapon scripti (firePoint buradan alınır).")]
    public PlayerWeapon playerWeapon;

    [Tooltip("Silaha ait fener ışığı.")]
    public Light2D flashlight;

    private SpriteRenderer weaponSpriteRenderer;

    private Vector3 firePointDefaultLocalPos;

    private void Awake()
    {
        if (weaponPivot != null)
            weaponSpriteRenderer = weaponPivot.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (flashlight != null)
            flashlight.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (flashlight != null)
            flashlight.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (GameStateManager.IsGamePaused || GameStateManager.IsGameOver)
            return;

        if (Camera.main == null || weaponPivot == null || playerWeapon == null)
            return;

        Transform firePoint = playerWeapon.firePoint;
        if (firePoint == null)
            return;

        // İlk frame'de firePoint referansı geldiyse default pos al
        if (firePointDefaultLocalPos == Vector3.zero)
            firePointDefaultLocalPos = firePoint.localPosition;

        // 🖱 Mouse world position
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        Vector3 dir = (worldPos - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 🔄 Pivot rotation
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        // 🔁 Sprite flip (PlayerMovement yönüne göre)
        float facing = PlayerMovement.FacingDirection;
        bool flip = facing < 0f;

        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipX = flip;
            weaponSpriteRenderer.flipY = flip;
        }

        // 🔦 Fener yönü
        if (flashlight != null)
            flashlight.transform.rotation =
                Quaternion.Euler(0f, 0f, angle - 90f);

        // 🔫 FirePoint local offset (flip'e göre)
        float sx = flip ? -1f : 1f;
        float sy = flip ? -1f : 1f;

        firePoint.localPosition = new Vector3(
            firePointDefaultLocalPos.x * sx,
            firePointDefaultLocalPos.y * sy,
            firePointDefaultLocalPos.z
        );

        firePoint.localRotation = Quaternion.identity;
    }
}
