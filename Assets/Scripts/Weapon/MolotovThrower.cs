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
        // 🔹 ThrowPoint’i sahnede bulmaya çalış
        TryAssignThrowPoint();
    }

    void OnEnable()
    {
        // 🔹 Her aktive olduğunda tekrar dene (inactive’tan çıkınca null olabilir)
        if (throwPoint == null)
            TryAssignThrowPoint();
            AutoWireThrowPoint();
    }

    private void TryAssignThrowPoint()
    {
        if (throwPoint != null) return;

        // 1️⃣ Kendi altındaki FirePoint
        Transform found = transform.Find("FirePoint");
        if (found != null)
        {
            throwPoint = found;
            return;
        }

        // 2️⃣ WeaponSlotManager’daki PlayerWeapon’dan al
        var wsm = WeaponSlotManager.Instance;
        if (wsm != null && wsm.playerWeapon != null && wsm.playerWeapon.firePoint != null)
        {
            throwPoint = wsm.playerWeapon.firePoint;
            Debug.Log($"MolotovThrower: ThrowPoint, PlayerWeapon'dan alındı -> {throwPoint.name}");
            return;
        }

        Debug.LogWarning($"MolotovThrower: ThrowPoint atanamadı! ({gameObject.name})");
    }

    void Update()
{
    // Molotov aktif değilse hiç çalışmasın
    var wm = WeaponSlotManager.Instance;
    if (wm == null) return;

    var bp = wm.GetBlueprintForSlot(wm.activeSlotIndex);
    if (bp == null || bp.weaponData == null || !bp.weaponData.isMolotov)
        return;  // ✨ aktif silah Molotov değil -> hiç işlem yapma

    // --- aşağısı senin mevcut kodun ---
    if (Time.time < lastThrowTime + cooldown)
        return;

    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        isCharging = true;
        currentForce = minThrowForce;
    }

    if (isCharging && Mouse.current.leftButton.isPressed)
    {
        currentForce += chargeSpeed * Time.deltaTime;
        currentForce = Mathf.Clamp(currentForce, minThrowForce, maxThrowForce);
    }

    if (isCharging && Mouse.current.leftButton.wasReleasedThisFrame)
    {
        TryThrowMolotov();
    }
}



private void AutoWireThrowPoint()
{
    if (throwPoint != null) return;

    // Önce aktif silahın FirePoint'ini dene
    var fp = WeaponSlotManager.Instance?.activeWeapon?.firePoint;
    if (fp != null) { throwPoint = fp; return; }

    // Olmazsa player içinde "FirePoint" ara
    var found = transform.Find("FirePoint");
    if (found != null) { throwPoint = found; }
}


   private void TryThrowMolotov()
{
    AutoWireThrowPoint();   // emin ol

    var wm = WeaponSlotManager.Instance;
    if (wm == null) return;

    int slotIndex = wm.activeSlotIndex;
    var bp = wm.GetBlueprintForSlot(slotIndex);
    if (bp == null || bp.weaponData == null || !bp.weaponData.isMolotov) return;

    var (clip, reserve) = wm.GetAmmoStateForSlot(slotIndex);
    if (clip <= 0)
    {
        wm.activeWeapon?.PlayEmptyClipSound();
        return;
    }

    ThrowMolotov();

    clip--;
    wm.SetAmmoStateForSlot(slotIndex, clip, reserve);
    wm.UpdateUI();
}

private void ThrowMolotov()
{
    AutoWireThrowPoint();   // emin ol
    if (molotovPrefab == null || throwPoint == null) return; // ❌ LogError yerine sessiz çık

    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    Vector2 dir = (mouseWorldPos - throwPoint.position).normalized;

    var molotov = Instantiate(molotovPrefab, throwPoint.position, Quaternion.identity);
    var rb = molotov.GetComponent<Rigidbody2D>();
    if (rb != null) rb.AddForce(dir * currentForce, ForceMode2D.Impulse);

    isCharging = false;
    lastThrowTime = Time.time;
}

    
}
