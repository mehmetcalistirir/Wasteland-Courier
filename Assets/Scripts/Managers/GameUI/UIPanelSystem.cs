using UnityEngine;
using System.Collections.Generic;

public class UIPanelSystem : MonoBehaviour
{
    public static UIPanelSystem Instance;

    public List<GameObject> allPanels;

    private GameObject currentPanel;
    private List<GameObject> hiddenCanvases = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsPanelOpen() => currentPanel != null;

    // ---------------- PANEL AÇMA ----------------

    public void OpenPanel(GameObject panel)
    {
        CloseAllPanels();

        currentPanel = panel;
        panel.SetActive(true);

        HideAllCanvasExceptPanel(panel);

        Time.timeScale = 0f;
    }

    // ---------------- PANEL KAPATMA ----------------

    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        RestoreHiddenCanvas();

        Time.timeScale = 1f;
    }

    public void CloseAllPanels()
    {
        foreach (var p in allPanels)
            p.SetActive(false);

        currentPanel = null;
    }

    // -------------- KAPATMA MANTIĞI ----------------

    private void HideAllCanvasExceptPanel(GameObject panel)
    {
        hiddenCanvases.Clear();

        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.gameObject == panel) 
                continue;

            // Canvas’ı kapat
            c.gameObject.SetActive(false);
            hiddenCanvases.Add(c.gameObject);
        }
    }

    private void RestoreHiddenCanvas()
    {
        foreach (var go in hiddenCanvases)
            go.SetActive(true);

        hiddenCanvases.Clear();
    }
}
