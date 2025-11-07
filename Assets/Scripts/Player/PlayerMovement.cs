using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.7f;
    private bool isSprinting = false;

    [Header("UI")]
    public Slider staminaBar;

    private Rigidbody2D rb;
    private PlayerStats stats;
    private Animator animator;
    private Camera mainCamera;

    private PlayerControls controls;
    private Vector2 moveInput;

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

    private void UpdateStamina()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isSprinting && isMoving)
            stats.ModifyStamina(-stats.staminaDrainRate * Time.deltaTime);
        else if (isMoving)
            stats.ModifyStamina(stats.staminaRegenWalk * Time.deltaTime);
        else
            stats.ModifyStamina(stats.staminaRegenIdle * Time.deltaTime);
    }

    private void UpdateRotationAndAnimation()
    {
        if (animator == null || mainCamera == null) return;

        // 🧭 1. Fare yönünü bul
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 aimDir = (mouseWorldPosition - transform.position).normalized;

        // 🌀 2. Oyuncunun yönünü fareye çevir (360° rotasyon)
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // Sprite yukarı bakıyorsa -90° uygundur

        // 🎞️ 3. Animasyon kontrolü
        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }
}
