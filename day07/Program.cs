using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int[] code = File.ReadAllText("input.txt").Split(',').Select(int.Parse).ToArray();

        int answerPart1 = Enumerable
            .Range(01234, 43210 - 01234 + 1)
            .Where(HasUniqueDigits0To4)
            .Select(phaseSettings => {
            
            int input = 0;

            for (int i = 0, div = 10000; i < 5; i++, div /= 10) {
                Amplifier amp = new Amplifier((int[])code.Clone(), (phaseSettings / div) % 10);
                input = amp.ProcessInput(input).Value;
            }

            return input;
        }).Max();

        Console.WriteLine("Part 1: " + answerPart1);

        int answerPart2 = Enumerable
            .Range(56789, 98765 - 56789 + 1)
            .Where(HasUniqueDigits5To9)
            .Select(phaseSettings => {

            Amplifier[] amps = new Amplifier[5];

            for (int i = 0, div = 10000; i < 5; i++, div /= 10) {
                amps[i] = new Amplifier((int[])code.Clone(), (phaseSettings / div) % 10);
            }

            int input = 0;
            int lastEOutput = -1;
            bool done = false;

            while (!done) {
                for (int i = 0; i < 5; i = (i + 1) % 5) {
                    var result = amps[i].ProcessInput(input);
                    if (result == null) {
                        // halted
                        done = true;
                        break;
                    } else {
                        // output signal
                        input = result.Value;
                        if (i == 4) lastEOutput = input;
                    }
                }
            }

            return lastEOutput;
        }).Max();

        Console.WriteLine("Part 2: " + answerPart2);
    }

    static bool HasUniqueDigits0To4(int i)
    {
        int[] occurances = new int[5];
        for (int pos = 0, div = 1; pos < 5; pos++, div *= 10) {
            int digit = (i / div) % 10;
            if (digit > 4) return false;
            if (++occurances[digit] > 1) return false;
        }
        return true;
    }

    static bool HasUniqueDigits5To9(int i)
    {
        int[] occurances = new int[5];
        for (int pos = 0, div = 1; pos < 5; pos++, div *= 10) {
            int digit = (i / div) % 10;
            if (digit < 5) return false;
            if (++occurances[digit-5] > 1) return false;
        }
        return true;
    }
}

class Amplifier
{
    private int[] memory;
    private int ip;
    private int? firstInput;

    public Amplifier(int[] program, int phaseSetting)
    {
        this.memory = program;
        this.firstInput = phaseSetting;
    }

    public int? ProcessInput(int input)
    {
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

        while (true) {
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
                    if (this.firstInput != null) {
                        Write(1, firstInput.Value);
                        firstInput = null;
                    } else {
                        Write(1, input);
                    }
                    ip += 2;
                    break;
                case 4:
                    var outVal = Read(1); 
                    ip += 2;
                    return outVal;
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
                    return null;
                default:
                    Console.Error.WriteLine($"Invalid opcode {opcode} at address {ip}");
                    return null;
            }
        }
    }
}