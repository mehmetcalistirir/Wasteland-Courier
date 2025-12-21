using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Footstep Loop Clips")]
    public AudioClip walkLoop;
    public AudioClip runLoop;

    [Header("Sound")]
    public float walkVolume = 0.7f;
    public float runVolume = 0.95f;

    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private AudioSource src;
    private Animator animator;

    private bool wasMovingLastFrame = false;
    private bool wasRunningLastFrame = false;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
    }

    void Update()
    {
        if (animator == null) return;

        bool isMoving = animator.GetBool("IsMoving");
        bool isRunning = animator.GetBool("IsRunning");

        // ⛔ HAREKET YOK → SES YOK
        if (!isMoving)
        {
            if (wasMovingLastFrame)
                StopFootsteps();

            wasMovingLastFrame = false;
            wasRunningLastFrame = false;
            return;
        }

        // ▶️ HAREKET BAŞLADI
        if (!wasMovingLastFrame)
        {
            PlayFootsteps(isRunning);
        }
        // 🔁 WALK ↔ RUN GEÇİŞİ
        else if (wasRunningLastFrame != isRunning)
        {
            PlayFootsteps(isRunning);
        }

        wasMovingLastFrame = true;
        wasRunningLastFrame = isRunning;
    }

    void PlayFootsteps(bool running)
    {
        AudioClip targetClip = running ? runLoop : walkLoop;
        if (targetClip == null) return;

        src.clip = targetClip;
        src.pitch = Random.Range(minPitch, maxPitch);
        src.volume = running ? runVolume : walkVolume;

        if (!src.isPlaying)
            src.Play();
    }

    void StopFootsteps()
    {
        if (src.isPlaying)
            src.Stop();
    }
}
