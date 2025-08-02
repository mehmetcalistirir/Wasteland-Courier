using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate = 0.2f;

    private float fireTimer = 0f;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && fireTimer <= 0f)
        {
            FireTowardMouse();
            fireTimer = fireRate;
        }
    }

    void FireTowardMouse()
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        Vector2 direction = (worldPos - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Launch(direction, bulletSpeed);

        Debug.Log("🚀 Mermi fırlatıldı!");
    }

}
