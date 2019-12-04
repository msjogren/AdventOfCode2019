using System;
using System.Linq;

class Program
{
    const int NumberOfDigits = 6;
    static readonly int[] divisors = {100_000, 10_000, 1_000, 100, 10, 1};

    static void Main(string[] args)
    {
        const int from = 138241, to = 674034;

        var part1Passwords = Enumerable
            .Range(from, to - from + 1)
            .Where(HasTwoOrMoreAdjacentEqualDigits)
            .Where(HasNoDecreasingDigits);
        Console.WriteLine(part1Passwords.Count());

        var part2Passwords = part1Passwords.Where(HasExactlyTwoAdjacentEqualDigits);
        Console.WriteLine(part2Passwords.Count());
    }

    static int DigitAtPosition(int number, int pos) => (number / divisors[pos]) % 10;

    static bool HasTwoOrMoreAdjacentEqualDigits(int number)
    {
        for (int i = 0; i < NumberOfDigits - 1; i++) {
            int leftDigit = DigitAtPosition(number, i);
            int rightDigit = DigitAtPosition(number, i + 1);
            if (leftDigit == rightDigit) return true;
        }

        return false;
    }

    static bool HasExactlyTwoAdjacentEqualDigits(int number)
    {
        for (int i = 0; i < NumberOfDigits - 1; i++) {
            int leftDigit = DigitAtPosition(number, i);
            int rightDigit = DigitAtPosition(number, i + 1);
            if (leftDigit == rightDigit) {
                if (i > 0) {
                    int digitBefore = DigitAtPosition(number, i - 1);
                    if (digitBefore == leftDigit) continue;
                }
                if (i < NumberOfDigits - 2) {
                    int digitAfter = DigitAtPosition(number, i + 2);
                    if (digitAfter == rightDigit) continue;
                }
                return true;
            }
        }

        return false;
    }

    static bool HasNoDecreasingDigits(int number)
    {
        for (int i = 0; i < NumberOfDigits - 1; i++) {
            int leftDigit = DigitAtPosition(number, i);
            int rightDigit = DigitAtPosition(number, i + 1);
            if (leftDigit > rightDigit) return false;
        }

        return true;
    }
}
