using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.7f; // KoÅŸarken hÄ±z artÄ±ÅŸÄ±
    private bool isSprinting = false;

    // --- BileÅŸen ReferanslarÄ± ---
    private Rigidbody2D rb;
    private PlayerStats stats;
    private Animator animator;
    private Camera mainCamera;

    // --- Input System ---
    private PlayerControls controls;
    private Vector2 moveInput;

    // --- Ek Ã–zellikler ---
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
    float currentSpeed = moveSpeed;
    bool isMoving = moveInput.magnitude > 0.1f;

    // PlayerStats'a koÅŸu/yÃ¼rÃ¼me bilgisini HER FRAME gÃ¶nder
    stats.SetMovementState(isMoving, isSprinting);

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

    }



    // --- 360Â° Fareye DÃ¶nÃ¼k Animasyon ve Rotasyon ---
    private void UpdateRotationAndAnimation()
    {
        if (animator == null || mainCamera == null) return;

        // ğŸ§­ Fare konumunu al ve yÃ¶nÃ¼ hesapla
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        Vector2 aimDirection = (mouseWorldPos - transform.position).normalized;

        // ğŸŒ€ Karakteri fareye Ã§evir (360Â°)
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // Sprite yukarÄ± bakÄ±yorsa -90f uygundur

        // ğŸ Animasyon durumu
        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        // ğŸ” FacingDirection flip kontrolÃ¼ (silah veya atÄ±ÅŸ yÃ¶nÃ¼ iÃ§in)
        FacingDirection = aimDirection.x >= 0 ? 1f : -1f;
    }
}