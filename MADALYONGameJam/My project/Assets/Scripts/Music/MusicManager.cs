using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip track1;
    public AudioClip track2;

    private AudioSource source;
    private int lastPlayed = -1;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
        PlayRandomTrack();
    }

    void Update()
    {
        if (source.clip == null) return;

        // GERÇEK bitiş kontrolü
        if (source.time >= source.clip.length - 0.1f)
        {
            PlayRandomTrack();
        }
    }

    private void PlayRandomTrack()
    {
        int nextTrack = Random.Range(0, 2);

        // Aynı şarkıyı iki kere çalma
        if (nextTrack == lastPlayed)
            nextTrack = (nextTrack + 1) % 2;

        lastPlayed = nextTrack;

        source.clip = (nextTrack == 0) ? track1 : track2;
        source.Play();
    }
}
