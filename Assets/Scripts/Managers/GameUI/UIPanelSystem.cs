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
        CloseAllPanels();                 // Önce tüm panelleri kapat
        currentPanel = panel;

        panel.SetActive(true);

        // Panel dışındaki tüm UI elemanlarını gizle
        HideEverythingExcept(panel);

        // Paneli en üste al
        panel.transform.SetAsLastSibling();

        // Oyunu durdur
        Time.timeScale = 1f;
    }

    // ------------------ PANEL KAPATMA ------------------
    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        // Tüm UI'ı geri aç
        ShowEverything();

        // Oyunu geri başlat
        Time.timeScale = 1f;
    }

    public void CloseAllPanels()
    {
        foreach (var p in allPanels)
            p.SetActive(false);

        currentPanel = null;
    }

    // ------------------ TÜM UI’YI GİZLE / GÖSTER ------------------

   private void HideEverythingExcept(GameObject panel)
{
    foreach (var p in allPanels)
        p.SetActive(p == panel);

    panel.transform.SetAsLastSibling();
}




    private void ShowEverything()
    {
        foreach (Transform t in mainCanvas.transform)
            t.gameObject.SetActive(true);
    }
}
