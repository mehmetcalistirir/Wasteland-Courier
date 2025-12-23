using System.Collections.Generic;
using UnityEngine;

public class RadioSystem : MonoBehaviour
{
    public static RadioSystem Instance;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Signal")]
    [Range(0f, 1f)]
    public float currentSignalStrength = 1f;

    public AudioLowPassFilter lowPassFilter;

    [Header("Runtime State")]
    public List<RadioStation> activeStations = new(); // unlocked
    public RadioStation currentStation;

    private int currentClipIndex = 0;

    private void Awake()
{
    if (Instance != null)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;

    // Low pass otomatik ekle
    lowPassFilter = gameObject.GetComponent<AudioLowPassFilter>();
    if (lowPassFilter == null)
        lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
}

public void SetSignalStrength(float strength)
{
    if (audioSource == null)
        return;

    currentSignalStrength = Mathf.Clamp01(strength);

    audioSource.volume = Mathf.Lerp(0.2f, 1f, currentSignalStrength);

    if (lowPassFilter != null)
    {
        lowPassFilter.cutoffFrequency =
            Mathf.Lerp(800f, 22000f, currentSignalStrength);
    }
}


    // ===============================
    // UNLOCK
    // ===============================
    public void UnlockStation(RadioStation station)
    {
        if (!activeStations.Contains(station))
        {
            activeStations.Add(station);
            Debug.Log($"📻 Radio unlocked: {station.displayName}");
        }
    }

    // ===============================
    // PLAY
    // ===============================
    public void PlayStation(RadioStation station)
    {
        if (!activeStations.Contains(station))
        {
            Debug.LogWarning("Station not unlocked!");
            return;
        }

        currentStation = station;
        currentClipIndex = 0;

        PlayCurrentClip();
    }

    private void PlayCurrentClip()
    {
        if (currentStation == null || currentStation.audioClips.Length == 0)
            return;

        audioSource.clip = currentStation.audioClips[currentClipIndex];
        audioSource.Play();

        CancelInvoke();
        Invoke(nameof(NextClip), audioSource.clip.length);
    }

    private void NextClip()
    {
        if (currentStation == null) return;

        currentClipIndex++;

        if (currentClipIndex >= currentStation.audioClips.Length)
        {
            if (!currentStation.loopPlaylist)
                return;

            currentClipIndex = 0;
        }

        PlayCurrentClip();
    }

    // ===============================
    // STOP
    // ===============================
    public void StopRadio()
    {
        CancelInvoke();
        audioSource.Stop();
        currentStation = null;
    }
}
