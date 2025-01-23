using Godot;
using System;

public static class Easing
{
    public enum Type
    {
        StartMaker = -1,
        None = -1,
        EaseOutBack = 0,
        EaseInBack,
        EaseOutQuad,
        EaseInQuad,
        EaseInOutQuad,
        EaseOutQuart,
        EaseInQuart,
        EaseInOutSin,
        EaseOutElastic,
        EndMarker
    }

    public static float Ease(Type type, float x)
    {
        return type switch
        {
            Type.None           => x,
            Type.EaseOutBack    => EaseOutBack   (x),
            Type.EaseInBack     => EaseInBack    (x),
            Type.EaseOutQuad    => EaseOutQuad   (x),
            Type.EaseInQuad     => EaseInQuad    (x),
            Type.EaseInOutQuad  => EaseInOutQuad (x),
            Type.EaseOutQuart   => EaseOutQuart  (x),
            Type.EaseInQuart    => EaseInQuart   (x),
            Type.EaseInOutSin   => EaseInOutSin  (x),
            Type.EaseOutElastic => EaseOutElastic(x),
            Type.EndMarker => throw new Exception("Missing easing type!"),
            _ => throw new Exception("Missing easing type!")
        };
    }

    // From https://easings.net
    public static float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    public static float EaseInBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return (c3 * x * x * x - c1 * x * x);
    }

    public static float EaseInOutBack(float x)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return x < 0.5
          ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
          : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }

    public static float EaseOutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }

    public static float EaseInQuad(float x)
    {
        return x * x;
    }

    public static float EaseInOutQuad(float x)
    {
        return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
    }

    public static float EaseOutQuart(float x)
    {
        return 1 - Mathf.Pow(1 - x, 4);
    }

    public static float EaseInQuart(float x)
    {
        return x * x * x * x;
    }

    public static float EaseInOutSin(float x)
    {
        return -(Mathf.Cos(Mathf.Pi * x) - 1) / 2;
    }

    public static float EaseOutElastic(float x)
    {
        float c4 = (2 * Mathf.Pi) / 3;

        return x == 0 ? 0 : x == 1 ? 1 : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }

    public static float EaseOutSin(float x)
    {
        return Mathf.Sin((x * Mathf.Pi) / 2);
    }

    public static float EaseInSin(float x)
    {
        return 1 - Mathf.Cos((x * Mathf.Pi) / 2);
    }
}
