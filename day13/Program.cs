using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

class Program
{
    enum Tile {Empty, Wall, Block, Paddle, Ball}
    const int Width = 50, Height = 30;

    static void Main(string[] args)
    {
        const bool Part2 = false;
        const bool DrawGame = true;

        long[] code = File.ReadAllText("input.txt").Split(',').Select(long.Parse).ToArray();
        long[] memory = new long[1_000_000];
        Array.Copy(code, memory, code.Length);

        if (Part2) memory[0] = 2;

        var grid = new Tile[Width, Height];
        int outputCount = 0;
        int nextX = 0, nextY = 0;
        int paddleX = 0;
        int ballX = 0;
        int score = 0;
        bool gameStarted = false;

        RunProgram(memory, () => {
            if (DrawGame) Thread.Sleep(40);
            return Math.Sign(ballX - paddleX);
        }, i => {
            switch (outputCount++ % 3) {
                case 0: nextX = (int)i; break;
                case 1: nextY = (int)i; break;
                case 2:
                    if (nextX == -1) {
                        score = (int)i;
                        gameStarted = true;
                    } else {
                        grid[nextX, nextY] = (Tile)i;
                        switch (grid[nextX, nextY]) {
                            case Tile.Ball:
                                ballX = nextX; 
                                break;
                            case Tile.Paddle:
                                paddleX = nextX;
                                break;
                        }
                    }
                    if (gameStarted && DrawGame) {
                        DrawGrid(grid);
                        Console.WriteLine($"Score: {score}");
                    }
                    break;
            }
        });

        if (Part2) {
            Console.WriteLine($"Part 2: Final score {score}");
        } else {
            int blocks = DrawGrid(grid);
            Console.WriteLine($"Part 1: {blocks} blocks");
        }
    }

    static int DrawGrid(Tile[,] grid)
    {
        int blocks = 0;

        Console.Clear();

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                if (grid[x, y] == Tile.Block) blocks++;
                Console.Write(grid[x, y] switch {
                    Tile.Wall => '|',
                    Tile.Block => '█',
                    Tile.Paddle => '_',
                    Tile.Ball => '*',
                    _ => ' '
                });
            }
            Console.WriteLine();
        }

        return blocks;
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
