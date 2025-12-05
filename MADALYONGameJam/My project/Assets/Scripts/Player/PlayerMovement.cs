using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    public AudioSource footstepSource;
    public AudioClip lightFootstepLoop;   // 0–5 piyon
    public AudioClip heavyFootstepLoop;   // >5 piyon
    public int pawnCount = 0;
    private PlayerPiyon playerPiyon; 


    private Rigidbody2D rb;
    private Vector2 moveInput;
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
        footstepSource.loop = true;
        playerPiyon = FindObjectOfType<PlayerPiyon>();
    }
  void Update()
    {
       pawnCount = playerPiyon.GetCount();

    Debug.Log("Pawn Count = " + pawnCount);

    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        HandleFootstepSound();
    }

    void HandleFootstepSound()
    {
        Debug.Log("Pawn Count = " + pawnCount);

        bool isMoving = moveInput != Vector2.zero;

        if (isMoving)
        {
            // Piyon sayısına göre hangi loop çalacak?
            AudioClip targetClip = (pawnCount > 5) ? heavyFootstepLoop : lightFootstepLoop;

            // Eğer yanlış clip çalıyorsa, doğru olanla değiştir
            if (footstepSource.clip != targetClip)
            {
                footstepSource.clip = targetClip;
                footstepSource.Play();
            }

            // Eğer hiç çalmıyorsa başlat
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            // Hareket durunca sesi anında kes
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }
}
