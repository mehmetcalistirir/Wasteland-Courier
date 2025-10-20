using UnityEngine;
using UnityEngine.InputSystem;

public class MolotovThrower : MonoBehaviour
{
    [Header("Molotov Settings")]
    public GameObject molotovPrefab;
    public Transform throwPoint;
    public float maxThrowForce = 12f;
    public float minThrowForce = 4f;
    public float chargeSpeed = 5f;
    public float cooldown = 1.5f;

    private float currentForce;
    private bool isCharging;
    private float lastThrowTime;

    void Awake()
    {
        if (throwPoint == null)
        {
            Transform found = transform.Find("FirePoint");
            if (found != null)
                throwPoint = found;
            else
                Debug.LogWarning($"MolotovThrower: FirePoint bulunamadı! ({gameObject.name})");
        }
    }

    void Update()
    {
        // 🔹 1. Cooldown kontrolü
        if (Time.time < lastThrowTime + cooldown)
            return;

        // 🔹 2. Fare sol tuşuna basıldı — Şarj başlasın
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCharging = true;
            currentForce = minThrowForce;
            Debug.Log("🧪 Molotov şarj edilmeye başladı!");
        }

        // 🔹 3. Basılı tutma süresince güç artsın
        if (isCharging && Mouse.current.leftButton.isPressed)
        {
            currentForce += chargeSpeed * Time.deltaTime;
            currentForce = Mathf.Clamp(currentForce, minThrowForce, maxThrowForce);
        }

        // 🔹 4. Fare bırakıldığında fırlat
        if (isCharging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            TryThrowMolotov();
        }
    }

    private void TryThrowMolotov()
    {
        // 🔸 WeaponSlotManager üzerinden aktif slotu kontrol et
        var wm = WeaponSlotManager.Instance;
        if (wm == null)
        {
            Debug.LogError("❌ WeaponSlotManager bulunamadı!");
            return;
        }

        // Aktif slotta mermi var mı?
        int slotIndex = wm.activeSlotIndex;
        var bp = wm.GetBlueprintForSlot(slotIndex);
        if (bp == null || bp.weaponData == null)
        {
            Debug.LogWarning("⚠️ Molotov weapon blueprint bulunamadı!");
            return;
        }

        // Eğer molotovun mermisi bitmişse
        var (clip, reserve) = wm.GetAmmoStateForSlot(slotIndex);
        if (clip <= 0)
        {
            Debug.Log("❌ Molotov kalmadı!");
            wm.activeWeapon?.PlayEmptyClipSound();
            return;
        }

        // ✅ Molotov fırlat
        ThrowMolotov();

        // 🔻 Mermiyi azalt
        clip--;
        wm.SetAmmoStateForSlot(slotIndex, clip, reserve);
        wm.UpdateUI();
    }

    private void ThrowMolotov()
    {
        if (molotovPrefab == null)
        {
            Debug.LogError("❌ MolotovPrefab atanmadı!");
            return;
        }

        if (throwPoint == null)
        {
            Debug.LogError("❌ ThrowPoint atanmadı!");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouseWorldPos - throwPoint.position).normalized;

        GameObject molotov = Instantiate(molotovPrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rb = molotov.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(dir * currentForce, ForceMode2D.Impulse);

        Debug.Log($"🔥 Molotov fırlatıldı! Kuvvet: {currentForce:F2}");
        isCharging = false;
        lastThrowTime = Time.time;
    }
}
