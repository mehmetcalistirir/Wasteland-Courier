using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MolotovThrower : MonoBehaviour
{
    [Header("Molotov Settings")]
    public GameObject molotovPrefab;
    public Transform throwPoint;
    public float maxThrowForce = 12f;
    public float minThrowForce = 4f;
    public float chargeSpeed = 4f;
    public float cooldown = 1.5f;

    private float currentForce;
    private bool isCharging;
    private float chargeStartTime;
    private float lastThrowTime;

    private bool justEnabled;


    void Awake()
    {
        AutoWireThrowPoint();
    }

    void Update()
    {

        if (justEnabled)
    {
        // 🔒 Bu frame'de hiçbir input işleme
        justEnabled = false;
        return;
    }

     
        
        // --- Fırlatma kilidi: cooldown bitmeden atış olmasın
        if (Time.time < lastThrowTime + cooldown)
            return;

        // --- Basılı tutma süresine göre şarj et
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            currentForce = minThrowForce;
            Debug.Log("🔥 Molotov şarj ediliyor...");
        }

        if (isCharging && Mouse.current.leftButton.isPressed)
        {
            float elapsed = Time.time - chargeStartTime;
            float t = Mathf.Clamp01(elapsed / 3f); // 3 saniyede tam şarj
            currentForce = Mathf.Lerp(minThrowForce, maxThrowForce, t);
        }

        // --- Mouse bırakıldığında fırlat
        if (isCharging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ThrowMolotov();
            isCharging = false;
        }
    }

void OnEnable()
{
    justEnabled = true; // aktif edildiği frame
}




    private void AutoWireThrowPoint()
    {
        if (throwPoint != null) return;

        // Önce player’ın FirePoint’ini bul
        var playerWeapon = GetComponentInParent<PlayerWeapon>();
        if (playerWeapon != null && playerWeapon.firePoint != null)
        {
            throwPoint = playerWeapon.firePoint;
            Debug.Log($"MolotovThrower → FirePoint otomatik atandı: {throwPoint.name}");
            return;
        }

        // Sahne içinde “FirePoint” isminde bir child varsa onu kullan
        var found = transform.Find("FirePoint");
        if (found != null)
        {
            throwPoint = found;
            Debug.Log($"MolotovThrower → FirePoint bulundu: {throwPoint.name}");
        }
    }

    private void ThrowMolotov()
{
    if (molotovPrefab == null || throwPoint == null)
    {
        Debug.LogWarning("⚠️ MolotovPrefab veya ThrowPoint atanmadı!");
        return;
    }

    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    Vector2 direction = (mouseWorldPos - throwPoint.position).normalized;

    Vector3 spawnPos = throwPoint.position + (Vector3)direction * 0.5f;
    GameObject molotov = Instantiate(molotovPrefab, spawnPos, Quaternion.identity);
    Rigidbody2D rb = molotov.GetComponent<Rigidbody2D>();

    if (rb != null)
    {
        rb.gravityScale = 1f; // 🔥 fırlatıldığında yerçekimini tekrar aç
        rb.AddForce(direction * currentForce, ForceMode2D.Impulse);
    }

    Debug.Log($"💣 Molotov fırlatıldı! Güç: {currentForce:F2}");

    lastThrowTime = Time.time;
}


}
