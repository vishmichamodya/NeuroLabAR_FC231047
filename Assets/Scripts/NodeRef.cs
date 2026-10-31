using UnityEngine;

public class NodeRef : MonoBehaviour
{
    public int index;    // 0,1 = inputs   2,3,4 = hidden   5 = output

    public void OnTap()
    {
        NetworkVisualizer v = GetComponentInParent<NetworkVisualizer>();
        if (v != null) v.OnNodeTapped(index);
    }
}