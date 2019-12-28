using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static char[,] map;
    static int mapWidth, mapHeight;
    static Dictionary<(int x, int y), (int x, int y, int dz)> portals = new Dictionary<(int, int), (int, int, int)>();
    
    static void Main(string[] args)
    {
        const bool Part2 = true;
        string[] lines = File.ReadAllLines("input.txt");

        var halfPortals = new Dictionary<string, (int x, int y)>();
        void HandlePortalEnd(int x, int y, string name)
        {
            if (halfPortals.TryGetValue(name, out (int x, int y) other)) {
                bool IsOuter((int x, int y) pt) {
                    return pt.x == 0 || pt.y == 0 || pt.x == mapWidth - 1 || pt.y == mapHeight - 1;
                }
                portals.Add(other, (x, y, Part2 ? (IsOuter(other) ? -1 : 1) : 0));
                portals.Add((x, y), (other.x, other.y, Part2 ? (IsOuter((x, y)) ? -1 : 1) : 0));
                halfPortals.Remove(name);
            } else {
                halfPortals.Add(name, (x, y));
            }
        }

        mapWidth = lines[0].Length - 4;
        mapHeight = lines.Length - 4;
        map = new char[mapWidth, mapHeight];
        for (int y = 2, mapy = 0; y < lines.Length - 2; y++, mapy++) {
            for (int x = 2, mapx = 0; x < lines[y].Length - 2; x++, mapx++) {
                switch ((lines[y][x])) {
                    case '#': map[mapx, mapy] = '#'; break;
                    case '.': 
                        map[mapx, mapy] = '.';
                        if (Char.IsLetter(lines[y][x - 1])) {
                            HandlePortalEnd(mapx, mapy, lines[y].Substring(x - 2, 2));
                        } else if (Char.IsLetter(lines[y][x + 1])) {
                            HandlePortalEnd(mapx, mapy, lines[y].Substring(x + 1, 2));
                        } else if (Char.IsLetter(lines[y-1][x])) {
                            HandlePortalEnd(mapx, mapy, new string(new[] {lines[y-2][x], lines[y-1][x] }));
                        } else if (Char.IsLetter(lines[y+1][x])) {
                            HandlePortalEnd(mapx, mapy, new string(new[] {lines[y+1][x], lines[y+2][x] }));
                        }
                        break;
                    default: map[mapx, mapy] = ' '; break;
                }
            }
        }

        (int x, int y) startPos = halfPortals["AA"], endPos = halfPortals["ZZ"];

        /*
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                char ch = map[x, y];
                if ((x, y) == startPos) {
                    ch = 'S';
                } else if ((x, y) == endPos) {
                    ch = 'E';
                }
                Console.Write(ch);
            }
            Console.WriteLine();
        }
        */

        Console.WriteLine($"Part {(Part2 ? 2 : 1)}: " + ShortestPathTo((endPos.x, endPos.y, 0), (startPos.x, startPos.y, 0), new HashSet<(int x, int y, int z)>()));            
    }

    static int ShortestPathTo((int x, int y, int z) to, (int x, int y, int z) current, HashSet<(int x, int y, int z)> visited)
    {
        var startedFrom = current;
        var previous = current;
        int steps = 0;
        visited.Add(current);
        if (current == to) {
            return steps;
        }

        while (true) {
            var allAdjacent = new[] {(dx: 1, dy: 0), (dx: -1, dy: 0), (dx: 0, dy: 1), (dx: 0, dy: -1)}
                .Select(offset => (x: current.x + offset.dx, y: current.y + offset.dy, z: current.z))
                .Where(pt => pt.x >= 0 && pt.x < mapWidth && pt.y >= 0 && pt.y < mapHeight)
                .Where(pt => map[pt.x, pt.y] == '.');
            var adjacentToVisit = allAdjacent.Where(pt => !visited.Contains(pt));

            if (portals.TryGetValue((current.x, current.y), out var portalTo) && 
                (current.z + portalTo.dz) >= 0 &&
                (current.z + portalTo.dz <= 30) &&      // To avoid endless depth recursion
                !visited.Contains((portalTo.x, portalTo.y, current.z + portalTo.dz))) {
                adjacentToVisit = adjacentToVisit.Prepend((x: portalTo.x, y: portalTo.y, z: current.z + portalTo.dz));
            }
            if (!adjacentToVisit.Any()) {
                if (allAdjacent.Count(pt => pt != previous) == 0 && 
                    (current.x != to.x || current.y != to.y) && 
                    !portals.ContainsKey((current.x, current.y))) {

                    // Dead end. Seal off to not waste time here again.
                    map[startedFrom.x, startedFrom.y] = '#';
                }
                return -1;
            } else if (adjacentToVisit.Count() == 1) {
                steps++;
                previous = current;
                current = adjacentToVisit.First();
                visited.Add(current);
                if (current == to) {
                    return steps;
                }
            } else {                
                var subPaths = adjacentToVisit
                    .Select(pt => ShortestPathTo(to, pt, new HashSet<(int x, int y, int z)>(visited)))
                    .Where(pathLength => pathLength >= 0)
                    .ToArray();
                if (subPaths.Any()) {
                    steps++;
                    return steps + subPaths.Min();
                } else {
                    return -1;
                }
            }
        }
    }
}
