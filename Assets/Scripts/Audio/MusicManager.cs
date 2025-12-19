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
        Debug.Log("🎵 MusicManager Awake ÇALIŞTI! Playlist Day=" 
          + dayMusicPlaylist.Count 
          + " Night=" + nightMusicPlaylist.Count);

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
public void StopAll()
{
    if (audioSource != null)
    {
        audioSource.Stop();
        audioSource.clip = null;      // Clip temizlensin
        audioSource.time = 0f;        // Parça zamanı reset
        // audioSource.enabled = false;  // ❌ KALDIRILDI!
    }

    currentTrackIndex = -1;
}




    // DayNightCycle bu fonksiyonu çağırarak durumu bildirir.
    public void SetDay(bool isCurrentlyDay)
{
    // Durumu her zaman güncelle
    this.isDay = isCurrentlyDay;

    // Müzik çalmıyorsa direkt başlat
    if (!audioSource.isPlaying || audioSource.clip == null)
    {
        PlayNextTrack();
        return;
    }

    // Eğer müzik çalıyorsa crossfade yap
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
    // Sesi yavaşça kıs (0'a kadar)
    while (audioSource.volume > 0)
    {
        audioSource.volume -= Time.deltaTime / crossfadeDuration;
        yield return null;
    }

    audioSource.Stop();
    
    // Yeni şarkıyı seç ve başlat
    PlayNextTrack();

    // Sesi yavaşça aç (1'e kadar)
    // Mixer grubuna bağlı olduğu için 1 değeri aslında kullanıcının slider ayarıdır.
    while (audioSource.volume < 1)
    {
        audioSource.volume += Time.deltaTime / crossfadeDuration;
        yield return null;
    }
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