using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.7f; // Koşarken hız artışı
    private bool isSprinting = false;
    
    [Header("UI")]
    public Slider staminaBar; 

    // --- Bileşen Referansları ---
    private Rigidbody2D rb;
    private PlayerStats stats;
    private Animator animator;
    private Camera mainCamera;

    // --- Input System ---
    private PlayerControls controls;
    private Vector2 moveInput;

    // FacingDirection artık silah sistemi için ve karakteri çevirmek için kullanılacak.
    public static float FacingDirection { get; private set; } = 1f;

    public float soundRadius = 5f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controls = new PlayerControls();
        stats = GetComponent<PlayerStats>();
        mainCamera = Camera.main; // Kamerayı bir kere al, daha performanslı.
    }

    private void OnEnable()
    {
        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Move.canceled += OnMoveCanceled;
        controls.Gameplay.Sprint.performed += ctx => isSprinting = true;
        controls.Gameplay.Sprint.canceled += ctx => isSprinting = false;
        controls.Gameplay.Enable();
    }

    public void EmitFootstep()
    {
        SoundEmitter.EmitSound(transform.position, soundRadius);
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        UpdateStamina();

        float currentSpeed = moveSpeed;
        if (isSprinting && stats.HasStamina() && moveInput.magnitude > 0.1f)
        {
            currentSpeed *= sprintMultiplier;
        }
        else
        {
            isSprinting = false; // stamina bitince koşma kapat
        }

        Vector2 moveDelta = moveInput * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDelta);

        animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }


    void Update()
    {
        if (GameStateManager.IsGamePaused) return;
        UpdateAnimationAndDirection();
        
        if (staminaBar != null && stats != null)
        {
            staminaBar.maxValue = stats.GetMaxStamina();
            staminaBar.value = stats.GetStamina();
        }
    }

    private void UpdateStamina()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isSprinting && isMoving)
        {
            stats.ModifyStamina(-stats.staminaDrainRate * Time.deltaTime);
        }
        else if (isMoving)
        {
            stats.ModifyStamina(stats.staminaRegenWalk * Time.deltaTime);
        }
        else
        {
            stats.ModifyStamina(stats.staminaRegenIdle * Time.deltaTime);
        }
    }

    private void UpdateAnimationAndDirection()
    {
        if (animator == null || mainCamera == null) return;

        // 1. Fare pozisyonunu ve nişan alma yönünü hesapla.
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 aimDirection = (mouseWorldPosition - transform.position).normalized;

        // --- YENİ MANTIK: HASSAS YÖNÜ, KESKİN 8 YÖNE YUVARLAMA ---

        // 2. Fareden gelen hassas yönü, 8 ana yönden birine çevir.
        Vector2 discretizedAimDirection = DiscretizeDirection(aimDirection);

        // ---------------------------------------------

        // 3. Animator'e bu yeni, "keskin" yönü gönder.
        animator.SetFloat("moveX", discretizedAimDirection.x);
        animator.SetFloat("moveY", discretizedAimDirection.y);

        // 4. Genel hareket durumunu bildir.
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        // 5. Sprite'ı çevirme (flip) mantığı.
        // Artık 'aimDirection' değil, yuvarlanmış 'discretizedAimDirection' kullanıyoruz.
        if (Mathf.Abs(discretizedAimDirection.x) > 0.01f)
        {
            FacingDirection = Mathf.Sign(discretizedAimDirection.x);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * FacingDirection, transform.localScale.y, transform.localScale.z);
        }
    }

    // YENİDEN EKLENEN YARDIMCI FONKSİYON: Gelen bir vektörü 8 ana yönden birine yuvarlar.
    private Vector2 DiscretizeDirection(Vector2 vector)
    {
        // Açıya göre 8 yönden birini seçme daha güvenilir çalışır.
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        // Açıyı 0-360 derece aralığına getir.
        if (angle < 0) angle += 360;

        // Her bir 45 derecelik dilim, bir ana yöne karşılık gelir.
        int slice = Mathf.RoundToInt(angle / 45f);

        switch (slice)
        {
            case 0: return Vector2.right;       // Sağ
            case 1: return new Vector2(1, 1).normalized;  // Sağ-Yukarı
            case 2: return Vector2.up;          // Yukarı
            case 3: return new Vector2(-1, 1).normalized; // Sol-Yukarı
            case 4: return Vector2.left;        // Sol
            case 5: return new Vector2(-1, -1).normalized;// Sol-Aşağı
            case 6: return Vector2.down;        // Aşağı
            case 7: return new Vector2(1, -1).normalized; // Sağ-Aşağı
            case 8: return Vector2.right;       // 360 derece = 0 derece
            default: return Vector2.right;
        }
    }
}