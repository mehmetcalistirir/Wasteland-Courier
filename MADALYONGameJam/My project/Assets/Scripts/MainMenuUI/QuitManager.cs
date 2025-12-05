using UnityEngine;

public class QuitManager : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Oyun kapatılıyor...");

        #if UNITY_EDITOR
        // Editor içindeyken oyunu durdur
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Build alınmış oyunu kapat
        Application.Quit();
        #endif
    }
}
