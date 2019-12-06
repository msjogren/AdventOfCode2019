using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int[] code = File.ReadAllText("input.txt").Split(',').Select(int.Parse).ToArray();

        Console.WriteLine("Part 1:");
        RunProgram((int[])code.Clone(), () => 1, i => {
            Console.WriteLine("Output: " + i);
        });

        Console.WriteLine("Part 2:");
        RunProgram((int[])code.Clone(), () => 5, i => {
            Console.WriteLine("Output: " + i);
        });
    }

    static void RunProgram(int[] memory, Func<int> input, Action<int> output)
    {
        int ip = 0;
        bool halt = false;

        int Read(int ipOffset)
        {
            int modeDigit = ipOffset + 2;
            int mode = memory[ip];
            while (--modeDigit > 0) mode /= 10;
            mode %= 10;
            if (mode == 1) {    // Immediate mode
                return memory[ip + ipOffset];
            } else {            // Position mode
                return memory[memory[ip + ipOffset]];
            }
        }

        void Write(int ipOffset, int value) => memory[memory[ip + ipOffset]] = value;

        while (!halt) {
            int opcode = memory[ip] % 100;
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
