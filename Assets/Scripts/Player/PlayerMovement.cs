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

    // --- Ek Özellikler ---
    public static float FacingDirection { get; private set; } = 1f;
    public float soundRadius = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controls = new PlayerControls();
        stats = GetComponent<PlayerStats>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Move.canceled += OnMoveCanceled;
        controls.Gameplay.Sprint.performed += ctx => isSprinting = true;
        controls.Gameplay.Sprint.canceled += ctx => isSprinting = false;
        controls.Gameplay.Enable();
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

    public void EmitFootstep()
    {
        SoundEmitter.EmitSound(transform.position, soundRadius);
    }

    private void FixedUpdate()
    {
        UpdateStamina();

        float currentSpeed = moveSpeed;
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isSprinting && stats.HasStamina() && isMoving)
            currentSpeed *= sprintMultiplier;
        else
            isSprinting = false;

        Vector2 moveDelta = moveInput * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDelta);
    }

    void Update()
    {
        if (GameStateManager.IsGamePaused) return;

        UpdateRotationAndAnimation();

        if (staminaBar != null && stats != null)
        {
            staminaBar.maxValue = stats.GetMaxStamina();
            staminaBar.value = stats.GetStamina();
        }
    }

    // --- Stamina Sistemi (Açlığa bağlı yenilenme dahil) ---
    private void UpdateStamina()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        // Açlığa göre stamina yenilenme hızı
        float hungerRatio = (float)stats.currentHunger / stats.maxHunger;
        float hungerMultiplier = 1f;

        if (hungerRatio >= 0.8f)
            hungerMultiplier = 1.0f;
        else if (hungerRatio >= 0.4f)
            hungerMultiplier = 0.7f;
        else
            hungerMultiplier = 0.4f;

        if (isSprinting && isMoving)
            stats.ModifyStamina(-stats.staminaDrainRate * Time.deltaTime);
        else if (isMoving)
            stats.ModifyStamina(stats.staminaRegenWalk * hungerMultiplier * Time.deltaTime);
        else
            stats.ModifyStamina(stats.staminaRegenIdle * hungerMultiplier * Time.deltaTime);
    }

    // --- 360° Fareye Dönük Animasyon ve Rotasyon ---
    private void UpdateRotationAndAnimation()
    {
        if (animator == null || mainCamera == null) return;

        // 🧭 Fare konumunu al ve yönü hesapla
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        Vector2 aimDirection = (mouseWorldPos - transform.position).normalized;

        // 🌀 Karakteri fareye çevir (360°)
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // Sprite yukarı bakıyorsa -90f uygundur

        // 🎞 Animasyon durumu
        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        // 🔁 FacingDirection flip kontrolü (silah veya atış yönü için)
        FacingDirection = aimDirection.x >= 0 ? 1f : -1f;
    }
}