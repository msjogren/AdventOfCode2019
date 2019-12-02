using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int[] input = File.ReadAllText("input.txt").Split(',').Select(int.Parse).ToArray();

        // Part 1
        Console.WriteLine(RunProgram(MemoryWithNounAndVerb(input, noun: 12, verb: 2)));

        // Part 2
        const int Part2Output = 19690720;
        bool found = false;
        for (int noun = 0; !found && noun < 100; noun++)
            for (int verb = 0; !found && verb < 100; verb++) {
                int[] data = MemoryWithNounAndVerb(input, noun, verb);
                if (RunProgram(data) == Part2Output) {
                    Console.WriteLine(noun * 100 + verb);
                    found = true;
                }
            }
    }

    static int[] MemoryWithNounAndVerb(int[] input, int noun, int verb)
    {
        int[] data = (int[])input.Clone();
        data[1] = noun;
        data[2] = verb;
        return data;
    }

    static int RunProgram(int[] memory)
    {
        int ip = 0;
        bool halt = false;
        while (!halt) {
            switch (memory[ip]) {
                case 1:
                    memory[memory[ip+3]] = memory[memory[ip+1]] + memory[memory[ip+2]];
                    ip += 4;
                    break;
                case 2:
                    memory[memory[ip+3]] = memory[memory[ip+1]] * memory[memory[ip+2]];
                    ip += 4;
                    break;
                case 99:
                    halt = true;
                    break;
                default:
                    Console.Error.WriteLine($"Invalid opcode {memory[ip]} at address {ip}");
                    halt = true;
                    break;
            }
        }

        return memory[0];
    }
}
