using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Pathfinder
{
    private static int[,] map;
    private static int[,] objects;
    private static Vector2I size;

    public static void SetMap(int[,] newMap, Vector2I newSize)
    {
        map = newMap;
        size = newSize;
        objects = new int[newSize.X, newSize.Y];
    }

    public static void PlaceObject(Vector2I pos)
    {
        objects[pos.X, pos.Y]++;
    }

    public static void RemoveObject(Vector2I pos)
    {
        objects[pos.X, pos.Y]--;
    }

    public static void MoveObject(Vector2I oldPos, Vector2I newPos)
    {
        RemoveObject(oldPos);
        PlaceObject(newPos);
    }

    public static float GetTrueDistance(Vector2I start, Vector2I end)
    {
        if (start == end)
        {
            return 0;
        }
        List<Vector2I> parts = GetPath(start, end);
        float sum = 0;
        for (int i = 0; i < parts.Count - 1; i++)
        {
            sum += parts[i].Distance(parts[i + 1]);
        }
        return sum;
    }

    public static bool HasLineOfSight(Vector2I start, Vector2I end)
    {
        return HasLineOfSight(new Point(start), new Point(end));
    }

    public static List<Vector2I> GetPath(Vector2I sourceVec, Vector2I destinationVec)
    {
        // From Wikipedia...
        Point source = new Point(sourceVec);
        Point destination = new Point(destinationVec);
        if (source == destination)
        {
            throw new Exception("Same source & destination!");
        }
        if (!CanMove(destination.x, destination.y))
        {
            throw new Exception("Destination is a blocked tile! (" + destination + ")");
        }

        List<Point> openSet = new List<Point>();
        openSet.Add(source);

        Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();

        Dictionary<Point, int> gScore = new Dictionary<Point, int>();
        gScore.Add(source, 0);

        Dictionary<Point, int> fScore = new Dictionary<Point, int>();
        fScore.Add(source, GetCost(source, destination));
        // Horrible infinte loop fix
        int attempts = 0;
        while (openSet.Count > 0)
        {
            if (attempts++ >= 100)
            {
                break;
            }
            Point current = openSet[0];
            int minValue = int.MaxValue;
            openSet.ForEach(a => { if (fScore.SafeGet(a, int.MaxValue) < minValue) minValue = fScore.SafeGet(current = a, int.MaxValue); });
            if (current == destination)
            {
                return RecoverPath(cameFrom, current);
            }
            openSet.Remove(current);
            foreach (Point neighbor in current.GetNeighbors())
            {
                if (CanMove(neighbor.x, neighbor.y))
                { 
                    int tentativeScore = gScore[current] + GetDistance(current, neighbor); // No safe as the current should always have a gValue
                    if (tentativeScore < gScore.SafeGet(neighbor, int.MaxValue))
                    {
                        cameFrom.AddOrSet(neighbor, current);
                        gScore.AddOrSet(neighbor, tentativeScore);
                        fScore.AddOrSet(neighbor, tentativeScore + GetCost(neighbor, destination));
                        if (openSet.FindIndex(a => a == neighbor) < 0)
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }
        // This should be impossible
        return new List<Vector2I>();
    }

    public static List<Vector2I> GetMoveArea(Vector2I start, int move)
    {
        List<Vector2I> result = new List<Vector2I>();
        result.Add(start);
        if (move <= 0)
        {
            return result;
        }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i != 0 && j == 0) || (j != 0 && i == 0))
                {
                    if (CanMove(start.X + i, start.Y + j))
                    {
                        result.AddRange(GetMoveArea(start + new Vector2I(i, j), move - 1));
                    }
                }
            }
        }
        return result;
    }

    public static List<Vector2I> GetAttackArea(Vector2I start, Vector2I range, int move = 0)
    {
        List<Vector2I> result = new List<Vector2I>();
        if (move >= range.X && move <= range.Y)
        {
            result.Add(start);
        }
        if (move >= range.Y)
        {
            return result;
        }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i != 0 && j == 0) || (j != 0 && i == 0))
                {
                    if (CanMove(start.X + i, start.Y + j, true))
                    {
                        result.AddRange(GetAttackArea(start + new Vector2I(i, j), range, move + 1));
                    }
                }
            }
        }
        return result;
    }

    private static List<Vector2I> RecoverPath(Dictionary<Point, Point> cameFrom, Point current)
    {
        void Squash(List<Point> totalPathArg, int curr, int next)
        {
            for (int i = curr + 1; i < next - 1; i++)
            {
                totalPathArg.RemoveAt(curr + 1);
            }
        }

        List<Point> totalPath = new List<Point>();
        totalPath.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        // Post-process path
        //if (totalPath.Count > 2) // No need to squash if it's just 2 steps...
        //{
        //    int curr = 0, next = 2;
        //    while (next < totalPath.Count)
        //    {
        //        if (!HasLineOfSight(totalPath[curr], totalPath[next]))
        //        {
        //            Squash(totalPath, curr, next);
        //            curr++;
        //            next = curr + 2; // Must have a line of sight with neighbors, so no need to check them
        //        }
        //        else
        //        {
        //            next++;
        //        }
        //    }
        //    Squash(totalPath, curr, next);
        //}
        // Reverse & convert path
        List<Vector2I> reversed = new List<Vector2I>();
        for (int i = totalPath.Count - 1; i >= 0; i--)
        {
            reversed.Add(totalPath[i].ToVector2I());
        }
        return reversed;
    }

    private static int GetCost(Point pos, Point destination)
    {
        return pos.GetDistance(destination);
    }

    private static int GetDistance(Point current, Point neighbor)
    {
        return 1; // No need to calculate, we know it's always 1
    }

    private static bool CanMove(int x, int y, bool ignoreObjects = false)
    {
        if (x < 0 || y < 0 || x >= size.X || y >= size.Y)
        {
            return true;
        }
        return (map[x, y] <= 0 || (ignoreObjects && map[x,y] == 3)) && (ignoreObjects || objects[x, y] <= 0);
    }

    private static bool HasLineOfSight(Point start, Point end)
    {
        // Adapted from https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        int x = start.x, y = start.y;
        int x2 = end.x, y2 = end.y;
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            if (!CanMove(x, y))
            {
                return false;
            }
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                if (i < longest && (!CanMove(x + dx1, y) || !CanMove(x, y + dy1)))
                {
                    //Debug.Log("No line of sight between " + new Vector2I(x, y) + " and " + new Vector2I(x + dx1, y + dy1) + " - checking " + start + " to " + end);
                    return false;
                }
                x += dx1;
                y += dy1;
            }
            else
            {
                if (i < longest && (!CanMove(x + dx2, y) || !CanMove(x, y + dy2)))
                {
                    //Debug.Log("No line of sight between " + new Vector2I(x, y) + " and " + new Vector2I(x + dx2, y + dy2) + " - checking " + start + " to " + end);
                    return false;
                }
                x += dx2;
                y += dy2;
            }
        }
        return true;
    }

    private class Point
    {
        public int x;
        public int y;

        public static bool operator ==(Point a, Point b)
        {
            if ((object)a == null) return (object)b == null;
            if ((object)b == null) return (object)a == null;
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public Point(Vector2I vector2Int)
        {
            x = vector2Int.X;
            y = vector2Int.Y;
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public List<Point> GetNeighbors()
        {
            List<Point> result = new List<Point>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((i == 0 || j == 0) && (i != 0 || j != 0))
                    {
                        result.Add(new Point(x + i, y + j));
                    }
                }
            }
            return result;
        }

        public int GetDistance(Point other)
        {
            return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(other.x - x, 2) + Mathf.Pow(other.y + y, 2))); // Mathf.Abs(other.x - x) + Mathf.Abs(other.y - y);
        }

        public Vector2I ToVector2I()
        {
            return new Vector2I(x, y);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
