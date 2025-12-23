using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PipBoyController : MonoBehaviour
{
    public static PipBoyController Instance;

    [Header("Root")]
    public GameObject pipBoyPanel;
    public bool IsOpen { get; private set; }


    [Header("Pages")]
    public GameObject inventoryPage;
    public GameObject statsPage;
    public GameObject radioPage;

    [Header("UI")]
    public TMP_Text titleText;

    private int currentIndex;

    private GameObject[] pages;
    private string[] titles = { "INVENTORY", "STATUS", "RADIO" };

    private PlayerControls controls;

    private void Awake()
    {
        Instance = this;

        pages = new GameObject[]
        {
            inventoryPage,
            statsPage,
            radioPage
        };

        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.UI.Navigate.performed += OnNavigate;
        controls.UI.Escape.performed += OnEscape;
    }

    private void OnDisable()
    {
        controls.UI.Navigate.performed -= OnNavigate;
        controls.UI.Escape.performed -= OnEscape;
    }

    // ================= OPEN / CLOSE =================
    public void Open(int startIndex)
{
    IsOpen = true;

    pipBoyPanel.SetActive(true);

    currentIndex = Mathf.Clamp(startIndex, 0, pages.Length - 1);
    RefreshPages();

    GameStateManager.SetPaused(true);
    controls.UI.Enable();

    
}


    public void Close()
{
    IsOpen = false;

    pipBoyPanel.SetActive(false);

    GameStateManager.SetPaused(false);
    controls.UI.Disable();

    
}


    // ================= PAGE NAVIGATION =================
    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();

        if (dir.x > 0.5f)
            NextPage();
        else if (dir.x < -0.5f)
            PreviousPage();
    }

    private void NextPage()
    {
        currentIndex = (currentIndex + 1) % pages.Length;
        RefreshPages();
    }

    private void PreviousPage()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = pages.Length - 1;

        RefreshPages();
    }

    private void RefreshPages()
{
    Debug.Log("RefreshPages CALLED");

    for (int i = 0; i < pages.Length; i++)
    {
        Debug.Log($"Page {i}: {pages[i].name}");
        pages[i].SetActive(i == currentIndex);
    }

    if (titleText != null)
        titleText.text = titles[currentIndex];
}

    // ================= ESC =================
    private void OnEscape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Close();
    }
}
