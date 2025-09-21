using UnityEngine;

public class StoryNote : MonoBehaviour
{
    [TextArea]
    public string[] noteTexts;   // Birden fazla metin
    public AudioClip[] radioClips; // Her metne karşılık ses (opsiyonel)
}
