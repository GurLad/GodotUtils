using Godot;
using System;

public class Vector2ISerializable
{
    public int X { get; set; }
    public int Y { get; set; }

    public Vector2ISerializable(Vector2I vector2I)
    {
        X = vector2I.X;
        Y = vector2I.Y;
    }

    [System.Text.Json.Serialization.JsonConstructor]
    public Vector2ISerializable(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2I ToV2I() => new Vector2I(X, Y);
}
