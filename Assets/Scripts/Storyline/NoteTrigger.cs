using UnityEngine;
using TMPro;

public class NoteTrigger : MonoBehaviour
{
    public GameObject notePanel;
    public TextMeshProUGUI noteTextUI;
    public AudioSource audioSource;

    private bool isPlayerNearby = false;
    private StoryNote currentNote;
    private int currentIndex = 0; // sırayla ya da rastgele

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentNote = GetComponent<StoryNote>();
            isPlayerNearby = true;

            if (notePanel != null && currentNote != null && currentNote.noteTexts.Length > 0)
            {
                notePanel.SetActive(true);

                // İstersen sırayla:
                string text = currentNote.noteTexts[currentIndex];
                noteTextUI.text = text;

                if (currentNote.radioClips != null && currentNote.radioClips.Length > currentIndex && audioSource != null)
                {
                    audioSource.clip = currentNote.radioClips[currentIndex];
                    audioSource.Play();
                }

                // sıradakine geç (loop)
                currentIndex = (currentIndex + 1) % currentNote.noteTexts.Length;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            notePanel.SetActive(false);
            if (audioSource != null) audioSource.Stop();
        }
    }
}
