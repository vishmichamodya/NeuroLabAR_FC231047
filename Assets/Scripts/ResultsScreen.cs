using TMPro;
using UnityEngine;

public class ResultsScreen : MonoBehaviour
{
    public ScreenManager screens;
    public TextMeshProUGUI headline, details, badge, preBarLabel, postBarLabel;
    public RectTransform preBar, postBar;     // bars: anchor/pivot at the bottom
    public float maxBarHeight = 520f;

    void OnEnable()  { ScreenManager.Changed += OnScreen; }
    void OnDisable() { ScreenManager.Changed -= OnScreen; }
    void OnScreen(string s) { if (s == "Results") Refresh(); }

    void Refresh()
    {
        int pre = PlayerPrefs.GetInt("pre_score", -1), preT = PlayerPrefs.GetInt("pre_total", 10);
        int post = PlayerPrefs.GetInt("post_score", -1), postT = PlayerPrefs.GetInt("post_total", 10);
        SetBar(preBar, preBarLabel, pre, preT);
        SetBar(postBar, postBarLabel, post, postT);

        if (pre < 0)
        {
            headline.text = "No results yet";
            details.text = "Take the pre-quiz, then the lab, then the post-quiz.";
            badge.text = ""; return;
        }
        if (post < 0)
        {
            headline.text = "Pre-quiz: " + pre + "/" + preT;
            details.text = "Finish the lab, then take the post-quiz to see your learning gain.";
            badge.text = ""; return;
        }

        float prePct = 100f * pre / preT, postPct = 100f * post / postT;
        float improvement = prePct > 0f ? (postPct - prePct) / prePct * 100f : (postPct > 0f ? 100f : 0f);
        bool met = improvement >= 20f;

        headline.text = met ? "Great learning gain!" : (postPct > prePct ? "You improved" : "Keep practising");
        details.text = "Pre-quiz: " + pre + "/" + preT + "     Post-quiz: " + post + "/" + postT + "\n"
                     + "Change: " + (postPct - prePct).ToString("+0;-0;0") + " percentage points  ("
                     + improvement.ToString("+0;-0;0") + "% vs pre-quiz)";
        badge.text = met ? "Target met: gain of 20% or more" : "Target: gain of 20% or more";
    }

    void SetBar(RectTransform bar, TextMeshProUGUI label, int score, int total)
    {
        if (score < 0) { bar.sizeDelta = new Vector2(bar.sizeDelta.x, 0f); label.text = "-"; return; }
        bar.sizeDelta = new Vector2(bar.sizeDelta.x, maxBarHeight * score / Mathf.Max(1, total));
        label.text = score + "/" + total;
    }

    public void NewTester()                 // button: clears scores for the next student
    {
        PlayerPrefs.DeleteKey("pre_score"); PlayerPrefs.DeleteKey("post_score");
        PlayerPrefs.SetInt("tester_no", PlayerPrefs.GetInt("tester_no", 1) + 1);
        PlayerPrefs.Save();
        screens.Show("Home");
    }
}