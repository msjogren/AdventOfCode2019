using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    enum NodeType { Unknown, Empty, Wall }
    enum Move { North = 1, South, West, East }

    static Dictionary<(int x, int y), NodeType> map = new Dictionary<(int x, int y), NodeType>();
    static Stack<(int x, int y, Move move)> moveStack = new Stack<(int x, int y, Move move)>();

    static NodeType GetNodeType((int x, int y) pos) => map.TryGetValue(pos, out NodeType type) ? type : NodeType.Unknown;

    static void Main(string[] args)
    {
        long[] code = File.ReadAllText("input.txt").Split(',').Select(long.Parse).ToArray();
        long[] memory = new long[1_000_000];
        Array.Copy(code, memory, code.Length);

        var invalidPos = (x: -1, y: -1);        // Assumes area is small enough to not go into negative coordinates
        var currentPos = (x: 100, y: 100);
        var nextPos = invalidPos;
        var nextMove = Move.North;
        var oxygenPos = invalidPos;

        // Start DFS to map the area
        MovedTo(currentPos, null);

        RunProgram(memory, () => {
            if (moveStack.TryPop(out (int x, int y, Move move) result)) {
                nextPos = (result.x, result.y);
                nextMove = result.move;
                return (int)nextMove;
            } else {
                DrawMap(currentPos, oxygenPos);
                Console.WriteLine("Part 1: " + BreadthFirstSearch(currentPos, oxygenPos));
                Console.WriteLine("Part 2: " + BreadthFirstSearch(oxygenPos, invalidPos));
                return 0;
            }
        }, i => {
            switch (i) {
                case 0:
                    map[nextPos] = NodeType.Wall;
                    break;
                case 1:
                    currentPos = nextPos;
                    MovedTo(currentPos, nextMove);
                    break;
                case 2:
                    oxygenPos = nextPos;
                    currentPos = nextPos;
                    MovedTo(currentPos, nextMove);
                    break;
            }

            // Uncomment to draw step by step discovery of map
            //DrawMap(currentPos, oxygenPos);
        });
    }

    static IEnumerable<((int x, int y) pt, Move direction)> GetAdjacentMoves((int x, int y) pt)
    {
        yield return ((pt.x, pt.y - 1), Move.North);
        yield return ((pt.x + 1, pt.y), Move.East);
        yield return ((pt.x, pt.y + 1), Move.South);
        yield return ((pt.x - 1, pt.y), Move.West);
    }

    static int BreadthFirstSearch((int x, int y) from, (int x, int y) to)
    {
        var visited = new Dictionary<(int x, int y), int>();
        var q = new Queue<(int x, int y)>();
        q.Enqueue(from);
        visited.Add(from, 0);

        while (q.TryDequeue(out (int x, int y) current)) {
            if (current == to) {
                return visited[current];
            } else {
                //foreach (var offset in new[] {(dx: 1, dy: 0), (dx: 0, dy: 1), (dx: -1, dy: 0), (dx: 0, dy: -1)}) {
                foreach (var adjacent in GetAdjacentMoves(current)) {
                    //var adjacent = (x: current.x + offset.dx, y: current.y + offset.dy);
                    if (!visited.ContainsKey(adjacent.pt) && GetNodeType(adjacent.pt) == NodeType.Empty) {
                        q.Enqueue(adjacent.pt);
                        visited.Add(adjacent.pt, visited[current] + 1);
                    }
                }
            }
        }

        // Not exactly standard BFS behavior but useful here. If target not found, return longest path length.
        return visited.Values.Max();
    }

    static void MovedTo((int x, int y) to, Move? fromDirection)
    {
        if (GetNodeType(to) != NodeType.Unknown) return;
        map[to] = NodeType.Empty;

        // First push the move back command, executed last
        if (fromDirection != null) {
            var opposite = fromDirection.Value switch {
                Move.North => Move.South,
                Move.South => Move.North,
                Move.West  => Move.East,
                _          => Move.West
            };
            var moveBack = GetAdjacentMoves(to).First(a => a.direction == opposite);
            moveStack.Push((moveBack.pt.x, moveBack.pt.y, opposite));
        }

        // Then push visits to all adjacent locations that aren't known to be walls.
        foreach (var adjacent in GetAdjacentMoves(to)) {
            if (GetNodeType(adjacent.pt) == NodeType.Unknown) {
                moveStack.Push((adjacent.pt.x, adjacent.pt.y, adjacent.direction));
            }
        }
    }

    static void DrawMap((int x, int y) droid, (int x, int y) o2)
    {
        int minX = map.Keys.Min(pt => pt.x), maxX = map.Keys.Max(pt => pt.x);
        int minY = map.Keys.Min(pt => pt.y), maxY = map.Keys.Max(pt => pt.y);

        for (int y = minY; y <= maxY; y++) {
            for (int x = minX; x <= maxX; x++) {
                var pt = (x, y);
                if (pt == droid) {
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    Console.Write('D');
                    Console.BackgroundColor = ConsoleColor.Black;
                } else if (pt == o2) {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.Write('@');
                    Console.BackgroundColor = ConsoleColor.Black;
                } else {
                    Console.Write(GetNodeType(pt) switch {
                        NodeType.Wall => '█',
                        NodeType.Empty => '.',
                        _ => ' '
                    });
                }
            }
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    enum ParameterMode
    {
        Position,
        Immediate,
        Relative
    }

    static void RunProgram(long[] memory, Func<long> input, Action<long> output)
    {
        long ip = 0;
        long baseAddress = 0;
        bool halt = false;

        ParameterMode ParamMode(int ipOffset)
        {
            int modeDigit = ipOffset + 2;
            long mode = memory[ip];
            while (--modeDigit > 0) mode /= 10;
            return (ParameterMode)(mode %= 10);
        }

        long Read(int ipOffset) =>
            ParamMode(ipOffset) switch {
                ParameterMode.Position => memory[memory[ip + ipOffset]],
                ParameterMode.Immediate => memory[ip + ipOffset],
                ParameterMode.Relative => memory[baseAddress + memory[ip + ipOffset]],
                _ => throw new InvalidProgramException()
            };

        void Write(int ipOffset, long value)
        {
            if (ParamMode(ipOffset) == ParameterMode.Relative) {
                memory[baseAddress + memory[ip + ipOffset]] = value;
            } else {
                memory[memory[ip + ipOffset]] = value;
            }
        }

        while (!halt) {
            long opcode = memory[ip] % 100;
            switch (opcode) {
                case 1:
                    Write(3, Read(1) + Read(2));
                    ip += 4;
                    break;
                case 2:
                    Write(3, Read(1) * Read(2));
                    ip += 4;
                    break;
                case 3:
                    Write(1, input());
                    ip += 2;
                    break;
                case 4:
                    var outVal = Read(1); 
                    output(outVal);
                    ip += 2;
                    break;
                case 5:
                    if (Read(1) != 0) {
                        ip = Read(2);
                    } else {
                        ip += 3;
                    }
                    break;
                case 6:
                    if (Read(1) == 0) {
                        ip = Read(2);
                    } else {
                        ip += 3;
                    }
                    break;
                case 7:
                    Write(3, Read(1) < Read(2) ? 1 : 0);
                    ip += 4;
                    break;
                case 8:
                    Write(3, Read(1) == Read(2) ? 1 : 0);
                    ip += 4;
                    break;
                case 9:
                    baseAddress += Read(1);
                    ip += 2;
                    break;
                case 99:
                    halt = true;
                    break;
                default:
                    Console.Error.WriteLine($"Invalid opcode {opcode} at address {ip}");
                    halt = true;
                    break;
            }
        }
    }
}
