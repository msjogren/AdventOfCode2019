using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class SpaceObject
{
    public SpaceObject(string name) { Name = name; }
    public string Name { get; private set; }
    public SpaceObject Orbited { get; set; }
    public int OrbitCount => Orbited == null ? 0 : Orbited.OrbitCount + 1;
}

class Program
{
    static void Main(string[] args)
    {
        var input = File.ReadAllLines("input.txt").Select(line => {
            var spaceObjects = line.Split(')');
            return (orbited: spaceObjects[0], orbiting: spaceObjects[1]);
        });

        var allObjects = new Dictionary<string, SpaceObject>();
        SpaceObject you = null, san = null;

        SpaceObject GetObject(string name)
        {
            if (!allObjects.TryGetValue(name, out SpaceObject spaceObj)) {
                spaceObj = new SpaceObject(name);
                allObjects.Add(name, spaceObj);
            }
            return spaceObj;
        }

        // Build graph
        foreach (var orbit in input) {
            var orbitedObject = GetObject(orbit.orbited);
            var orbitingObject = GetObject(orbit.orbiting);
            orbitingObject.Orbited = orbitedObject;

            if (orbit.orbiting == "YOU") you = orbitingObject;
            if (orbit.orbiting == "SAN") san = orbitingObject;
        }

        // Part 1
        int sum = allObjects.Sum(kvp => kvp.Value.OrbitCount);
        Console.WriteLine(sum);

        // Part 2
        // Calculate tranfers to first common ancestor
        SpaceObject youPath = you.Orbited, sanPath = san.Orbited;
        int tranfers = 0;
        do {
            if (youPath.OrbitCount > sanPath.OrbitCount) {
                youPath = youPath.Orbited;
                tranfers++;
            } else if (youPath.OrbitCount < sanPath.OrbitCount) {
                sanPath = sanPath.Orbited;
                tranfers++;
            } else {
                youPath = youPath.Orbited;
                sanPath = sanPath.Orbited;
                tranfers += 2;
            }
        } while (youPath != sanPath);
        Console.WriteLine(tranfers);
    }
}
