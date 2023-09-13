using Godot;
using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
    private static int PHYSICAL_SIZE => LevelGenerator.PHYSICAL_SIZE;

    public static Vector3 To3D(this Vector2I vector2Int)
    {
        return new Vector3(vector2Int.Y, 0, vector2Int.X) * PHYSICAL_SIZE;
    }

    public static Vector2I To2D(this Vector3 vector3)
    {
        return new Vector2I(Mathf.RoundToInt(vector3.Z / PHYSICAL_SIZE), Mathf.RoundToInt(vector3.X / PHYSICAL_SIZE));
    }

    public static float Distance(this Vector2I origin, Vector2I target)
    {
        return Mathf.Sqrt(Mathf.Pow(origin.X - target.X, 2) + Mathf.Pow(origin.Y - target.Y, 2));
    }

    public static float NextFloat(this Random random, Vector2 range)
    {
        return random.NextFloat(range.X, range.Y);
    }

    public static float NextFloat(this Random random, float minValue, float maxValue)
    {
        return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
    }

    public static float Percent(this Timer timer)
    {
        return (float)(1 - timer.TimeLeft / timer.WaitTime);
    }
}
