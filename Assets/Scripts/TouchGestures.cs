using UnityEngine;

public class TouchGestures : MonoBehaviour
{
    public float minScale = 0.4f;
    public float maxScale = 3f;

    void Update()
    {
        if (Input.touchCount != 2) return;
        Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
        Vector2 prev0 = t0.position - t0.deltaPosition;
        Vector2 prev1 = t1.position - t1.deltaPosition;

        // PINCH: distance between fingers changed -> scale
        float prevDist = Vector2.Distance(prev0, prev1);
        float curDist = Vector2.Distance(t0.position, t1.position);
        if (prevDist > 1f)
        {
            float s = Mathf.Clamp(transform.localScale.x * (curDist / prevDist), minScale, maxScale);
            transform.localScale = Vector3.one * s;
        }

        // TWIST: angle between fingers changed -> rotate
        float prevAng = Mathf.Atan2(prev1.y - prev0.y, prev1.x - prev0.x) * Mathf.Rad2Deg;
        float curAng = Mathf.Atan2(t1.position.y - t0.position.y, t1.position.x - t0.position.x) * Mathf.Rad2Deg;
        transform.Rotate(0f, -Mathf.DeltaAngle(prevAng, curAng), 0f, Space.World);
    }
}