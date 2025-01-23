using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class ExtensionMethods
{
    private static int PHYSICAL_SIZE => LevelGenerator.PHYSICAL_SIZE;
    //private static readonly Random rng = new Random();

    // Math

    public static Vector3 To3D(this Vector2I vector2Int)
    {
        return new Vector3(vector2Int.Y, 0, vector2Int.X) * PHYSICAL_SIZE;
    }

    public static Vector2I To2D(this Vector3 vector3)
    {
        return new Vector2I(Mathf.RoundToInt(vector3.Z / PHYSICAL_SIZE), Mathf.RoundToInt(vector3.X / PHYSICAL_SIZE));
    }

    public static Vector2I ToV2I(this Vector2 vector2)
    {
        return new Vector2I(Mathf.RoundToInt(vector2.X), Mathf.RoundToInt(vector2.Y));
    }

    public static float Distance(this Vector2I origin, Vector2I target)
    {
        return Mathf.Sqrt(Mathf.Pow(origin.X - target.X, 2) + Mathf.Pow(origin.Y - target.Y, 2));
    }

    public static int Dot(this Vector2I a, Vector2I b) => a.X * b.X + a.Y * b.Y;

    // Modified from https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line
    public static Vector2I GetClosestPointOnLine(this Vector2I point, Vector2I lineStart, Vector2I lineEnd)
    {
        Vector2I ap = point - lineStart;
        Vector2I ab = lineEnd - lineStart;
        float t = Mathf.Clamp((float)ap.Dot(ab) / ab.LengthSquared(), 0, 1);
        return lineStart + new Vector2I(Mathf.RoundToInt(ab.X * t), Mathf.RoundToInt(ab.Y * t));
    }

    // Random

    public static float NextFloat(this Random random, Vector2 range)
    {
        return random.NextFloat(range.X, range.Y);
    }

    public static float NextFloat(this Random random, float minValue, float maxValue)
    {
        return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
    }

    // public static T RandomItemInList<T>(this List<T> list)
    // {
    //     return list.Count > 0 ? list[rng.Next(0, list.Count)] : default;
    // }

    // public static T RandomItemInList<T>(this T[] list)
    // {
    //     return list.Length > 0 ? list[rng.Next(0, list.Length)] : default;
    // }

    // public static List<T> Shuffle<T>(this List<T> list)
    // {
    //     List<T> temp = list.FindAll(a => true);
    //     list = new List<T>();
    //     while (temp.Count > 0)
    //     {
    //         int i = rng.Next(0, temp.Count);
    //         list.Add(temp[i]);
    //         temp.RemoveAt(i);
    //     }
    //     return list;
    // }

    // Timers

    public static float Percent(this Timer timer)
    {
        return (float)(1 - timer.TimeLeft / timer.WaitTime);
    }

    public static float SinTime(float rate)
    {
        return (Mathf.Sin((Time.GetTicksMsec() / 1000f) * rate * Mathf.Pi) + 1) / 2;
    }

    // Json

    public static string ToJson<T>(this T obj, bool prettyPrint = true)
    {
        return JsonSerializer.Serialize(obj, typeof(T), new JsonSerializerOptions { WriteIndented = prettyPrint });
    }

    public static T JsonToObject<T>(this string jsonContent)
    {
        return (T)JsonSerializer.Deserialize(jsonContent, typeof(T));
    }

    public static Vector2ISerializable Serializable(this Vector2I vector2I) => new Vector2ISerializable(vector2I);

    // Strings

    public static string FixFileName(this string str)
    {
        return str.Replace("\"", "").Replace("\\", "").Replace("/", "").Replace(":", "").Replace("?", "").Replace("|", "").Replace("*", "").Replace("<", "").Replace(">", "");
    }

    public static string FindLineBreaks(this string line, int lineWidth)
    {
        string cutLine = line;
        for (int i = line.IndexOf(' '); i > -1; i = cutLine.IndexOf(' ', i + 1))
        {
            int nextLength = cutLine.Substring(i + 1).Split(' ')[0].Length;
            int length = i + 1 + nextLength;
            if (length > lineWidth)
            {
                line = line.Substring(0, line.LastIndexOf('\n') + 1) + cutLine.Substring(0, i) + '\n' + cutLine.Substring(i + 1);
                i = 0;
                cutLine = line.Substring(line.LastIndexOf('\n') + 1);
            }
        }
        // Fix too long words
        int prev = 0;
        //GD.Print("Init: " + '"' + line + '"');
        for (int i = line.IndexOf('\n'); ; i = line.IndexOf('\n', i + 1))
        {
            string currentLine = line.Substring(prev, (i > -1 ? i : line.Length) - prev);
            //GD.Print('"' + currentLine + '"');
            if (currentLine.Length > lineWidth)
            {
                line = line.Substring(0, prev) +
                    currentLine.Substring(0, lineWidth) + "\n" +
                    currentLine.Substring(lineWidth, currentLine.Length - lineWidth) +
                    line.Substring(i > -1 ? i : line.Length);
                //GD.Print("Fixed: " + '"' + line + '"');
            }
            prev = i + 1;
            if (i <= -1)
            {
                break;
            }
        }
        return line;
    }

    public static List<string> ToLineBrokenList(this string rawData)
    {
        List<string> entries = rawData.Trim().Replace("\r", "").Split('\n').ToList().ConvertAll(a => a.Trim());
        entries.RemoveAll(a => string.IsNullOrEmpty(a));
        return entries;
    }

    public static bool BeginsWith(this string source, string value) => source.StartsWith(value);

    public static bool BeginsWith(this string source, char value) => source.StartsWith(value);

    // List extensions

    public static T Find<T>(this List<T> list, Func<T, int, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i], i))
            {
                return list[i];
            }
        }
        return default;
    }

    public static List<T> FindAll<T>(this List<T> list, Func<T, int, bool> predicate)
    {
        List<T> result = new List<T>();
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i], i))
            {
                result.Add(list[i]);
            }
        }
        return result;
    }

    public static int FindIndex<T>(this List<T> list, Func<T, int, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i], i))
            {
                return i;
            }
        }
        return -1;
    }

    public static void ForEach<T>(this List<T> list, Action<T, int> action)
    {
        for (int i = 0; i < list.Count; i++)
        {
            action(list[i], i);
        }
    }

    // Dictionary extensions
	
    public static S SafeGet<T, S>(this Dictionary<T, S> dictionary, T key, S defaultValue = default)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }

    public static S AddOrSet<T, S>(this Dictionary<T, S> dictionary, T key, S value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
        else
        {
            dictionary[key] = value;
        }
        return value;
    }

    public static void ForEach<Key, Value>(this Dictionary<Key, Value> dictionary, Action<Key, Value> action)
    {
        dictionary.Keys.ToList().ForEach(a => action(a, dictionary[a]));
    }

    public static Dictionary<NewKey, NewValue> ConvertAll<OldKey, OldValue, NewKey, NewValue>(this Dictionary<OldKey, OldValue> dictionary, Func<OldKey, OldValue, (NewKey, NewValue)> predicate)
    {
        Dictionary<NewKey, NewValue> result = new Dictionary<NewKey, NewValue>();
        dictionary.ForEach((key, value) =>
        {
            (NewKey key, NewValue value) newItem = predicate(key, value);
            result.Add(newItem.key, newItem.value);
        });
        return result;
    }
}
