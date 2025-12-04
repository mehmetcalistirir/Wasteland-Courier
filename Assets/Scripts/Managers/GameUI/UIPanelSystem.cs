using UnityEngine;
using System.Collections.Generic;

public class UIPanelSystem : MonoBehaviour
{
    public static UIPanelSystem Instance;

    [Header("Ana Canvas (Canvas kök)")]
    public Canvas mainCanvas;

    [Header("Tüm Paneller (Inspector’dan ekle)")]
    public List<GameObject> allPanels;

    private GameObject currentPanel;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsPanelOpen() => currentPanel != null;

    // ------------------ PANEL AÇMA ------------------

    public void OpenPanel(GameObject panel)
    {
        CloseAllPanels();             // Önce hepsini kapat
        currentPanel = panel;

        panel.SetActive(true);

        HideEverythingExcept(panel);
        Time.timeScale = 0f;
    }

    // ------------------ PANEL KAPATMA ------------------

    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        ShowEverything();
        Time.timeScale = 1f;
    }

    public void CloseAllPanels()
    {
        foreach (var p in allPanels)
            p.SetActive(false);

        currentPanel = null;
    }

    // ------------------ TÜM UI’Yİ GİZLE / GÖSTER ------------------

    private void HideEverythingExcept(GameObject panel)
    {
        foreach (Transform t in mainCanvas.transform)
        {
            if (t.gameObject != panel)
                t.gameObject.SetActive(false);
        }
    }

    private void ShowEverything()
    {
        foreach (Transform t in mainCanvas.transform)
            t.gameObject.SetActive(true);
    }
}
