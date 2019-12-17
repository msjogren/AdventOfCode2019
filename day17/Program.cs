using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        long[] code = File.ReadAllText("input.txt").Split(',').Select(long.Parse).ToArray();
        long[] memory = new long[1_000_000];
        Array.Copy(code, memory, code.Length);

        // Part 1
        var output = new List<byte>();
        RunProgram(memory, () => {
            return 1;
        }, i => {
            if (output.Any() && output.Last() == 10 && i == 10) {
                Console.WriteLine($"Part 1: {ProcessOutput(output)}");
            } else {
                output.Add((byte)i);
            }
        });

        /*
            Part 2

            Manually tracing the output path.
            
            Path: L4 L6 L8 L12 L8 R12 L12 L8 R12 L12 L4 L6 L8 L12 L8 R12 L12 R12 L6 L6 L8 L4 L6 L8 L12 R12 L6 L6 L8 L8 R12 L12 R12 L6 L6 L8
            With:
            A = L4 L6 L8 L12
            B = L8 R12 L12
            C = R12 L6 L6 L8
            Gives main routine: A B B A B C A C B C
        */
        memory = new long[1_000_000];
        Array.Copy(code, memory, code.Length);
        memory[0] = 2;
        var inputAscii = Encoding.ASCII.GetBytes(
            "A,B,B,A,B,C,A,C,B,C\n" +
            "L,4,L,6,L,8,L,12\n" +
            "L,8,R,12,L,12\n" +
            "R,12,L,6,L,6,L,8\n" +
            "n\n"                     // No video feed
        );
        var nextInput = 0;
        RunProgram(memory, () => {
            return inputAscii[nextInput++];
        }, i => {
            if (i > 128) Console.WriteLine("Part 2: " + i);
        });
    }

    static int ProcessOutput(IEnumerable<byte> output)
    {
        var outputString = Encoding.ASCII.GetString(output.ToArray());
        Console.WriteLine(outputString);
        var lines = outputString.Split('\n');

        int alignmentParameterSum = 0;
        for (int y = 1; y < lines.Length - 1; y++) {
            string line = lines[y];
            for (int x = 1; x < line.Length - 1; x++) {
                if (line[x - 1] == '#' && line[x] == '#' && line[x + 1] == '#' &&
                    lines[y - 1][x] == '#' && lines[y + 1][x] == '#') {
                        alignmentParameterSum += x * y;
                }
            }
        }

        return alignmentParameterSum;
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

