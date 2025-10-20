// MusicManager.cs

using UnityEngine;
using System.Collections; // Coroutine için
using System.Collections.Generic; // List için

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Playlists")]
    public List<AudioClip> dayMusicPlaylist;
    public List<AudioClip> nightMusicPlaylist;

    [Header("Settings")]
    [Tooltip("Şarkılar arasındaki geçişin yumuşaklığı (saniye).")]
    public float crossfadeDuration = 2.0f;

    private AudioSource audioSource;
    private bool isDay = true;
    private int currentTrackIndex = -1;

    private bool isAppPaused = false;
    private bool wasPausedByFocus = false;

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu yöneticinin sahneler arası geçişte kalmasını sağlar.
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false; // Şarkı bitince Coroutine ile yenisini başlatacağız.
    }

    void Update()
    {
        // Uygulama odak dışındayken/pauselayken asla parça değiştirme
        if (isAppPaused) return;

        // Sadece gerçekten şarkı bitince yeni parçaya geç
        if (!audioSource.isPlaying && (isDay ? dayMusicPlaylist.Count > 0 : nightMusicPlaylist.Count > 0))
        {
            PlayNextTrack();
        }
    }


    // DayNightCycle bu fonksiyonu çağırarak durumu bildirir.
    public void SetDay(bool isCurrentlyDay)
    {
        // Eğer durum zaten aynıysa, bir şey yapma.
        if (this.isDay == isCurrentlyDay) return;

        this.isDay = isCurrentlyDay;

        // Yeni duruma göre yeni bir şarkı başlat.
        StartCoroutine(CrossfadeToNextTrack());
    }

    private void PlayNextTrack()
    {
        List<AudioClip> currentPlaylist = isDay ? dayMusicPlaylist : nightMusicPlaylist;
        if (currentPlaylist.Count == 0) return;

        // Rastgele bir sonraki şarkı seç (aynı şarkıyı tekrar çalmasın diye kontrol edebiliriz).
        int nextTrackIndex = Random.Range(0, currentPlaylist.Count);
        if (currentPlaylist.Count > 1 && nextTrackIndex == currentTrackIndex)
        {
            nextTrackIndex = (nextTrackIndex + 1) % currentPlaylist.Count;
        }
        currentTrackIndex = nextTrackIndex;

        audioSource.clip = currentPlaylist[currentTrackIndex];
        audioSource.Play();
    }

    private IEnumerator CrossfadeToNextTrack()
    {
        float startVolume = audioSource.volume;

        // Sesi yavaşça kıs
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / crossfadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Sesi eski haline getir

        // Yeni duruma göre bir sonraki şarkıyı çal
        PlayNextTrack();
    }

    // Uygulama duraklatıldığında/geri gelince çağrılır
    void OnApplicationPause(bool pause)
    {
        isAppPaused = pause;
        if (audioSource == null) return;

        if (pause)
        {
            // Odak gidince o anki şarkıyı duraklat (clip ve time korunur)
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                wasPausedByFocus = true;
            }
        }
        else
        {
            // Geri dönünce aynen kaldığı yerden devam et
            if (wasPausedByFocus)
            {
                audioSource.UnPause();
                wasPausedByFocus = false;
            }
        }
    }

    // Bazı platformlarda sadece focus tetiklenir; aynı mantığı yönlendir
    void OnApplicationFocus(bool hasFocus)
    {
        OnApplicationPause(!hasFocus);
    }

    
}