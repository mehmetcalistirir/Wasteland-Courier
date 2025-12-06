using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerAbilityUI : MonoBehaviour
{
    public static PlayerAbilityUI instance;

    public GameObject buttonPrefab;      // AbilityButtonPrefab
    public Transform buttonContainer;    // Vertical Layout Group

    Dictionary<AbilityType, Button> activeButtons = new Dictionary<AbilityType, Button>();

    void Awake() => instance = this;

    // UI’ye yeni bir buton ekle
    public void AddAbilityButton(AbilityType type, System.Action action)
    {
        if (activeButtons.ContainsKey(type))
            return; // zaten var

        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        Button btn = btnObj.GetComponent<Button>();

        btn.onClick.AddListener(() => action());

        btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = type.ToString();

        activeButtons[type] = btn;
    }

    // Butonu kaldır (geçici yetenek bittiğinde)
    public void RemoveAbilityButton(AbilityType type)
    {
        if (!activeButtons.ContainsKey(type))
            return;

        Destroy(activeButtons[type].gameObject);
        activeButtons.Remove(type);
    }
}
