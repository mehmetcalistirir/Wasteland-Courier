using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Footstep Loop Clip")]
    public AudioClip footstepLoop;

    [Header("Sound")]
    public float volume = 0.9f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private AudioSource src;
    private Animator animator;
    private bool wasMovingLastFrame = false;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
        src.volume = volume;
    }

    void Update()
    {
        if (animator == null) return;

        // 🔑 GERÇEK HAREKET KAYNAĞI
        bool isMoving = animator.GetBool("IsMoving");

        // Hareket BAŞLADI
        if (isMoving && !wasMovingLastFrame)
        {
            StartFootsteps();
        }
        // Hareket BİTTİ
        else if (!isMoving && wasMovingLastFrame)
        {
            StopFootsteps();
        }

        wasMovingLastFrame = isMoving;
    }

    void StartFootsteps()
    {
        if (footstepLoop == null) return;
        if (src.isPlaying) return;

        src.clip = footstepLoop;
        src.pitch = Random.Range(minPitch, maxPitch);
        src.volume = volume;
        src.Play();
    }

    void StopFootsteps()
    {
        if (src.isPlaying)
            src.Stop();
    }
}
