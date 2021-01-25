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
        Int64 loops = 1000;
        int cubes = 100;
        public void Train()
        {

            NeuralNetwork net = new NeuralNetwork(new int[] { cubes + 4, 50, 50, 50, 3 });

            int currentId = 0;

            for (var trainingLoop = 0; trainingLoop < loops; trainingLoop++)
            {
                // Get a random distance between 0.1 and 80
                double distance = new Random().NextDouble() * (80 - 0.1) + 0.1;

                // Get the star to be used as the correct answer (expected) for the training data
                Star expectedStar = GetCurrentStar(currentId);

                // Set the currentId to the current stars ID so we can select the next star on the next loop
                currentId = expectedStar.Id;

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

                for (var i = 0; i < miniCubes.Count(); i++)
                {
                    int count = 0;

                    foreach (Star s in stars)
                    {
                        if (s.Coords["x"] < miniCubes[i + 1].Item1 && s.Coords["x"] >= miniCubes[i].Item1 && s.Coords["y"] < miniCubes[i + 1].Item2 && s.Coords["y"] >= miniCubes[i].Item2 && s.Coords["z"] < miniCubes[i + 1].Item3 && s.Coords["z"] >= miniCubes[i].Item3)
                        {
                            count++;
                            stars.Remove(s);
                        }
                    }

                    // Normalize count to be between 0 and 1
                    float normalizedCount = count / stars.Count();

                    starCounts.Add(i, normalizedCount);
                }

                // Normalize center star coordinates and distance
                float normalizedX = (float)((star.Coords["x"] + 40000) / 80000);
                float normalizedY = (float)((star.Coords["y"] + 10000) / 70000);
                float normalizedZ = (float)((star.Coords["z"] + 11000) / 20000);
                float normalizedDistance = (float)((distance + 0.1) / 79.9);

                // Creat input array
                float[] inputs = new float[104];

                inputs[0] = normalizedX;
                inputs[1] = normalizedY;
                inputs[2] = normalizedZ;
                inputs[3] = normalizedDistance;
                for(var i = 0; i < starCounts.Count(); i++)
                {
                    inputs[i + 4] = starCounts[i];
                }

                // Input normalized fields
                net.FeedForward(inputs);

                // Normalize expected star coordinates
                float normalizedExpectedX = (float)((expectedStar.Coords["x"] + 40000) / 80000);
                float normalizedExpectedY = (float)((expectedStar.Coords["y"] + 10000) / 70000);
                float normalizedExpectedZ = (float)((expectedStar.Coords["z"] + 11000) / 20000);

                // Back propogate expected stars normalized coordinates
                net.BackProp(new float[] { normalizedExpectedX, normalizedExpectedY, normalizedExpectedZ });
            }
        }

        public Star GetCurrentStar(int id)
        {
            string query1 = $@"
select Min(id)
from Discovered_Systems
where id > {id}";

            int? nextId = null;

            string query2 = $@"
select *
from Discovered_Systems
where id = {nextId}";

            Star star = null;

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

                if (nextId != null)
                {
                    SqlCommand cmd2 = new SqlCommand(query2, con);
                    IAsyncResult result2 = cmd.BeginExecuteReader();

                    while (!result2.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    using (SqlDataReader reader = cmd.EndExecuteReader(result2))
                    {
                        while (reader.Read())
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

                    cmd2.Dispose();
                }
            }
            finally
            {
                con.Close();
            }

            return star;
        }

        public List<Star> GetStarsWithinDistance(Star star, Dictionary<string, double> minMax)
        {
            string query = $@"
select *
from Discovered_Systems
where x_coor between {minMax["minX"]} and {minMax["maxX"]}
and y_coor between {minMax["minY"]} and {minMax["maxY"]}
and z_coor between {minMax["minZ"]} and {minMax["maxZ"]}
and discovered_date < {star.Date}
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

            for (var i = 0; i < Math.Pow(amountOfCubes, (double)1 / 3); i++)
            {
                newCubeXLimits.Add(i + 1, newCubeXLimits[i] + xSplitLength);
                newCubeYLimits.Add(i + 1, newCubeYLimits[i] + ySplitLength);
                newCubeZLimits.Add(i + 1, newCubeZLimits[i] + zSplitLength);
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
