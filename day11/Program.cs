using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

class Program
{
    enum Direction {Up, Right, Down, Left}

    static void Main(string[] args)
    {
        long[] code = File.ReadAllText("input.txt").Split(',').Select(long.Parse).ToArray();
        const int Black = 0, White = 1;

        foreach (int part in new[] {1, 2}) {
            long[] memory = new long[1_000_000];
            Array.Copy(code, memory, code.Length);
            var paintedSquares = new Dictionary<Point, int>();
            Point robotPos = Point.Empty;
            Direction robotDir = Direction.Up;
            bool outputIsTurn = false;

            paintedSquares[robotPos] = part == 2 ? White : Black;

            RunProgram(memory, () => {
                return paintedSquares.TryGetValue(robotPos, out int color) ? color : Black;
            }, i => {
                if (outputIsTurn) {
                    if (i == 1) {
                        robotDir = (Direction)(((int)robotDir + 1) % 4);
                    } else {
                        robotDir = (Direction)(((int)robotDir - 1 + 4) % 4);
                    }
                    robotPos = robotDir switch {
                        Direction.Up => new Point(robotPos.X, robotPos.Y - 1),
                        Direction.Right => new Point(robotPos.X + 1, robotPos.Y),
                        Direction.Down => new Point(robotPos.X, robotPos.Y + 1),
                        _ => new Point(robotPos.X - 1, robotPos.Y),
                    };
                    outputIsTurn = false;
                } else {
                    paintedSquares[robotPos] = (int)i;
                    outputIsTurn = true;
                }
            });

            Console.WriteLine($"Part {part}:");
            Console.WriteLine($"{paintedSquares.Count} painted squares");

            int minX = paintedSquares.Keys.Min(pt => pt.X),
                maxX = paintedSquares.Keys.Max(pt => pt.X),
                minY = paintedSquares.Keys.Min(pt => pt.Y),
                maxY = paintedSquares.Keys.Max(pt => pt.Y);

            for (int y = 0; y < (maxY - minY + 1); y++) {
                for (int x = 0; x < (maxX - minX + 1); x++) {
                    var color = paintedSquares.TryGetValue(new Point(x + minX, y + minY), out int c) ? c : Black;
                    Console.Write(c == White ? '█' : '.');
                }
                Console.WriteLine();
            }
        }
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
