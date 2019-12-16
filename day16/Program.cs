using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static class Program
{
    static void Main(string[] args)
    {
        string input = File.ReadAllText("input.txt");

        // Part 1
        int[] signal = input.Select(ch => ch - '0').ToArray();
        int[] output = new int[signal.Length];
        int[] basePattern = {0, 1, 0, -1};

        for (int phase = 0; phase < 100; phase++) {
            for (int signalPos = 0; signalPos < signal.Length; signalPos++) {
                long sum = 0;
                for (int multiplierPos = 0; multiplierPos < signal.Length; multiplierPos++) {
                    int multiplier = basePattern[((multiplierPos + 1) / (signalPos + 1)) % 4];
                    sum += multiplier * signal[multiplierPos];
                }
                output[signalPos] = (int)((long)Math.Abs(sum) % 10);
            }

            var tmp = signal;
            signal = output;
            output = tmp;
        }

        Console.Write("Part 1: ");
        foreach (int i in signal.Take(8)) Console.Write(i);
        Console.WriteLine();


        // Part 2

        // Optimizations:
        // n = input signal length
        // For positions p > n/2, the multiplication sequence contains just 0 and 1 (no -1).
        // Last multiplication sequences are 
        //   
        //  ...
        //  n-2 000...000111
        //  n-1 000...000011
        //  n   000...000001
        //
        // So for positions p > n/2, the multiplication sum is equal to the sum of numbers from p to the end.
        // Can be summarized in reverse order to avoid duplicate effort.
        // The requested message offset seems to be close to the end of the input signal (at least > n/2)
        // both for the real input and for samples, this would not work if offset < n/2.

        signal = input.Select(ch => ch - '0').RepeatSequence(10_000).ToArray();
        output = new int[signal.Length];
        int messageOffset = int.Parse(input.Substring(0, 7));

        for (int phase = 0; phase < 100; phase++) {
            long sum = 0;
            for (int signalPos = signal.Length - 1; signalPos >= messageOffset; signalPos--) {
                sum += signal[signalPos];
                output[signalPos] = (int)(sum % 10);
            }

            var tmp = signal;
            signal = output;
            output = tmp;
        }

        Console.Write("Part 2: ");
        foreach (int i in signal.Skip(messageOffset).Take(8)) Console.Write(i);
        Console.WriteLine();            
    }

    static IEnumerable<T> RepeatSequence<T>(this IEnumerable<T> sequence, int n = 1)
    {
        for (int i = 0; i < n; i++)
            foreach (var t in sequence)
                yield return t;
    }
}
