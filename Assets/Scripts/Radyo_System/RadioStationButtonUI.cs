using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadioStationButtonUI : MonoBehaviour
{
    public TMP_Text label;
    private RadioStation station;

    public void Setup(RadioStation stationData)
    {
        station = stationData;
        label.text = station.displayName;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        RadioSystem.Instance.PlayStation(station);
    }
}
