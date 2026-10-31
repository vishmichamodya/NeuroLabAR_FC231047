using TMPro;
using UnityEngine;

public class InstructionsScreen : MonoBehaviour
{
    public ScreenManager screens;
    public QuizManager quiz;
    public TextMeshProUGUI titleText, bodyText, counterText, nextLabel;

    static readonly string[] Titles = {
        "1. Allow the camera", "2. Scan and tap", "3. Step through",
        "4. Touch everything", "5. Listen and read", "6. Quiz, learn, quiz" };

    static readonly string[] Bodies = {
        "NeuroLab shows the lab in your room. The camera is used live and nothing is saved. Stay aware of your surroundings.",
        "Move your phone slowly over a floor or desk with good light. When a surface is found, tap it to place the network.",
        "Use Next and Back. Each step moves the 3D network and highlights the matching code line together.",
        "Walk around the network. Tap input nodes to flip 0 and 1. Pinch to resize and twist to rotate with two fingers. Use the sliders and buttons.",
        "Every step is narrated and has subtitles. Use the Sound button any time.",
        "A short pre-quiz, five lab scenes, then a post-quiz shows how much you learned." };

    int index;

    void OnEnable()  { ScreenManager.Changed += OnScreen; }
    void OnDisable() { ScreenManager.Changed -= OnScreen; }
    void OnScreen(string s) { if (s == "Instructions") { index = 0; Refresh(); } }

    void Refresh()
    {
        titleText.text = Titles[index];
        bodyText.text = Bodies[index];
        counterText.text = (index + 1) + " / " + Titles.Length;
        nextLabel.text = index < Titles.Length - 1 ? "Next" : "Take the pre-quiz";
    }

    public void Next()
    {
        if (index < Titles.Length - 1) { index++; Refresh(); }
        else quiz.StartPre();
    }

    public void Previous()
    {
        if (index > 0) { index--; Refresh(); }
        else screens.Show("Home");
    }
}