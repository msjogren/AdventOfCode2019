using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string[] paths = File.ReadAllLines("input.txt");
        var firstPathPoints = TracePathPoints(paths[0]);
        var secondPathPoints = TracePathPoints(paths[1]);

        var part1Distance = firstPathPoints.Keys
            .Intersect(secondPathPoints.Keys)
            .Select(ManhattanDistance)
            .OrderBy(dist => dist)
            .First();

        Console.WriteLine(part1Distance);

        var part2Steps = firstPathPoints
            .Where(kvp => secondPathPoints.ContainsKey(kvp.Key))
            .Select(kvp => kvp.Value + secondPathPoints[kvp.Key])
            .OrderBy(steps => steps)
            .First();

        Console.WriteLine(part2Steps);
    }

    static int ManhattanDistance(Point point) => Math.Abs(point.X) + Math.Abs(point.Y);

    static Dictionary<Point, int> TracePathPoints(string path)
    {
        var current = Point.Empty;
        var visited = new Dictionary<Point, int>();
        var moves = path.Split(',').Select(move => (move[0], int.Parse(move.Substring(1))));
        var totalSteps = 0;

        foreach ((char direction, int steps) in moves) {
            for (int i = 0; i < steps; i++) {
                current = direction switch {
                    'U' => new Point(current.X, current.Y - 1),
                    'D' => new Point(current.X, current.Y + 1),
                    'L' => new Point(current.X - 1, current.Y),
                    'R' => new Point(current.X + 1, current.Y),
                    _ => throw new ArgumentException("path")
                };
                visited.TryAdd(current, ++totalSteps);
            }
        }

        return visited;
    }
}
