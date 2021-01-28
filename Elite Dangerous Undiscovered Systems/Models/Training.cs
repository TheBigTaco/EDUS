using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EDUS.Models
{
    public class Training
    {
        // How many stars it should loop through for training data.
        // TODO: Make a method to get count of stars in database and set loops to it multiplied by X
        static readonly Int64 loops = 10000;
        static readonly int cubes = 125;
        static readonly int maxX = 40000;
        static readonly int minX = -40000;
        static readonly int maxY = 60000;
        static readonly int minY = -10000;
        static readonly int maxZ = 9000;
        static readonly int minZ = -11000;
        static readonly double maxDistance = 80;
        static readonly double minDistance = 0.1;
        static readonly int hiddenLayerCount = (int)Math.Ceiling((double)cubes / 2);
        public static void Train()
        {

            NeuralNetwork net = new NeuralNetwork(new int[] { cubes + 4, hiddenLayerCount, hiddenLayerCount, hiddenLayerCount, 3 });

            int? currentId = 0;

            for (var trainingLoop = 0; trainingLoop < loops; trainingLoop++)
            {
                // Get a random distance between 0.1 and 80
                double distance = new Random().NextDouble() * (maxDistance - minDistance) + minDistance;

                // Get the star to be used as the correct answer (expected) for the training data
                currentId = GetCurrentStarId(currentId);
                if (currentId == null) currentId = 0;

                // Set the currentId to the current stars ID so we can select the next star on the next loop
                Star expectedStar = GetCurrentStar(currentId);

                // Get stars within distance around expected star to be used as center point of cube
                List<Star> starsToChoose = GetStarsWithinDistance(expectedStar, CalculateMinMaxCoords(expectedStar, distance));

                // If there are no stars within distance, increase distance and try again
                int counter = 1;
                while (starsToChoose.Count() == 0)
                {
                    distance += counter;
                    starsToChoose = GetStarsWithinDistance(expectedStar, CalculateMinMaxCoords(expectedStar, distance));
                    counter++;
                }

                // Select a random star from the list of stars to choose to use as center point of cube
                Star star = starsToChoose[new Random().Next(0, starsToChoose.Count() - 1)];

                // Get cube min max coordinates
                Dictionary<string, double> minMax = CalculateMinMaxCoords(star, distance);

                // Get stars around center point. This should always include expected star.
                List<Star> stars = GetStarsWithinDistance(star, minMax);

                // Partition cube into 100 mini cubes
                Dictionary<int, Tuple<double, double, double>> miniCubes = PartitionCube(cubes, minMax);

                // Count how many stars are in each partitioned mini cube and add to list of star counts.
                Dictionary<int, float> starCounts = new Dictionary<int, float>();
                
                for (var i = 1; i < miniCubes.Count(); i++)
                {
                    int count = 0;

                    foreach (Star s in stars)
                    {
                        if (s.Coords["x"] < miniCubes[i].Item1 && s.Coords["x"] >= miniCubes[i - 1].Item1 && s.Coords["y"] < miniCubes[i].Item2 && s.Coords["y"] >= miniCubes[i - 1].Item2 && s.Coords["z"] < miniCubes[i].Item3 && s.Coords["z"] >= miniCubes[i - 1].Item3)
                        {
                            count++;
                            stars.Remove(s);
                        }
                    }

                    // Normalize count to be between 0 and 1
                    float normalizedCount = 0;
                    if (stars.Count() != 0) normalizedCount = count / stars.Count();

                    starCounts.Add(i - 1, normalizedCount);
                }

                // Normalize center star coordinates and distance
                float normalizedX = (float)((star.Coords["x"] - minX) / (maxX - minX));
                float normalizedY = (float)((star.Coords["y"] - minY) / (maxY - minY));
                float normalizedZ = (float)((star.Coords["z"] - minZ) / (maxZ - minZ));
                float normalizedDistance = (float)((distance + 0.1) / 79.9);

                // Creat input array
                float[] inputs = new float[cubes + 4];

                inputs[0] = normalizedX;
                inputs[1] = normalizedY;
                inputs[2] = normalizedZ;
                inputs[3] = normalizedDistance;
                for(var i = 0; i < starCounts.Count(); i++)
                {
                    inputs[i + 4] = starCounts[i];
                }

                // Input normalized fields
                float[] output = net.FeedForward(inputs);

                float outputX = output[0] * (maxX - minX) + minX;
                float outputY = output[1] * (maxY - minY) + minY;
                float outputZ = output[2] * (maxZ - minZ) + minZ;

                // Normalize expected star coordinates
                float normalizedExpectedX = (float)((expectedStar.Coords["x"] - minX) / (maxX - minX));
                float normalizedExpectedY = (float)((expectedStar.Coords["y"] - minY) / (maxY - minY));
                float normalizedExpectedZ = (float)((expectedStar.Coords["z"] - minZ) / (maxZ - minZ));

                float xAccuracy = (1 - (Math.Abs(normalizedExpectedX - output[0]) / 100)) * 100;
                float yAccuracy = (1 - (Math.Abs(normalizedExpectedY - output[1]) / 100)) * 100;
                float zAccuracy = (1 - (Math.Abs(normalizedExpectedZ - output[2]) / 100)) * 100;

                Console.WriteLine(@$"
Output X: {outputX} Expected X: {expectedStar.Coords["x"]} X Accuracy: {xAccuracy}
Output Y: {outputY} Expected Y: {expectedStar.Coords["y"]} Y Accuracy: {yAccuracy}
Output Z: {outputZ} Expected Z: {expectedStar.Coords["z"]} Z Accuracy: {zAccuracy}
");

                // Back propogate expected stars normalized coordinates
                net.BackProp(new float[] { normalizedExpectedX, normalizedExpectedY, normalizedExpectedZ });
            }
        }

        public static int? GetCurrentStarId(int? id)
        {
            string query1 = $@"
select Min(id)
from Discovered_Systems
where id > {id}";

            int? nextId = null;

            SqlConnection con = DataRefresh.GetConnection();

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query1, con);
                IAsyncResult result = cmd.BeginExecuteReader();

                while (!result.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }

                using (SqlDataReader reader = cmd.EndExecuteReader(result))
                {
                    while(reader.Read())
                    {
                        nextId = reader.GetInt32(0);
                    }
                }

                cmd.Dispose();
            }
            finally
            {
                con.Close();
            }

            return nextId;
        }

        public static Star GetCurrentStar(int? id)
        {
            Star star = null;

            SqlConnection con = DataRefresh.GetConnection();

            string query = $@"
select *
from Discovered_Systems
where id = {id}";

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                IAsyncResult result = cmd.BeginExecuteReader();

                while (!result.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }

                using (SqlDataReader reader = cmd.EndExecuteReader(result))
                {
                    while (reader.Read())
                    {
                        if (reader.FieldCount == 6)
                        {
                            Dictionary<string, double> coords = new Dictionary<string, double>
                            {
                                { "x", reader.GetDouble(2) },
                                { "y", reader.GetDouble(3) },
                                { "z", reader.GetDouble(4) },
                            };
                            star = new Star(reader.GetInt32(0), null, reader.GetString(1), coords, reader.GetDateTime(5));
                        }
                    }
                }

                cmd.Dispose();
            }
            finally
            {
                con.Close();
            }

            return star;
        }

        public static List<Star> GetStarsWithinDistance(Star star, Dictionary<string, double> minMax)
        {
            string query = $@"
select *
from Discovered_Systems
where x_coor between {minMax["minX"]} and {minMax["maxX"]}
and y_coor between {minMax["minY"]} and {minMax["maxY"]}
and z_coor between {minMax["minZ"]} and {minMax["maxZ"]}
and date_discovered < '{star.Date}'
";

            List<Star> stars = new List<Star>();

            SqlConnection con = DataRefresh.GetConnection();

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                IAsyncResult result = cmd.BeginExecuteReader();

                while (!result.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }

                using (SqlDataReader reader = cmd.EndExecuteReader(result))
                {
                    while (reader.Read())
                    {
                        Dictionary<string, double> coords = new Dictionary<string, double>
                                {
                                    { "x", reader.GetDouble(2) },
                                    { "y", reader.GetDouble(3) },
                                    { "z", reader.GetDouble(4) },
                                };
                        Star foundStar = new Star(reader.GetInt32(0), null, reader.GetString(1), coords, reader.GetDateTime(5));

                        stars.Add(foundStar);
                    }
                }

                cmd.Dispose();
            }
            finally
            {
                con.Close();
            }

            return stars;
        }

        public static double CalculateDistance(Star currentStar, Star otherStar)
        {
            double distance = Math.Sqrt(Math.Pow(otherStar.Coords["x"] - currentStar.Coords["x"], 2) + Math.Pow(otherStar.Coords["y"] - currentStar.Coords["y"], 2) + Math.Pow(otherStar.Coords["z"] - currentStar.Coords["z"], 2));

            return distance;
        }

        public static Dictionary<string, double> CalculateMinMaxCoords(Star star, double distance)
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

        public static Dictionary<int, Tuple<double, double, double>> PartitionCube(int amountOfCubes, Dictionary<string, double> minMaxCoordinates)
        {
            double cubeRoot = Math.Pow(amountOfCubes, 1.0 / 3.0);

            double xSplitLength = Math.Abs(minMaxCoordinates["maxX"] - minMaxCoordinates["minX"]) / cubeRoot;
            double ySplitLength = Math.Abs(minMaxCoordinates["maxY"] - minMaxCoordinates["minY"]) / cubeRoot;
            double zSplitLength = Math.Abs(minMaxCoordinates["maxZ"] - minMaxCoordinates["minZ"]) / cubeRoot;

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

            for (var i = 1; i < cubeRoot; i++)
            {
                newCubeXLimits.Add(i, newCubeXLimits[i - 1] + xSplitLength);
                newCubeYLimits.Add(i, newCubeYLimits[i - 1] + ySplitLength);
                newCubeZLimits.Add(i, newCubeZLimits[i - 1] + zSplitLength);
            }

            Dictionary<int, Tuple<double, double, double>> newCubes = new Dictionary<int, Tuple<double, double, double>>();

            int counter = 0;

            for (var i = 0; i < newCubeXLimits.Count(); i++)
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
