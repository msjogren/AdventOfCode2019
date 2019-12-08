using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static class Program
{
    const int Width = 25, Height = 6;

    static void Main(string[] args)
    {
        string input = File.ReadAllText("input.txt");

        // Part 1
        var fewestZeros = input
            .SplitLayers()
            .Select(l => (layer: l, zeros: l.Count(c => c == '0')))
            .OrderBy(t => t.zeros)
            .First();

        Console.WriteLine("Part 1: " + (fewestZeros.layer.Count(c => c == '1') * fewestZeros.layer.Count(c => c == '2')));

        // Part 2
        const char Black = '0';
        const char White = '1';
        const char Transparent = '2';      
        string image = input
            .SplitLayers()
            .Reverse()
            .Aggregate((backImage, frontLayer) => {
                return new String(backImage
                    .Zip(frontLayer)
                    .Select<(char back, char front), char>(
                        chars => (chars.front == Transparent ? chars.back : chars.front)
                    )
                    .ToArray());
            });

        Console.WriteLine("Part 2:");
        foreach (string imageRow in image.SplitRows()) {
            Console.WriteLine(imageRow.Replace(Black, ' ').Replace(White, '█'));
        }
    }

    static IEnumerable<string> SplitEvery(this string image, int count)
    {
        int startIdx = 0;
        while (startIdx < image.Length) {
            yield return image.Substring(startIdx, count);
            startIdx += count;
        }
    }

    static IEnumerable<string> SplitLayers(this string image) => SplitEvery(image, Width * Height);
    static IEnumerable<string> SplitRows(this string image) => SplitEvery(image, Width);
}
