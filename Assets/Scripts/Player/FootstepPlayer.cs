using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Footstep Clip (Loop)")]
    public AudioClip footstepLoop;

    [Header("Movement")]
    public float minMovementSpeed = 0.005f;

    [Header("Sound")]
    public float volume = 0.9f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private AudioSource src;
    private Vector3 lastPosition;
    private bool isMoving = false;

    void Awake()
    {
        src = GetComponent<AudioSource>();

        src.playOnAwake = false;
        src.loop = true;          // 🔴 EN ÖNEMLİ
        src.spatialBlend = 0f;    // 2D
        src.volume = volume;

        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 currentPos = transform.position;
        float distance = (currentPos - lastPosition).magnitude;
        float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = currentPos;

        bool movingNow = speed >= minMovementSpeed;

        // Hareket BAŞLADI
        if (movingNow && !isMoving)
        {
            StartFootsteps();
        }
        // Hareket BİTTİ
        else if (!movingNow && isMoving)
        {
            StopFootsteps();
        }

        isMoving = movingNow;
    }

    void StartFootsteps()
    {
        if (footstepLoop == null) return;

        src.clip = footstepLoop;
        src.pitch = Random.Range(minPitch, maxPitch);
        src.volume = volume;

        src.Play();
    }

    void StopFootsteps()
    {
        src.Stop();
    }
}
