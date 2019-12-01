using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        int sum1 = 0, sum2 = 0;

        int FuelForMass(int mass) => (mass / 3) - 2;

        foreach (string line in File.ReadAllLines("input.txt")) {
            int fuel = FuelForMass(int.Parse(line)); 
            sum1 += fuel;

            while (fuel > 0) {
                sum2 += fuel;
                fuel = FuelForMass(fuel);
            }
        }

        Console.WriteLine(sum1);
        Console.WriteLine(sum2);
    }
}
