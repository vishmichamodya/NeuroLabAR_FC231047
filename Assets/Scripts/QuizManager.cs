using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public ScreenManager screens;
    public TextMeshProUGUI titleText, progressText, levelText, questionText, feedbackText, nextLabel;
    public Button[] options;                  // 4 option buttons
    public GameObject feedbackPanel, nextButton;
    public Image progressFill;                // Image Type = Filled, Horizontal
    public int questionsPerQuiz = 10;

    public Color normalColor   = new Color(0.07f, 0.10f, 0.23f, 1f);
    public Color selectedColor = new Color(0.18f, 0.40f, 0.55f, 1f);
    public Color correctColor  = new Color(0.30f, 0.60f, 0.20f, 1f);
    public Color wrongColor    = new Color(0.70f, 0.15f, 0.40f, 1f);

    List<Question> qs;
    string kind;
    int index, score;
    bool answered;

    public void StartPre()  { StartQuiz("pre"); }
    public void StartPost() { StartQuiz("post"); }

    void StartQuiz(string k)
    {
        kind = k;
        TextAsset json = Resources.Load<TextAsset>(k == "pre" ? "pre_quiz" : "post_quiz");
        qs = new List<Question>(JsonUtility.FromJson<QuizFile>(json.text).questions);
        for (int i = qs.Count - 1; i > 0; i--)            // shuffle
        {
            int j = Random.Range(0, i + 1);
            Question tmp = qs[i]; qs[i] = qs[j]; qs[j] = tmp;
        }
        if (qs.Count > questionsPerQuiz) qs.RemoveRange(questionsPerQuiz, qs.Count - questionsPerQuiz);
        index = 0; score = 0;
        titleText.text = (k == "pre") ? "Pre-quiz" : "Post-quiz";
        screens.Show("Quiz");
        ShowQuestion();
    }

    void ShowQuestion()
    {
        answered = false;
        Question q = qs[index];
        progressText.text = "Question " + (index + 1) + " of " + qs.Count;
        progressFill.fillAmount = (float)index / qs.Count;
        levelText.text = q.level;
        questionText.text = q.text;
        for (int i = 0; i < options.Length; i++)
        {
            bool has = i < q.options.Length;
            options[i].gameObject.SetActive(has);
            if (!has) continue;
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = q.options[i];
            options[i].image.color = normalColor;
        }
        feedbackPanel.SetActive(false);
        nextButton.SetActive(false);
    }

    public void Pick(int i)                  // each option button calls this with its own number
    {
        if (answered) return;
        answered = true;
        Question q = qs[index];
        bool ok = (i == q.correct);
        if (ok) score++;

        if (kind == "post")
        {
            options[q.correct].image.color = correctColor;
            if (!ok) options[i].image.color = wrongColor;
            feedbackPanel.SetActive(true);
            feedbackText.text = (ok ? "Correct! " : "Not quite. ") + q.explanation;
        }
        else options[i].image.color = selectedColor;      // pre-quiz: no answers shown

        bool last = index == qs.Count - 1;
        nextLabel.text = last ? (kind == "pre" ? "Start the AR lab" : "See my results") : "Next";
        nextButton.SetActive(true);
    }

    public void Next()                       // Next button
    {
        if (!answered) return;
        if (index < qs.Count - 1) { index++; ShowQuestion(); }
        else Finish();
    }

    void Finish()
    {
        PlayerPrefs.SetInt(kind + "_score", score);
        PlayerPrefs.SetInt(kind + "_total", qs.Count);
        PlayerPrefs.Save();
        if (kind == "post") LogCsv();
        screens.Show(kind == "pre" ? "Warning" : "Results");
    }

    // one anonymous line per tester: tester number, pre, post, time
    static void LogCsv()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "results.csv");
            if (!File.Exists(path)) File.WriteAllText(path, "tester,pre,post,time\n");
            string line = PlayerPrefs.GetInt("tester_no", 1) + "," + PlayerPrefs.GetInt("pre_score", -1) + ","
                        + PlayerPrefs.GetInt("post_score", -1) + "," + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\n";
            File.AppendAllText(path, line);
        }
        catch (System.Exception e) { Debug.LogWarning(e.Message); }
    }
}