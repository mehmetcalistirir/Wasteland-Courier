using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Dynamic Step Settings")]
    public float baseStepCooldown = 0.2f;
    public float cooldownPerPawn = 0.01f;

    [Header("Step Sizes")]
    public float straightStepSize = 1f;
    public float diagonalStepSize = 1.4f;

    private float buffAmount = 0f;
    private float zoneBuff = 0f;
    private float normalSpeed;
    public float zoneBonus = 2f;   // Kendi köyünde hızlanma
    public float zoneSlow = -1.5f;   // Düşman köyünde yavaşlama
    private BaseController zone;   // içinde bulunduğu bölge referansı


    private float stepCooldown;
    private bool canMove = true;

    private Vector2 moveInput;      // klavye input (WASD)
    private Vector2 mobileInput;    // mobil UI input

    private Rigidbody2D rb;
    private BaseController insideZone = null;
private bool slowApplied = false;
private float originalSpeed;


    public AudioSource footstepSource;
    public AudioClip lightFootstepLoop;
    public AudioClip heavyFootstepLoop;

    public int pawnCount;
    private PlayerPiyon playerPiyon;
    private PlayerControls controls;
    public float baseSpeed = 5f;        // Normal hız
    public float speedBoostAmount = 0f; // Geçici buff (+3 gibi)

private bool zoneApplied = false;
private float zoneSpeedModifier = 0f; // eklenen/çıkarılan değer



    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        normalSpeed = moveSpeed;

        rb = GetComponent<Rigidbody2D>();
        footstepSource.loop = false;
        playerPiyon = FindObjectOfType<PlayerPiyon>();
        baseSpeed = moveSpeed;
    }

    // 🔥 UI yön butonları burayı çağıracak
    public void SetMobileInput(Vector2 val)
    {
        mobileInput = val;
    }

    void Update()
    {
        pawnCount = playerPiyon.GetCount();
        stepCooldown = baseStepCooldown + (pawnCount * cooldownPerPawn);

    float zoneEffect = 0f;

    if (zone != null)
    {
        if (zone.owner == Team.Player)
            zoneEffect = zoneBonus;     // Kendi bölgesi → hızlan
        else if (zone.owner == Team.Enemy)
            zoneEffect = zoneSlow;      // Düşman bölgesi → yavaşla
    }

    // FINAL HIZ FORMÜLÜ
    float finalSpeed = baseSpeed + speedBoostAmount + zoneEffect;

    // Hızı uygula
    moveSpeed = finalSpeed;

    // Hareket sistemi
    if (canMove && moveInput != Vector2.zero)
    {
        Vector2 dir = NormalizeDirection(moveInput);
        StartCoroutine(MoveOneStep(dir));
    }
}



    public void AddSpeedBuff(float amount)
    {
        if (zoneBuff > 0f)
            return;

        zoneBuff = amount;
        moveSpeed += amount;

        Debug.Log("BUFF APPLIED: +" + amount);
    }

    public void RemoveSpeedBuff(float amount)
    {
        if (zoneBuff <= 0f)
            return;

        moveSpeed -= zoneBuff;
        zoneBuff = 0f;

        Debug.Log("BUFF REMOVED");
    }
    public void SetInsideZone(BaseController zone)
{
    // Bölge bilgisi
    insideZone = zone;

    // Eğer bölgeye yeni giriyorsa
    if (zone != null && !zoneApplied)
    {
        zoneApplied = true;

        // Ne kadar hız değişecek?
        zoneSpeedModifier = -1.5f;  // örnek: 1.5 yavaşlat
        // zoneSpeedModifier = +2f; // hız arttırmak istersen

        moveSpeed += zoneSpeedModifier;
    }

    // Bölgeden çıkıyorsa
    else if (zone == null && zoneApplied)
    {
        moveSpeed -= zoneSpeedModifier; // eklenen değer geri alınır
        zoneApplied = false;
        zoneSpeedModifier = 0f;
    }
}



    Vector2 NormalizeDirection(Vector2 input)
    {
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        if (Mathf.Abs(input.x) < 0.5f) x = 0;
        if (Mathf.Abs(input.y) < 0.5f) y = 0;

        return new Vector2(x, y).normalized;
    }

    IEnumerator MoveOneStep(Vector2 direction)
    {
        canMove = false;

        float step = (direction.x != 0 && direction.y != 0)
            ? diagonalStepSize
            : straightStepSize;

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction * step;

        float t = 0f;
        float duration = step / moveSpeed;

        PlayFootstep();

        while (t < duration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t / duration));
            t += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);

        yield return new WaitForSeconds(stepCooldown);

        canMove = true;
    }

    void PlayFootstep()
    {
        AudioClip clip = (pawnCount > 5) ? heavyFootstepLoop : lightFootstepLoop;
        footstepSource.PlayOneShot(clip);
    }

    public IEnumerator TemporarySpeedBoost(float amount, float duration)
    {
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed -= amount;
    }
}
