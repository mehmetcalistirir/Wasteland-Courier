using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;     // bir kareye giderken hız

    [Header("Dynamic Step Settings")]
    public float baseStepCooldown = 0.2f;
    public float cooldownPerPawn = 0.01f;
    public float stepSize = 1f;       // satranç karesi 1 birim

    private float stepCooldown;       // 🔹 EKSİK OLAN ALAN BUYDU
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
        footstepSource.loop = false; // adım sesini tek sefer çalacağız
        playerPiyon = FindObjectOfType<PlayerPiyon>();
    }

    void Update()
    {
        // 🔹 Önce piyon sayısını güncelle
        pawnCount = playerPiyon.GetCount();

        // 🔹 Sonra dinamik cooldown hesapla
        stepCooldown = baseStepCooldown + (pawnCount * cooldownPerPawn);
        // İstersen minimum / maksimum sınır koyabilirsin:
        // stepCooldown = Mathf.Clamp(stepCooldown, 0.1f, 1.0f);

        Debug.Log("pawnCount = " + pawnCount + " | stepCooldown = " + stepCooldown);

        // input varsa ve hareket edebiliyorsak
        if (canMove && moveInput != Vector2.zero)
        {
            Vector2 dir = NormalizeDirection(moveInput);
            StartCoroutine(MoveOneStep(dir));
        }
    }

    Vector2 NormalizeDirection(Vector2 input)
    {
        // Hareket yönünü 8 yönlü (grid) hale getiriyoruz
        float x = Mathf.Sign(input.x);
        float y = Mathf.Sign(input.y);

        // eksenlerin mutlak değerleri küçükse sıfırla
        if (Mathf.Abs(input.x) < 0.5f) x = 0;
        if (Mathf.Abs(input.y) < 0.5f) y = 0;

        return new Vector2(x, y).normalized;
    }

    IEnumerator MoveOneStep(Vector2 direction)
    {
        canMove = false;

        // hedef nokta
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction * stepSize;

        float t = 0f;
        float duration = stepSize / moveSpeed;

        PlayFootstep();  // adım sesini çal

        // kareye doğru smooth hareket
        while (t < duration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t / duration));
            t += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);

        // Adım bittikten sonra verilen cooldown kadar bekle
        yield return new WaitForSeconds(stepCooldown);

        canMove = true;
    }

    void PlayFootstep()
    {
        AudioClip clip = (pawnCount > 5) ? heavyFootstepLoop : lightFootstepLoop;
        footstepSource.PlayOneShot(clip); // loop değil tek seferlik adım sesi
    }
}
