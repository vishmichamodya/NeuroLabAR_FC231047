using System.Text;
using TMPro;
using UnityEngine;

public class StepEngine : MonoBehaviour
{
    public TextMeshProUGUI titleText, counterText, codeText, subtitleText, nextLabel;
    public WidgetController widgets;
    public NarrationPlayer narration;
    public QuizManager quiz;

    Lesson[] lessons;
    int lesson, step;
    NetworkVisualizer viz;

    void Awake()
    {
        TextAsset json = Resources.Load<TextAsset>("lessons");
        lessons = JsonUtility.FromJson<LessonFile>(json.text).lessons;
    }

    void OnEnable()
    {
        ARPlacement.Placed += OnPlaced;
        ScreenManager.Changed += OnScreen;
    }

    void OnDisable()
    {
        ARPlacement.Placed -= OnPlaced;
        ScreenManager.Changed -= OnScreen;
    }

    void OnPlaced(NetworkVisualizer v) { viz = v; Apply(); }

    void OnScreen(string screenName)
    {
        if (screenName == "Lab") Apply();
        else narration.Stop();
    }

    public void Next()
    {
        Lesson L = lessons[lesson];
        if (step < L.steps.Length - 1) step++;
        else if (lesson < lessons.Length - 1) { lesson++; step = 0; }
        else { lesson = 0; step = 0; quiz.StartPost(); return; }   // finished -> post-quiz
        Apply();
    }

    public void Previous()
    {
        if (step > 0) step--;
        else if (lesson > 0) { lesson--; step = lessons[lesson].steps.Length - 1; }
        Apply();
    }

    void Apply()
    {
        if (ScreenManager.Current != "Lab") return;
        Lesson L = lessons[lesson];
        LessonStep S = L.steps[step];

        titleText.text = (lesson + 1) + "/" + lessons.Length + "  " + L.title;
        counterText.text = "Step " + (step + 1) + " of " + L.steps.Length;
        codeText.text = BuildCode(L, S);
        subtitleText.text = S.narration;
        bool last = lesson == lessons.Length - 1 && step == L.steps.Length - 1;
        nextLabel.text = last ? "Post-quiz" : "Next";

        if (viz != null)
        {
            viz.highlightLayer = S.highlightLayer;
            viz.pulseLayer = S.pulseLayer;
            viz.SetExtras(S.showTarget, S.showGradients);
            viz.Refresh();
        }
        widgets.Show(L.widget, S.widgetMode, viz);

        AudioClip clip = Resources.Load<AudioClip>("Audio/L" + (lesson + 1) + "_S" + (step + 1));
        narration.Play(S.narration, clip);
    }

    // builds the code panel text; the current line gets a coloured highlight
    string BuildCode(Lesson L, LessonStep S)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < L.code.Length; i++)
        {
            bool hl = S.codeLines != null && System.Array.IndexOf(S.codeLines, i) >= 0;
            sb.Append(hl ? "<mark=#2DE2E655><color=#FFFFFF>" : "<color=#8B97C9>");
            sb.Append(L.code[i]);
            sb.Append(hl ? "</color></mark>" : "</color>");
            sb.Append('\n');
        }
        return sb.ToString();
    }
}