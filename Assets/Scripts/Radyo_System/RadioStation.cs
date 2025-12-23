using UnityEngine;


public enum UnlockType
{
    Zone,
    Story,
    Item
}

[CreateAssetMenu(menuName = "Radio/Radio Station")]
public class RadioStation : ScriptableObject
{
    [Header("Info")]
    public string stationId;          // "wasteland_news"
    public string displayName;        // "Wasteland News"

    [Header("Unlock")]
    public UnlockType unlockType;

    [Header("Audio")]
    public AudioClip[] audioClips;
    public bool loopPlaylist = true;
}
