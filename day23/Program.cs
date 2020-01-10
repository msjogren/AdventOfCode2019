using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        long[] code = File.ReadAllText("input.txt").Split(',').Select(long.Parse).ToArray();
        
        const int ComputerCount = 50;
        var computers = new IntcodeComputer[ComputerCount];

        for (int i = 0; i < ComputerCount; i++) {
            computers[i] = new IntcodeComputer(code, i);
        }

        bool part1Done = false, part2Done = false;
        (long x, long y) nat = (-1, -1);
        long prevNatY = 0;

        while (!part2Done) {

            for (int i = 0; i < ComputerCount; i++) {
                bool waitingForInput = computers[i].Run();
                if (waitingForInput) {
                    computers[i].InputQueue.Enqueue(-1);
                }

                var outputs = computers[i].OutputQueue;
                while (outputs.Count >= 3) {
                    long addr, x, y;
                    outputs.TryDequeue(out addr);
                    outputs.TryDequeue(out x);
                    outputs.TryDequeue(out y);

                    if (addr == 255) {
                        nat = (x, y);
                        if (!part1Done) {
                            Console.WriteLine($"Part 1: {y}");
                            part1Done = true;
                        }
                    } else {
                        computers[addr].InputQueue.Enqueue(x);
                        computers[addr].InputQueue.Enqueue(y);
                    }
                }
            }

            bool idle = computers.Count(c => c.InputQueue.Count > 1) == 0;
            if (idle) {
                if (prevNatY == nat.y) {
                    Console.WriteLine($"Part 2: {nat.y}");
                    part2Done = true;
                }
                computers[0].InputQueue.Enqueue(nat.x);
                computers[0].InputQueue.Enqueue(nat.y);
                prevNatY = nat.y;
            }
        }
    }
}

class IntcodeComputer
{
    private long[] _memory = new long[1_000_000];
    private int _networkAddress;

    private long _ip = 0;
    private long _baseAddress = 0;

    enum ParameterMode
    {
        Position,
        Immediate,
        Relative
    }

    public Queue<long> InputQueue = new Queue<long>();
    public Queue<long> OutputQueue = new Queue<long>();

    public IntcodeComputer(long[] code, int address)
    {
        Array.Copy(code, _memory, code.Length);
        _networkAddress = address;
        InputQueue.Enqueue(address);
    }

    public bool Run()
    {
        var memory = _memory;

        ParameterMode ParamMode(int ipOffset)
        {
            int modeDigit = ipOffset + 2;
            long mode = memory[_ip];
            while (--modeDigit > 0) mode /= 10;
            return (ParameterMode)(mode %= 10);
        }

        long Read(int ipOffset) =>
            ParamMode(ipOffset) switch {
                ParameterMode.Position => memory[memory[_ip + ipOffset]],
                ParameterMode.Immediate => memory[_ip + ipOffset],
                ParameterMode.Relative => memory[_baseAddress + memory[_ip + ipOffset]],
                _ => throw new InvalidProgramException()
            };

        void Write(int ipOffset, long value)
        {
            if (ParamMode(ipOffset) == ParameterMode.Relative) {
                memory[_baseAddress + memory[_ip + ipOffset]] = value;
            } else {
                memory[memory[_ip + ipOffset]] = value;
            }
        }

        while (true) {
            long opcode = memory[_ip] % 100;
            switch (opcode) {
                case 1:
                    Write(3, Read(1) + Read(2));
                    _ip += 4;
                    break;
                case 2:
                    Write(3, Read(1) * Read(2));
                    _ip += 4;
                    break;
                case 3:
                    if (InputQueue.TryDequeue(out var inputValue)) {
                        Write(1, inputValue);
                        _ip += 2;
                    } else {
                        return true;
                    }
                    break;
                case 4:
                    var outVal = Read(1); 
                    OutputQueue.Enqueue(outVal);
                    _ip += 2;
                    break;
                case 5:
                    if (Read(1) != 0) {
                        _ip = Read(2);
                    } else {
                        _ip += 3;
                    }
                    break;
                case 6:
                    if (Read(1) == 0) {
                        _ip = Read(2);
                    } else {
                        _ip += 3;
                    }
                    break;
                case 7:
                    Write(3, Read(1) < Read(2) ? 1 : 0);
                    _ip += 4;
                    break;
                case 8:
                    Write(3, Read(1) == Read(2) ? 1 : 0);
                    _ip += 4;
                    break;
                case 9:
                    _baseAddress += Read(1);
                    _ip += 2;
                    break;
                case 99:
                    return false;
                default:
                    Console.Error.WriteLine($"Invalid opcode {opcode} at address {_ip}");
                    return false;
            }
        }
    }
}

