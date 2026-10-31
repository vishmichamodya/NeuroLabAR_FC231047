using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WidgetController : MonoBehaviour
{
    [Header("Panels (one per widget)")]
    public GameObject inputsPanel, activationPanel, lossPanel, trainingPanel;
    [Header("Texts")]
    public TextMeshProUGUI inputsText, activationText, lossText, trainingText;
    [Header("Controls")]
    public Slider zSlider, predSlider;
    public Image lossBar;                // Image Type = Filled, Fill Method = Horizontal

    NetworkVisualizer viz;
    string current = "None";
    bool relu;
    int epochs;

    public void Show(string widget, int mode, NetworkVisualizer v)
    {
        viz = v;
        current = widget;
        inputsPanel.SetActive(widget == "Inputs");
        activationPanel.SetActive(widget == "Activation");
        lossPanel.SetActive(widget == "Loss");
        trainingPanel.SetActive(widget == "Training");

        if (widget == "Activation") { relu = (mode == 1); OnZChanged(zSlider.value); }
        if (widget == "Loss") OnPredChanged(predSlider.value);
    }

    void Update()          // live text for the widgets that depend on the network
    {
        if (current == "Inputs")
        {
            if (viz == null) { inputsText.text = "Place the network first: tap a glowing surface."; return; }
            NeuralNet.Result r = viz.result;
            inputsText.text = "Inputs (x1, x2) = (" + viz.x[0] + ", " + viz.x[1] + ")\n"
                + "h1: z = " + r.z1[0].ToString("F2") + "  ->  a = " + r.h[0].ToString("F2") + "\n"
                + "Prediction y = " + r.y.ToString("F3") + "\nTap a green input node to flip 0 / 1.";
        }
        else if (current == "Training")
        {
            if (viz == null) { trainingText.text = "Place the network first."; return; }
            string s = "Loss: " + viz.net.Loss().ToString("F4") + "   Epochs: " + epochs + "\n";
            for (int k = 0; k < 4; k++)
                s += "(" + NeuralNet.X[k, 0] + "," + NeuralNet.X[k, 1] + ") -> "
                   + viz.net.Forward(NeuralNet.X[k, 0], NeuralNet.X[k, 1]).y.ToString("F2")
                   + " (want " + NeuralNet.T[k] + ")   ";
            trainingText.text = s;
        }
    }

    // ---- Activation widget (slider On Value Changed -> OnZChanged) ----
    public void OnZChanged(float z)
    {
        float a = relu ? Mathf.Max(0f, z) : NeuralNet.Sigmoid(z);
        string name = relu ? "ReLU" : "Sigmoid";
        activationText.text = "z = " + z.ToString("F1") + "\n" + name + "(z) = " + a.ToString("F3");
    }
    public void UseSigmoid() { relu = false; OnZChanged(zSlider.value); }
    public void UseRelu()    { relu = true;  OnZChanged(zSlider.value); }

    // ---- Loss widget (slider On Value Changed -> OnPredChanged) ----
    public void OnPredChanged(float p)
    {
        float error = 1f - p;
        float loss = error * error;
        lossBar.fillAmount = loss;
        lossText.text = "prediction = " + p.ToString("F2") + "   target = 1\nerror = " + error.ToString("F2")
                      + "   loss = " + loss.ToString("F3");
    }

    // ---- Training widget (buttons) ----
    public void TrainClicked()
    {
        if (viz == null) return;
        viz.net.Train(500);
        epochs += 500;
        viz.Refresh();
    }

    public void NewWeights()
    {
        if (viz == null) return;
        viz.net.Randomize();
        epochs = 0;
        viz.Refresh();
    }
}