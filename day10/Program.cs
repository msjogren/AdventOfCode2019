using System;
using System.Drawing;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var asteroidCoords = File.ReadAllLines("input.txt")
            .SelectMany((line, y) =>
                line
                .Select((c, x) => (c, x))
                .Where(cx => cx.c == '#')
                .Select((cx, x) => new Point(cx.x, y)));

        // Part 1
        var best = asteroidCoords
            .Select(pt => {
                int seen = asteroidCoords
                    .Except(new[] {pt})
                    .OrderBy(pt2 => Distance(pt, pt2))
                    .GroupBy(pt2 => Math.Atan2(pt.Y - pt2.Y, pt2.X - pt.X))
                    .Count();
                return (pt, seen);
            })
            .OrderByDescending(a => a.seen)
            .First();
            
        Console.WriteLine($"Part 1: {best.seen} @ {best.pt.X},{best.pt.Y}");
 
        // Part 2
        var vaporized200 = asteroidCoords
            .Except(new[] {best.pt})
            .Select(pt => (
                pt: pt, 
                // Transform angle from what Atan2 returns (-π...π) to what we want (0...2π, 0 pointing up, clockwise)
                angle: (2*Math.PI + Math.PI/2 - Math.Atan2(best.pt.Y - pt.Y, pt.X - best.pt.X)) % (2*Math.PI), 
                dist: Distance(pt, best.pt)
            ))
            .OrderBy(a => a.angle)
            .ThenBy(a => a.dist)
            .GroupBy(a => a.angle, a => a)
            .SelectMany(grp => grp.Select((a, i) => (
                pt: a.pt,
                angle: a.angle,
                lap: i
            )))
            .OrderBy(a => a.lap)
            .ThenBy(a => a.angle)
            .Skip(199)
            .First();

        Console.WriteLine($"Part 2: {vaporized200.pt.X * 100 + vaporized200.pt.Y}");
    }

    static double Distance(Point p1, Point p2) => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
}
