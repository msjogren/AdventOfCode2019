using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Reaction
{
    public static Dictionary<string, Reaction> AllReactions = new Dictionary<string, Reaction>();

    public Reaction(string name, int count)
    {
        InputChemicals = new List<(string name, int count)>();
        OutputChemical = name;
        OutputChemicalCount = count;
    }

    public int StepsFromOre => OutputChemical == "ORE" ? 0 : InputChemicals.Max(c => AllReactions[c.name].StepsFromOre) + 1;
    public string OutputChemical { get; }
    public int OutputChemicalCount { get; }
    public List<(string name, int count)> InputChemicals { get; }
}

class Program
{
    static void Main(string[] args)
    {
        var reactions = File
            .ReadAllLines("input.txt")
            .Select(line => {
                (string name, int count) ParseNumberAndChemical(string s)
                {
                    var parts = s.Trim().Split(' ');
                    return (parts[1], int.Parse(parts[0]));
                }

                var inputAndOutput = line.Split("=>");
                var inputs = inputAndOutput[0].Split(",");
                var output = ParseNumberAndChemical(inputAndOutput[1]);

                var react = new Reaction(output.name, output.count);
                react.InputChemicals.AddRange(inputs.Select(input => ParseNumberAndChemical(input)));
                return react;
            });

        Reaction.AllReactions.Add("ORE", new Reaction("ORE", 1));
        foreach (var react in reactions) Reaction.AllReactions.Add(react.OutputChemical, react);

        // Part 1
        long minOrePerFuel = GetOreNeededForFuel(1);
        Console.WriteLine($"Part 1: {minOrePerFuel} ");

        // Part 2
        const long trillion = 1_000_000_000_000;        
        long oreLeft = trillion;
        long fuelProduced = 0;
        while (oreLeft > minOrePerFuel) {
            long fuelToProduce = (long)Math.Floor((double)oreLeft / (double)minOrePerFuel);
            oreLeft -= GetOreNeededForFuel(fuelToProduce);
            fuelProduced += fuelToProduce;
        }
        // Above estimate gives correct or slightly too low result
        while (GetOreNeededForFuel(fuelProduced + 1) < trillion) fuelProduced++;
        Console.WriteLine($"Part 2: {fuelProduced}");
    }

    static long GetOreNeededForFuel(long fuelCount)
    {
        var needs = new Dictionary<string, long>() { { "FUEL", fuelCount } };

        do {
            var chemicalNeeded = needs.OrderByDescending(need => Reaction.AllReactions[need.Key].StepsFromOre).First();
            long needCount = chemicalNeeded.Value;
            var reaction = Reaction.AllReactions[chemicalNeeded.Key];
            var multiplier = (long)Math.Ceiling((double)needCount / (double)reaction.OutputChemicalCount);

            foreach (var inputChemical in reaction.InputChemicals) {{
                if (needs.TryGetValue(inputChemical.name, out long existingNeed)) {
                    needs[inputChemical.name] = existingNeed + multiplier * inputChemical.count;
                } else {
                    needs[inputChemical.name] = multiplier * inputChemical.count;
                }
            }}
            needs.Remove(chemicalNeeded.Key);
        } while (needs.Keys.Count > 1);
        
        return needs.First().Value;
    }
}
