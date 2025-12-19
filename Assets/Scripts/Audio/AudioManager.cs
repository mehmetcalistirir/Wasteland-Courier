// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioMixer mainMixer;            // MainMixer.asset
    public AudioMixerGroup musicGroup;      // Inspector: Music
    public AudioMixerGroup sfxGroup;        // Inspector: SFX

    [Tooltip("Exposed param adları")]
    public string musicParam = "MusicVolume";
    public string sfxParam = "SFXVolume";
    public string masterParam = "MasterVolume";

   void Awake()
{
    // 1. ADIM: Singleton ve Kalıcılık Kontrolü
    if (Instance == null) 
    { 
        Instance = this; 
        DontDestroyOnLoad(gameObject); // Bu obje sahneler değiştikçe silinmez
    }
    else 
    { 
        Destroy(gameObject); // Eğer zaten bir AudioManager varsa, yeni geleni yok et
        return; 
    }

    // 2. ADIM: Ayarları Yükle (Sadece ilk oluşan AudioManager için çalışır)
    float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
    float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
    float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
    
    SetMasterVolume(master);
    SetMusicVolume(music);
    SetSFXVolume(sfx);
}

    float ToDecibels(float value) => (value <= 0.0001f) ? -80f : Mathf.Log10(value) * 20f;

    public void SetMasterVolume(float v)
{
    float db = ToDecibels(v);
    Debug.Log($"Master Volume Slider: {v} -> dB: {db}"); // Konsolda bu yazıyı görüyor musunuz?
    bool success = mainMixer.SetFloat(masterParam, db);
    if(!success) Debug.LogError($"{masterParam} isimli parametre Mixer'da bulunamadı!");
    PlayerPrefs.SetFloat("MasterVolume", v);
}
    public void SetMusicVolume(float v)
    {
        mainMixer.SetFloat(musicParam, ToDecibels(v));
        PlayerPrefs.SetFloat("MusicVolume", v);
    }

    public void SetSFXVolume(float v)
    {
        mainMixer.SetFloat(sfxParam, ToDecibels(v));
        PlayerPrefs.SetFloat("SFXVolume", v);
    }

    // PlayClipAtPoint alternatifi: miksere bağlı tek-seferlik çalma
    public void PlayOneShotAt(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (clip == null || sfxGroup == null) return;
        var go = new GameObject("OneShotSFX");
        go.transform.position = pos;
        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = sfxGroup;
        src.spatialBlend = 0f; // 2D UI/SFX ise 0f, 3D istiyorsan 1f’e çekebilirsin
        src.PlayOneShot(clip, volume);
        Destroy(go, clip.length + 0.1f);
    }

    // Bir AudioSource’u programatik olarak SFX’e bağlamak istersen:
    public void RouteToSFX(AudioSource src)
    {
        if (src != null && sfxGroup != null) src.outputAudioMixerGroup = sfxGroup;
    }

    public void RouteToMusic(AudioSource src)
    {
        if (src != null && musicGroup != null) src.outputAudioMixerGroup = musicGroup;
    }
}
