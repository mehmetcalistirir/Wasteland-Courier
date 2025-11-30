using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    public enum Mode { DistanceBased, AnimationEvents }
    [Header("Mode")]
    public Mode mode = Mode.DistanceBased;

    [Header("Footstep Clips")]
    public AudioClip[] footstepClips;

    [Header("Step Settings")]
    public float walkStepDistance = 0.85f;  // yürürken adım mesafesi
    public float runStepDistance  = 0.55f;  // koşarken daha kısa mesafe
    public float runThreshold     = 3.0f;   // koşma hızı eşiği
    public float minMovementSpeed = 0.05f;  // durma eşiği

    [Header("Sound Settings")]
    public float volume = 0.9f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;
    public float minStepGap = 0.12f; // iki adım arası min süre

    private AudioSource src;
    private Vector3 lastPosition;
    private float accumulatedDistance = 0f;
    private float lastStepTime = 0f;
    private int lastClipIndex = -1;
    private float nextStepAvailableTime = 0f;


    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D sound

        lastPosition = transform.position;
    }

    void Update()
    {
        if (mode == Mode.AnimationEvents)
        {
            lastPosition = transform.position;
            return;
        }

        if (footstepClips == null || footstepClips.Length == 0) return;
        if (Time.timeScale == 0f) return; // oyun durduysa ses yok

        Vector3 currentPos = transform.position;
        float frameDistance = Vector3.Distance(currentPos, lastPosition);
        lastPosition = currentPos;

        float speed = frameDistance / Mathf.Max(Time.deltaTime, 0.0001f);

        if (speed < minMovementSpeed)
        {
            accumulatedDistance = 0f;
            return;
        }

        accumulatedDistance += frameDistance;

        float targetStepDist = speed >= runThreshold ? runStepDistance : walkStepDistance;

        if (accumulatedDistance >= targetStepDist)
        {
            PlayStep();
            accumulatedDistance -= targetStepDist; // stable rhythm
        }
    }

    // Animation event için
    public void AnimationFootstep()
    {
        if (mode == Mode.AnimationEvents)
            PlayStep();
    }

    private void PlayStep()
{
    // 1) Ses süresi + gap bitmeden adım çalma
    if (Time.time < nextStepAvailableTime) return;

    // 2) minStepGap kontrolü (çift tetik engeli)
    if (Time.time - lastStepTime < minStepGap) return;
    lastStepTime = Time.time;

    // 3) Ses seç
    int idx = PickClipIndex();
    float clipLength = footstepClips[idx].length;

    // 4) Ses çal
    src.pitch = Random.Range(minPitch, maxPitch);
    src.PlayOneShot(footstepClips[idx], volume);

    // 5) Ses boyunca yeni adımı engelle
    nextStepAvailableTime = Time.time + clipLength;
}

    private int PickClipIndex()
    {
        if (footstepClips.Length == 1) return 0;

        int i;
        do { i = Random.Range(0, footstepClips.Length); }
        while (i == lastClipIndex);
        lastClipIndex = i;

        return i;
    }
}
