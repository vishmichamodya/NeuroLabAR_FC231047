using TMPro;
using UnityEngine;

public class HudToast : MonoBehaviour
{
    public static HudToast I;
    public GameObject panel;
    public TextMeshProUGUI label;
    float hideAt;

    void Awake() { I = this; panel.SetActive(false); }

    public static void Show(string text)
    {
        if (I == null) return;
        I.label.text = text;
        I.panel.SetActive(true);
        I.hideAt = Time.time + 3f;
    }

    void Update()
    {
        if (panel.activeSelf && Time.time > hideAt) panel.SetActive(false);
    }
}