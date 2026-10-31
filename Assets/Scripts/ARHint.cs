using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARHint : MonoBehaviour
{
    public GameObject hintPanel;            // the box inside the Lab panel
    public TextMeshProUGUI hintText;
    public ARPlaneManager planeManager;     // drag XR Origin here
    public ARPlacement placement;           // drag XR Origin here

    void Update()
    {
        if (ScreenManager.Current != "Lab") return;

        string msg = "";
        if (ARSession.state != ARSessionState.SessionTracking)
            msg = "Tracking lost. Move slowly, add light and point at a textured surface.";
        else if (!placement.HasPlaced)
            msg = planeManager.trackables.count == 0
                ? "Move your phone slowly to scan the floor or a table. Plain white or shiny surfaces are hard to track."
                : "Surface found! Tap it to place the network.";

        hintPanel.SetActive(msg != "");
        hintText.text = msg;
    }
}