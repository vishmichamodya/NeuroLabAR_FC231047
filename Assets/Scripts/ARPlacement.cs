using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacement : MonoBehaviour
{
    public static event Action<NetworkVisualizer> Placed;   // null means "removed"

    public ARRaycastManager raycastManager;   // drag XR Origin here
    public ARPlaneManager planeManager;       // drag XR Origin here
    public GameObject networkPrefab;          // drag the NetworkRoot prefab here
    public AudioSource sfx;                   // optional
    public AudioClip placeSound;              // optional

    GameObject placed;
    bool planesVisible = true;
    readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public bool HasPlaced { get { return placed != null; } }

    void OnEnable()  { planeManager.trackablesChanged.AddListener(OnPlanesChanged); }
    void OnDisable() { planeManager.trackablesChanged.RemoveListener(OnPlanesChanged); }
    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args) { ApplyPlaneVisibility(); }

    void ApplyPlaneVisibility()
    {
        foreach (ARPlane p in planeManager.trackables) p.gameObject.SetActive(planesVisible);
    }

    // called by the "Planes" toggle in the HUD
    public void SetPlanesVisible(bool visible)
    {
        planesVisible = visible;
        ApplyPlaneVisibility();
    }

    // called by the "Re-place" button
    public void ResetPlacement()
    {
        if (placed != null) Destroy(placed);
        placed = null;
        SetPlanesVisible(true);
        Placed?.Invoke(null);
    }

    void Update()
    {
        if (ScreenManager.Current != "Lab") return;
        if (!TryGetTap(out Vector2 pos)) return;
        if (IsTapOnUI()) return;

        // 1. Did the tap hit a node? (normal Unity ray)
        Ray ray = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NodeRef node = hit.collider.GetComponentInParent<NodeRef>();
            if (node != null) { node.OnTap(); return; }
        }

        // 2. Otherwise: AR raycast against detected surfaces
        if (raycastManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            if (placed == null)
            {
                Quaternion rot = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
                placed = Instantiate(networkPrefab, pose.position, rot);
                SetPlanesVisible(false);
                Placed?.Invoke(placed.GetComponent<NetworkVisualizer>());
                HudToast.Show("Placed! Pinch to resize, twist to rotate, tap the nodes.");
            }
            else
            {
                placed.transform.position = pose.position;      // tap again to move it
            }
            if (sfx != null && placeSound != null) sfx.PlayOneShot(placeSound);
        }
    }

    bool TryGetTap(out Vector2 position)
    {
        position = Vector2.zero;
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        { position = Input.GetTouch(0).position; return true; }
        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
        { position = Input.mousePosition; return true; }
        return false;
    }

    bool IsTapOnUI()
    {
        if (EventSystem.current == null) return false;
        if (Input.touchCount > 0) return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return EventSystem.current.IsPointerOverGameObject();
    }
}