// PlayerFootsteps.cs — ÇAKIŞMASIZ, TEK MOD, DEBOUNCE'LU
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    public enum Mode { DistanceBased, AnimationEvents }
    [Header("Mode")]
    public Mode mode = Mode.DistanceBased;  // Sadece birini kullan

    [Header("Clips")]
    public AudioClip[] footstepClips;

    [Header("Distance-Based Settings")]
    [Tooltip("Yürürken adım atmak için gereken mesafe (metre).")]
    public float stepDistanceWalk = 0.9f;
    [Tooltip("Koşarken adım mesafesi (metre). Daha kısa -> daha sık adım.")]
    public float stepDistanceRun  = 0.6f;
    [Tooltip("Koşu sayılan hız eşiği (m/sn).")]
    public float runSpeedThreshold = 3.0f;
    [Tooltip("Hareket algılama eşiği (m/sn) — bunun altı sayılmaz.")]
    public float minMoveSpeed = 0.05f;

    [Header("Sound")]
    public float volume   = 0.9f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;
    [Tooltip("İki adım arasında minimum süre (sn). Çift tetiklemeleri engeller.")]
    public float minStepGap = 0.12f;

    private AudioSource src;
    private Vector2 lastPos;
    private float distanceAccum;
    private float lastStepTime;
    private int lastClipIndex = -1; // Aynı klibi üst üste çalmamak için

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D ses
        lastPos = transform.position;

        // Güvenlik: sahnede aynı objede birden fazla AudioSource varsa, bu bileşenin src'si dışındakileri kapatmak isteyebilirsin.
        // (İstemiyorsan sil)
        var allSources = GetComponents<AudioSource>();
        for (int i = 0; i < allSources.Length; i++)
            if (allSources[i] != src) allSources[i].enabled = false;
    }

    void Update()
    {
        if (GameStateManager.IsGamePaused) return;
        if (footstepClips == null || footstepClips.Length == 0) return;

        if (mode == Mode.AnimationEvents)
        {
            // Bu modda Update adım çalmaz; sadece AnimationFootstep() tetikler.
            lastPos = transform.position; // pozisyonu güncel tut
            return;
        }

        // --- Distance-based mod ---
        Vector2 now = transform.position;
        float frameDist = (now - lastPos).magnitude;
        lastPos = now;

        // hızı pozisyon farkından hesapla (MovePosition kullandığın için güvenilir)
        float speed = frameDist / Mathf.Max(Time.deltaTime, 0.0001f);
        if (speed <= minMoveSpeed)
        {
            distanceAccum = 0f; // durduysa biriktirme
            return;
        }

        distanceAccum += frameDist;

        float targetStepDist = (speed >= runSpeedThreshold) ? stepDistanceRun : stepDistanceWalk;

        if (distanceAccum >= targetStepDist)
        {
            TryPlayStep();
            // Fazlayı koru ki ritim stabil kalsın
            distanceAccum -= targetStepDist;
        }
    }

    // Animasyonun topuk temas frame’ine event koyarsan bunu çağır:
    public void AnimationFootstep()
    {
        if (mode != Mode.AnimationEvents) return; // yanlış modda ise yok say
        TryPlayStep();
    }

    private void TryPlayStep()
    {
        // Debounce: aynı anda/aynı frame'de iki tetik gelse bile tek ses
        if (Time.time - lastStepTime < minStepGap) return;
        lastStepTime = Time.time;

        int idx = RandomClipIndexNoImmediateRepeat();
        src.pitch = Random.Range(minPitch, maxPitch);
        src.PlayOneShot(footstepClips[idx], volume);

        // Dünya sesi yaymak istersen:
        // SoundEmitter.EmitSound(transform.position, 5f);
    }

    private int RandomClipIndexNoImmediateRepeat()
    {
        if (footstepClips.Length == 1) return 0;
        int idx;
        do { idx = Random.Range(0, footstepClips.Length); }
        while (idx == lastClipIndex);
        lastClipIndex = idx;
        return idx;
    }
}
