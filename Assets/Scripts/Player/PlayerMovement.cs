using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.7f;

    private bool isSprinting = false;
    private bool isMoving = false;

    // --- References ---
    private Rigidbody2D rb;
    private PlayerStats stats;
    private Animator animator;
    private Camera mainCamera;
    private PlayerWeapon weapon;

    // --- Input ---
    private PlayerControls controls;
    private Vector2 moveInput;

    // --- Extra ---
    public static float FacingDirection { get; private set; } = 1f;
    public float soundRadius = 5f;

    void Awake()
    {
        weapon = GetComponent<PlayerWeapon>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        mainCamera = Camera.main;

        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Move.canceled += OnMoveCanceled;

        controls.Gameplay.Sprint.performed += ctx =>
        {
            if (weapon != null && weapon.IsBusy) return;
            isSprinting = true;
        };

        controls.Gameplay.Sprint.canceled += ctx =>
        {
            isSprinting = false;
        };

        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    // ---------------- INPUT ----------------

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    // ---------------- UPDATE (LOGIC + ANIMATION) ----------------

    void Update()
    {
        if (GameStateManager.IsGamePaused) return;

        UpdateMovementState();
        UpdateRotation();
        UpdateAnimation();
    }

    private void UpdateMovementState()
    {
        isMoving = moveInput.magnitude > 0.1f;

        // Weapon busy ise sprint iptal
        if (weapon != null && weapon.IsBusy)
            isSprinting = false;

        // Stats her frame güncellensin
        stats.SetMovementState(isMoving, isSprinting);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetBool("IsMoving", isMoving);
    }

    private void UpdateRotation()
    {
        if (mainCamera == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        Vector2 aimDirection = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        FacingDirection = aimDirection.x >= 0 ? 1f : -1f;
    }

    // ---------------- FIXED UPDATE (PHYSICS) ----------------

    void FixedUpdate()
    {
        if (!isMoving)
            return;

        float currentSpeed = moveSpeed;

        if (isSprinting && stats.HasStamina())
            currentSpeed *= sprintMultiplier;
        else
            isSprinting = false;

        Vector2 moveDelta = moveInput.normalized * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDelta);
    }

    // ---------------- SOUND HOOK (OPTIONAL) ----------------

    public void EmitFootstep()
    {
        SoundEmitter.EmitSound(transform.position, soundRadius);
    }
}
