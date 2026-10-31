using UnityEngine;

public class NeuralNet
{
    public float[,] w1 = new float[3, 2];   // weights input -> hidden
    public float[] b1 = new float[3];       // hidden biases
    public float[] w2 = new float[3];       // weights hidden -> output
    public float b2;                        // output bias
    public float learningRate = 2f;

    public struct Result { public float[] z1; public float[] h; public float z2; public float y; }

    // the XOR puzzle: output 1 only when exactly one input is 1
    public static readonly float[,] X = { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } };
    public static readonly float[] T = { 0, 1, 1, 0 };

    public NeuralNet() { LoadDemo(); }

    public static float Sigmoid(float z) { return 1f / (1f + Mathf.Exp(-z)); }

    public void LoadDemo()
    {
        w1[0, 0] = 1.6f;  w1[0, 1] = -1.2f;
        w1[1, 0] = -1.4f; w1[1, 1] = 1.7f;
        w1[2, 0] = 0.9f;  w1[2, 1] = 1.1f;
        b1[0] = 0.2f; b1[1] = -0.3f; b1[2] = 0.1f;
        w2[0] = 1.5f; w2[1] = -1.3f; w2[2] = 1.1f;
        b2 = -0.2f;
    }

    public void Randomize()
    {
        for (int i = 0; i < 3; i++)
        {
            w1[i, 0] = Random.Range(-2f, 2f);
            w1[i, 1] = Random.Range(-2f, 2f);
            b1[i] = Random.Range(-1f, 1f);
            w2[i] = Random.Range(-2f, 2f);
        }
        b2 = Random.Range(-1f, 1f);
    }

    // forward pass: inputs -> hidden -> output
    public Result Forward(float x0, float x1)
    {
        Result r = new Result { z1 = new float[3], h = new float[3] };
        float z2 = b2;
        for (int i = 0; i < 3; i++)
        {
            r.z1[i] = w1[i, 0] * x0 + w1[i, 1] * x1 + b1[i];   // weighted sum + bias
            r.h[i] = Sigmoid(r.z1[i]);                         // activation
            z2 += w2[i] * r.h[i];
        }
        r.z2 = z2;
        r.y = Sigmoid(z2);
        return r;
    }

    // average squared error over the 4 XOR examples
    public float Loss()
    {
        float sum = 0f;
        for (int k = 0; k < 4; k++)
        {
            float e = T[k] - Forward(X[k, 0], X[k, 1]).y;
            sum += e * e;
        }
        return sum / 4f;
    }

    // backpropagation + gradient descent
    public void Train(int epochs)
    {
        for (int e = 0; e < epochs; e++)
        {
            for (int k = 0; k < 4; k++)
            {
                Result f = Forward(X[k, 0], X[k, 1]);
                float dy = (f.y - T[k]) * f.y * (1f - f.y);          // output error signal
                for (int i = 0; i < 3; i++)
                {
                    float dh = dy * w2[i] * f.h[i] * (1f - f.h[i]); // hidden error signal
                    w2[i] -= learningRate * dy * f.h[i];
                    w1[i, 0] -= learningRate * dh * X[k, 0];
                    w1[i, 1] -= learningRate * dh * X[k, 1];
                    b1[i] -= learningRate * dh;
                }
                b2 -= learningRate * dy;
            }
        }
    }
}