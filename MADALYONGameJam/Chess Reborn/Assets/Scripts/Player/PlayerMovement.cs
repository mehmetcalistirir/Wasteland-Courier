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
    public float straightStepSize = 1f;    // düz hareket mesafesi
    public float diagonalStepSize = 1.4f;  // çapraz hareket mesafesi

    private float stepCooldown;
    private bool canMove = true;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    public AudioSource footstepSource;
    public AudioClip lightFootstepLoop;
    public AudioClip heavyFootstepLoop;

    public int pawnCount;
    private PlayerPiyon playerPiyon;
    private PlayerControls controls;

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
        rb = GetComponent<Rigidbody2D>();
        footstepSource.loop = false;
        playerPiyon = FindObjectOfType<PlayerPiyon>();
    }

    void Update()
    {
        pawnCount = playerPiyon.GetCount();

        stepCooldown = baseStepCooldown + (pawnCount * cooldownPerPawn);

        if (canMove && moveInput != Vector2.zero)
        {
            Vector2 dir = NormalizeDirection(moveInput);
            StartCoroutine(MoveOneStep(dir));
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

        // 🔥 yön düz mü çapraz mı?
        float step = (direction.x != 0 && direction.y != 0)
            ? diagonalStepSize      // çapraz
            : straightStepSize;     // düz

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
