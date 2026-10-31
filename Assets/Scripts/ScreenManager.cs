using System;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [Serializable]
    public class Entry { public string name; public GameObject panel; }

    public Entry[] screens;
    public string startScreen = "Home";

    public static string Current { get; private set; }
    public static event Action<string> Changed;

    void Start() { Show(startScreen); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))      // Android Back button
        {
            if (Current == "Home") Application.Quit();
            else Show("Home");
        }
    }

    // Buttons call this with a text value, e.g. "Lab"
    public void Show(string screenName)
    {
        Current = screenName;
        foreach (Entry s in screens) s.panel.SetActive(s.name == screenName);
        Changed?.Invoke(screenName);
    }
}