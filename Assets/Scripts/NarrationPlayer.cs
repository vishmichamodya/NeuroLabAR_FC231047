using TMPro;
using UnityEngine;

public class NarrationPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public TextMeshProUGUI muteLabel;
    public bool muted;

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject tts;
    bool ttsReady, langSet;

    class InitListener : AndroidJavaProxy
    {
        readonly System.Action<int> cb;
        public InitListener(System.Action<int> c) : base("android.speech.tts.TextToSpeech$OnInitListener") { cb = c; }
        public void onInit(int status) { cb(status); }
    }
#endif

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener(s => ttsReady = (s == 0)));
            }
        }
        catch (System.Exception e) { Debug.LogWarning("Text-to-speech not available: " + e.Message); }
#endif
        UpdateLabel();
    }

    public void Play(string text, AudioClip clip)
    {
        Stop();
        if (muted) return;
        if (clip != null) { audioSource.clip = clip; audioSource.Play(); }
        else Speak(text);
    }

    void Speak(string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (tts == null || !ttsReady) return;
        try
        {
            if (!langSet)
            {
                using (AndroidJavaClass loc = new AndroidJavaClass("java.util.Locale"))
                    tts.Call<int>("setLanguage", loc.GetStatic<AndroidJavaObject>("US"));
                langSet = true;
            }
            tts.Call<int>("speak", text, 0, null, "neurolab");     // 0 = replace anything already speaking
        }
        catch (System.Exception e) { Debug.LogWarning(e.Message); }
#else
        Debug.Log("[Narration] " + text);                          // in the Editor we just print it
#endif
    }

    public void Stop()
    {
        if (audioSource != null) audioSource.Stop();
#if UNITY_ANDROID && !UNITY_EDITOR
        try { if (tts != null && ttsReady) tts.Call<int>("stop"); } catch { }
#endif
    }

    public void ToggleMute()      // Mute button
    {
        muted = !muted;
        if (muted) Stop();
        UpdateLabel();
    }

    void UpdateLabel() { if (muteLabel != null) muteLabel.text = muted ? "Sound off" : "Sound on"; }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { if (tts != null) tts.Call("shutdown"); } catch { }
#endif
    }
}