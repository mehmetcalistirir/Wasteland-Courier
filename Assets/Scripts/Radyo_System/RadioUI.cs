using UnityEngine;
using TMPro;

public class RadioUI : MonoBehaviour
{
    [Header("UI")]
    public Transform stationListParent;
    public GameObject stationButtonPrefab;
    public TMP_Text currentStationText;
    public UnityEngine.UI.Slider signalSlider;

    private void OnEnable()
    {
        RefreshStations();
        UpdateSignal();
    }

    private void Update()
    {
        UpdateSignal();
        UpdateCurrentStation();
    }

    void RefreshStations()
    {
        foreach (Transform c in stationListParent)
            Destroy(c.gameObject);

        foreach (var station in RadioSystem.Instance.activeStations)
        {
            GameObject btn = Instantiate(stationButtonPrefab, stationListParent);
            btn.GetComponent<RadioStationButtonUI>().Setup(station);
        }
    }

    void UpdateCurrentStation()
    {
        var current = RadioSystem.Instance.currentStation;
        currentStationText.text = current != null
            ? $"▶ {current.displayName}"
            : "No Signal";
    }

    void UpdateSignal()
    {
        signalSlider.value = RadioSystem.Instance.currentSignalStrength;
    }
}
