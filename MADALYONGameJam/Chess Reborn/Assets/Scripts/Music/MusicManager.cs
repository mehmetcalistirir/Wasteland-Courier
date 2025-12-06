using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip track1;
    public AudioClip track2;

    private AudioSource source;
    private int lastPlayed = -1;

    public static MusicManager instance;

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
        PlayRandomTrack();
    }

    void Update()
    {
        if (source.clip == null) return;

        // Şarkı doğal olarak bittiyse tekrar rastgele çal
        if (source.time >= source.clip.length - 0.1f)
        {
            PlayRandomTrack();
        }
    }

    private void PlayRandomTrack()
    {
        int nextTrack = Random.Range(0, 2);

        // Aynı şarkıyı 2 kere üst üste çalma
        if (nextTrack == lastPlayed)
            nextTrack = (nextTrack + 1) % 2;

        lastPlayed = nextTrack;

        source.clip = (nextTrack == 0) ? track1 : track2;
        source.Play();
    }

    // 🔥 Restart'a basınca çağrılacak
    public void StopMusic()
    {
        if (source != null)
            source.Stop();
    }
}
