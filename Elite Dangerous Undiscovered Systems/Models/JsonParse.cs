﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EDUS.Models
{
    public class JsonParse
    {
        /// <summary>
        /// Loads file for a full refresh from json file into database
        /// </summary>
        /// <param name="fileName">string of filename of stars to be deserialized</param>
        public static void LoadAllStarsFromJson(string fileName)
        {
            List<Star> stars = DeserializeJsonStars(fileName);
            List<Star> starsSplit = new List<Star>();
            for (var i = 0; i < stars.Count(); i++)
            {
                starsSplit.Add(stars[i]);
                if (i % 1000 == 0 || i + 1 == stars.Count())
                {
                    DataRefresh.InsertUpdateStars(starsSplit);
                    starsSplit.Clear();
                }
            }
        }

        /// <summary>
        /// Loads file for the nightly update from json file into database
        /// </summary>
        /// <param name="fileName"></param>
        public static void LoadNightlyStarsFromJson(string fileName)
        {
            DataRefresh.InsertUpdateStars(DeserializeJsonStars(fileName));
        }

        /// <summary>
        /// Reads a Json file and deserializes it into a list of stars
        /// </summary>
        /// <param name="fileName">string of filename of stars to be deserialized</param>
        /// <returns>A list of Star objects</returns>
        public static List<Star> DeserializeJsonStars(string fileName)
        {
            string fileLocation = "C:\\Users\\Adam\\source\\repos\\Elite Dangerous Undiscovered Systems\\Elite Dangerous Undiscovered Systems\\NightlyDumps\\";

            List<Star> stars = new List<Star>();
            using (StreamReader file = File.OpenText(fileLocation + fileName))
            {
                stars = JsonConvert.DeserializeObject<List<Star>>(file.ReadToEnd());
            }

            return stars;
        }
    }

    public class Star
    {
        public int id { get; set; }
        public int id64 { get; set; }
        public string name { get; set; }
        public Dictionary<string,double> coords { get; set; }
        public DateTime date { get; set; }
    }
}
