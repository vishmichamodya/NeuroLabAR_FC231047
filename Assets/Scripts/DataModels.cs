using System;

[Serializable]
public class Question
{
    public string level;
    public string text;
    public string[] options;
    public int correct;
    public string explanation;
}

[Serializable] public class QuizFile { public Question[] questions; }

[Serializable]
public class LessonStep
{
    public string narration;
    public int[] codeLines;
    public int highlightLayer = -1;   // 0 input, 1 hidden, 2 output, -1 none
    public int pulseLayer = -1;       // 0 = input->hidden connections, 1 = hidden->output
    public int widgetMode;            // 0 sigmoid, 1 relu (Activation lesson)
    public bool showTarget;           // show the target flag
    public bool showGradients;        // show the backprop arrows
}

[Serializable]
public class Lesson
{
    public string title;
    public string module;
    public string widget;             // None, Inputs, Activation, Loss, Training
    public string[] code;
    public LessonStep[] steps;
}

[Serializable] public class LessonFile { public Lesson[] lessons; }