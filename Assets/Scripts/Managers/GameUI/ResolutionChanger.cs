using UnityEngine;
using TMPro;

public class ResolutionChanger : MonoBehaviour
{
    private TMP_Dropdown resolutionDropdown;
    private int lastIndex = -1;

    private readonly Vector2[] resolutions =
    {
        new Vector2(320,200),
        new Vector2(320,240),
        new Vector2(400,300),
        new Vector2(512,384),
        new Vector2(640,400),
        new Vector2(640,480),
        new Vector2(800,600),
        new Vector2(1024,768),
        new Vector2(1152,864),
        new Vector2(1280,600),
        new Vector2(1280,720),
        new Vector2(1280,768),
        new Vector2(1280,800),
        new Vector2(1280,960),
        new Vector2(1280,1024),
        new Vector2(1360,768),
        new Vector2(1366,768),
        new Vector2(1400,1050),
        new Vector2(1440,900),
        new Vector2(1600,900),
        new Vector2(1680,1050),
        new Vector2(1920,1080),
    };

    private void Awake()
    {
        resolutionDropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        int saved = PlayerPrefs.GetInt("ResolutionIndex", 0);

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.value = saved;
        lastIndex = saved;

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetResolution(int index)
    {
        if (index == lastIndex) return;
        lastIndex = index;

        Vector2 res = resolutions[index];
        Screen.SetResolution((int)res.x, (int)res.y, FullScreenMode.Windowed);

        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();

        Debug.Log($"Resolution changed to {res.x}x{res.y}");
    }
}
