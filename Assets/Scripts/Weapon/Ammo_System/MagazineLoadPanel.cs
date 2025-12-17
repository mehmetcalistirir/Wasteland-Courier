using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagazineLoadPanel : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text ammoInfoText;
    public Slider loadSlider;

    public Button btnLoad;
    public Button btnFullLoad;
    public Button btnClose; // 🔥 YENİ


    private MagazineInstance currentMag;

    private void Awake()
    {
        gameObject.SetActive(false);

        loadSlider.onValueChanged.AddListener(_ =>
        {
            RefreshText();
        });

        if (btnClose != null)
            btnClose.onClick.AddListener(Hide);
    }

    private void Update()
    {
        // 🔥 ESC ile kapatma
        if (!gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void Show(MagazineInstance mag)
    {
        if (mag == null || mag.data == null)
            return;

        currentMag = mag;
        gameObject.SetActive(true);

        titleText.text = mag.data.itemName;

        int availableAmmo =
            Inventory.Instance.GetAmmoAmount(mag.data.ammoType);

        int space = mag.data.capacity - mag.currentAmmo;

        loadSlider.minValue = 0;
        loadSlider.maxValue = Mathf.Min(space, availableAmmo);
        loadSlider.value = loadSlider.maxValue;

        bool canLoad = loadSlider.maxValue > 0;
        btnLoad.interactable = canLoad;
        btnFullLoad.interactable = canLoad;

        RefreshText();
    }


    public void Hide()
    {
        currentMag = null;
        gameObject.SetActive(false);
    }

    public void RefreshText()
    {
        if (currentMag == null) return;

        ammoInfoText.text =
            $"{currentMag.currentAmmo}/{currentMag.data.capacity}  " +
            $"→ +{(int)loadSlider.value}";
    }

    public void OnLoadPressed()
    {
        if (currentMag == null) return;

        Inventory.Instance.LoadAmmoIntoMagazine(
            currentMag,
            (int)loadSlider.value
        );

        Show(currentMag); // refresh
    }

    public void OnFullLoadPressed()
    {
        if (currentMag == null) return;

        Inventory.Instance.FullLoadMagazine(currentMag);
        Show(currentMag);
    }
}
