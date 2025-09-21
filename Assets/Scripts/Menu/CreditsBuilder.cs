// CreditsBuilder.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreditsBuilder : MonoBehaviour
{
    [System.Serializable]
    public class Section
    {
        public string title;
        [TextArea] public string[] lines;
    }

    public Transform content;           // Scroll View -> Viewport -> Content
    public TextMeshProUGUI sectionTitlePrefab;
    public TextMeshProUGUI linePrefab;

    public Section[] sections;

    void Start()
    {
        Build();
    }


    public void Build()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        foreach (var s in sections)
        {
            if (!string.IsNullOrEmpty(s.title))
            {
                var title = Instantiate(sectionTitlePrefab, content);
                title.text = $"<b>{s.title}</b>";
            }

            foreach (var line in s.lines)
            {
                var l = Instantiate(linePrefab, content);
                l.text = line;
            }

            var spacer = Instantiate(linePrefab, content);
            spacer.text = "\n";
        }

        // --- kritik: layout’ı hemen hesaplat ---
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);

        // baştan başla
        ((RectTransform)content).anchoredPosition = Vector2.zero;
    }


}
