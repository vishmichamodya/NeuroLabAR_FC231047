using UnityEngine;

public class NetworkVisualizer : MonoBehaviour
{
    [Header("Drag the node objects here")]
    public Transform[] inputs = new Transform[2];
    public Transform[] hidden = new Transform[3];
    public Transform output;
    public Transform[] rings = new Transform[3];

    [Header("Extras shown in some lessons")]
    public GameObject targetFlag;
    public GameObject gradientArrows;

    [Header("Look")]
    public Material lineMaterial;
    public GameObject packetPrefab;
    public Color positiveColor = new Color(0.18f, 0.89f, 0.90f);
    public Color negativeColor = new Color(1f, 0.24f, 0.67f);
    public Color inputColor    = new Color(0.72f, 1f, 0.36f);
    public Color hiddenColor   = new Color(0.18f, 0.89f, 0.90f);
    public Color outputColor   = new Color(1f, 0.24f, 0.67f);

    [HideInInspector] public int highlightLayer = -1;
    [HideInInspector] public int pulseLayer = -1;

    public NeuralNet net = new NeuralNet();
    public float[] x = { 1f, 0f };
    public NeuralNet.Result result;

    Transform[] all;
    Renderer[] rends;
    Vector3[] baseScale, ringBase;
    LineRenderer[] lines = new LineRenderer[9];   // 0-5 input->hidden, 6-8 hidden->output
    Transform[] packets = new Transform[9];
    Vector3[] starts = new Vector3[9], ends = new Vector3[9];
    MaterialPropertyBlock mpb;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        all = new Transform[] { inputs[0], inputs[1], hidden[0], hidden[1], hidden[2], output };
        rends = new Renderer[6];
        baseScale = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            rends[i] = all[i].GetComponentInChildren<Renderer>();
            baseScale[i] = all[i].localScale;
        }
        ringBase = new Vector3[3];
        for (int j = 0; j < 3; j++) if (rings[j] != null) ringBase[j] = rings[j].localScale;

        int n = 0;
        for (int i = 0; i < 2; i++) for (int j = 0; j < 3; j++) MakeEdge(n++, inputs[i], hidden[j]);
        for (int j = 0; j < 3; j++) MakeEdge(n++, hidden[j], output);

        SetExtras(false, false);
        Refresh();
    }

    void MakeEdge(int idx, Transform a, Transform b)
    {
        GameObject go = new GameObject("Edge" + idx);
        go.transform.SetParent(transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, a.localPosition);
        lr.SetPosition(1, b.localPosition);
        lr.sharedMaterial = lineMaterial;
        lr.numCapVertices = 4;
        lines[idx] = lr;
        starts[idx] = a.localPosition;
        ends[idx] = b.localPosition;

        if (packetPrefab != null)
        {
            GameObject p = Instantiate(packetPrefab, transform);
            p.transform.localScale = Vector3.one * 0.02f;
            p.SetActive(false);
            packets[idx] = p.transform;
        }
    }

    float Weight(int e) { return e < 6 ? net.w1[e % 3, e / 3] : net.w2[e - 6]; }

    static Color Dim(Color c, float k) { return new Color(c.r * k, c.g * k, c.b * k, 1f); }

    // Call this whenever inputs or weights change
    public void Refresh()
    {
        result = net.Forward(x[0], x[1]);
        float[] act = { x[0], x[1], result.h[0], result.h[1], result.h[2], result.y };

        for (int i = 0; i < 6; i++)
        {
            if (rends[i] == null) continue;
            Color c = i < 2 ? inputColor : (i < 5 ? hiddenColor : outputColor);
            float a = Mathf.Clamp01(act[i]);
            mpb.Clear();
            mpb.SetColor("_BaseColor", Color.Lerp(Dim(c, 0.3f), c, a));
            mpb.SetColor("_EmissionColor", c * (0.2f + 1.8f * a));
            rends[i].SetPropertyBlock(mpb);
        }

        for (int j = 0; j < 3; j++)
            if (rings[j] != null) rings[j].localScale = ringBase[j] * (0.8f + 0.5f * result.h[j]);

        for (int e = 0; e < 9; e++)
        {
            float w = Weight(e);
            Color c = w >= 0f ? positiveColor : negativeColor;
            bool dim = pulseLayer >= 0 && pulseLayer != (e < 6 ? 0 : 1);
            c.a = dim ? 0.15f : 0.85f;
            lines[e].startColor = c;
            lines[e].endColor = c;
            float width = 0.002f + Mathf.Min(Mathf.Abs(w), 3f) * 0.0035f;   // thicker = stronger weight
            lines[e].startWidth = width;
            lines[e].endWidth = width;
        }
    }

    public void SetExtras(bool flag, bool grads)
    {
        if (targetFlag != null) targetFlag.SetActive(flag);
        if (gradientArrows != null) gradientArrows.SetActive(grads);
    }

    public void OnNodeTapped(int i)
    {
        if (i < 2)
        {
            x[i] = 1f - x[i];
            Refresh();
            HudToast.Show("x" + (i + 1) + " is now " + x[i]);
        }
        else if (i < 5)
        {
            int j = i - 2;
            HudToast.Show("h" + (j + 1) + ":  z = " + result.z1[j].ToString("F2") + ",  activation = " + result.h[j].ToString("F2"));
        }
        else
        {
            HudToast.Show("Output y = " + result.y.ToString("F3") + "  (z = " + result.z2.ToString("F2") + ")");
        }
    }

    void Update()
    {
        float t = Time.time;

        for (int i = 0; i < 6; i++)                // glowing "breathing" of the highlighted layer
        {
            int layer = i < 2 ? 0 : (i < 5 ? 1 : 2);
            float s = (layer == highlightLayer) ? 1f + 0.18f * Mathf.Sin(t * 6f) : 1f;
            all[i].localScale = baseScale[i] * s;
        }

        for (int e = 0; e < 9; e++)                // data packets travelling along connections
        {
            if (packets[e] == null) continue;
            bool on = pulseLayer == (e < 6 ? 0 : 1);
            packets[e].gameObject.SetActive(on);
            if (on) packets[e].localPosition = Vector3.Lerp(starts[e], ends[e], (t * 0.7f + e * 0.11f) % 1f);
        }

        if (gradientArrows != null && gradientArrows.activeSelf)
            gradientArrows.transform.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(t * 6f));
    }
}