using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    Rect last;

    void Update()
    {
        Rect r = Screen.safeArea;
        if (r == last) return;
        last = r;
        RectTransform rt = (RectTransform)transform;
        Vector2 min = r.position, max = r.position + r.size;
        min.x /= Screen.width;  min.y /= Screen.height;
        max.x /= Screen.width;  max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}