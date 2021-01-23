using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDUS.Models
{
    public class NeuralNetwork
    {
        public double CalculateDistance(Star currentStar, Star otherStar)
        {
            double distance = Math.Sqrt(Math.Pow(otherStar.Coords["x"] - currentStar.Coords["x"], 2) + Math.Pow(otherStar.Coords["y"] - currentStar.Coords["y"], 2) + Math.Pow(otherStar.Coords["z"] - currentStar.Coords["z"], 2));

            return distance;
        }

        public Dictionary<string, double> CalculateMinMaxCoords(Star star, double distance)
        {
            Dictionary<string, double> minMaxCoords = new Dictionary<string, double>
            {
                { "minX", star.Coords["x"] - distance},
                { "maxX", star.Coords["x"] + distance},
                { "minY", star.Coords["y"] - distance},
                { "maxY", star.Coords["y"] + distance},
                { "minZ", star.Coords["z"] - distance},
                { "maxZ", star.Coords["z"] + distance}
            };

            return minMaxCoords;
        }

        public Dictionary<int, Tuple<double, double, double>> PartitionCube(int amountOfCubes, Dictionary<string, double> minMaxCoordinates)
        {
            double xSplitLength = Math.Abs(minMaxCoordinates["maxX"] - minMaxCoordinates["minX"]) / Math.Pow(amountOfCubes, (double)1 / 3);
            double ySplitLength = Math.Abs(minMaxCoordinates["maxY"] - minMaxCoordinates["minY"]) / Math.Pow(amountOfCubes, (double)1 / 3);
            double zSplitLength = Math.Abs(minMaxCoordinates["maxZ"] - minMaxCoordinates["minZ"]) / Math.Pow(amountOfCubes, (double)1 / 3);

            Dictionary<int, double> newCubeXLimits = new Dictionary<int, double>
            {
                { 0, minMaxCoordinates["minX"] }
            };
            Dictionary<int, double> newCubeYLimits = new Dictionary<int, double>
            {
                { 0, minMaxCoordinates["minY"] }
            };
            Dictionary<int, double> newCubeZLimits = new Dictionary<int, double>
            {
                { 0, minMaxCoordinates["minZ"] }
            };

            for(var i = 0; i < Math.Pow(amountOfCubes, (double)1 / 3); i++)
            {
                newCubeXLimits.Add(i + 1, newCubeXLimits[i] + xSplitLength);
                newCubeYLimits.Add(i + 1, newCubeYLimits[i] + ySplitLength);
                newCubeZLimits.Add(i + 1, newCubeZLimits[i] + zSplitLength);
            }

            Dictionary<int, Tuple<double, double, double>> newCubes = new Dictionary<int, Tuple<double, double, double>>();

            int counter = 0;

            for(var i = 0; i < newCubeXLimits.Count(); i++)
            {
                for (var d = 0; d < newCubeYLimits.Count(); d++)
                {
                    for (var k = 0; k < newCubeZLimits.Count(); k++)
                    {
                        Tuple<double, double, double> newCoords = Tuple.Create(newCubeXLimits[i], newCubeYLimits[d], newCubeZLimits[k]);
                        newCubes.Add(counter, newCoords);
                        counter++;
                    }
                }
            }

            return newCubes;
        }
    }
}
