using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class NoteTrigger : MonoBehaviour
{
    public GameObject notePanel;
    public TextMeshProUGUI noteTextUI;
    public AudioSource audioSource;

    private bool isPlayerNearby = false;
    private StoryNote currentNote;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentNote = GetComponent<StoryNote>();
            isPlayerNearby = true;

            if (notePanel != null && currentNote != null)
            {
                notePanel.SetActive(true);
                noteTextUI.text = currentNote.noteText;

                if (currentNote.radioClip != null && audioSource != null)
                {
                    audioSource.clip = currentNote.radioClip;
                    audioSource.Play();
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            currentNote = null;

            if (notePanel != null)
                notePanel.SetActive(false);

            if (audioSource != null)
                audioSource.Stop();
        }
    }
}
