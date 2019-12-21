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

        string[] scripts = new string[2]; 
        /*
            Part 1

                ABCD
            if @??.# then jump
            if @?.## then jump
            if @.??? then jump 
        */
        scripts[0] =
            // if @??.# then jump
            "NOT C J\n" +
            "AND D J\n" +
            // or if @?.## then jump
            "NOT B T\n" +
            "AND D T\n" +
            "OR T J\n" +
            // or if @.??? then jump
            "NOT A T\n" +
            "OR T J\n" +
            "WALK\n";

        /*
            Part 2

                ABCDEFGHI
            if @??.#        then jump
            if @?.##        then jump
            if @.???        then jump
            if @##?#?#?.?   then dont jump yet
        */
        scripts[1] =
            // if @??.# then jump  
            "NOT C J\n" +
            "AND D J\n" +
            // or if @?.## then jump
            "NOT B T\n" +
            "AND D T\n" +
            "OR T J\n" +
            // or if @.??? then jump
            "NOT A T\n" +
            "OR T J\n" +
            // if @##?#?#... then dont jump yet
            "NOT H T\n" + 
            "AND F T\n" +
            "AND D T\n" +
            "AND B T\n" +
            "AND A T\n" +
            "NOT T T\n" +
            "AND T J\n" +
            "RUN\n";

        for (int part = 1; part <= 2; part++) {
            Array.Copy(code, memory, code.Length);
            var inputAscii = Encoding.ASCII.GetBytes(scripts[part-1]);
            var nextInput = 0;
            RunProgram(memory, () => {
                return inputAscii[nextInput++];
            }, i => {
                if (i > 128)
                    Console.WriteLine($"Part {part}: {i}");
                else
                    Console.Write((char)i);
            });
        }
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

