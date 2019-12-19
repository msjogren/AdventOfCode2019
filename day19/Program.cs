using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        long[] code = File.ReadAllText("input.txt").Split(',').Select(long.Parse).ToArray();
        long[] memory = new long[1_000_000];

        // Part 1
        int part1Sum = 0;
        for (int y = 0; y < 50; y++) {
            for (int x = 0; x < 50; x++) {
                bool inputY = false;
                Array.Copy(code, memory, code.Length);
                RunProgram(memory, () => {
                    int input = inputY ? y : x;
                    inputY = !inputY;
                    return input;
                }, i => {
                    Console.Write(i == 1 ? '#' : '.');
                    part1Sum += (int)i;
                });
            }
            Console.WriteLine();
        }
        Console.WriteLine("Part 1: " + part1Sum);


        // Part 2
        const int MaxY = 1000;
        const int MaxX = 2 * MaxY;
        int beamStartX = 0;
        int[] beamEndX = new int[MaxY];
        bool part2Done = false;
        

        for (int y = 1; y < MaxY && !part2Done; y++) {
            bool lineDone = false;
            int lastLineStartX = beamStartX;
            beamStartX = 0;

            for (int x = lastLineStartX; !lineDone && x < MaxX; x++) {
                bool inputY = false;
                Array.Copy(code, memory, code.Length);
                RunProgram(memory, () => {
                    int input = inputY ? y : x;
                    inputY = !inputY;
                    return input;
                }, i => {
                    if (beamStartX == 0 && i == 1) {
                        beamStartX = x;
                    } else if (beamStartX > 0 && i == 0) {
                        lineDone = true;
                        beamEndX[y] = x - 1;
                        if (y >= 100 && beamEndX[y - 99] >= (beamStartX + 99)) {
                            part2Done = true;
                            Console.WriteLine("Part 2: " + (10_000 * beamStartX + (y - 99)));
                        }
                    }
                });
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

