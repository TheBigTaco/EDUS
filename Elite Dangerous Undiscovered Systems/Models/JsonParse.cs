using System;
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
        /// Loads file for the full refresh from json file into database
        /// </summary>
        /// <param name="fileName">string of filename of stars to be deserialized</param>
        public static void LoadAllStarsFromJson(string fileName)
        {
            DataRefresh.FullRefresh(JsonToStarDictionary(fileName));
        }

        /// <summary>
        /// Loads file for the nightly update from json file into database
        /// </summary>
        /// <param name="fileName"></param>
        public static void LoadNightlyStarsFromJson(string fileName)
        {
            DataRefresh.InsertUpdateStars(JsonToStarDictionary(fileName));
        }

        /// <summary>
        /// Converts deserialized json into dictionary of ID and Tuple key value pair. 
        /// (I'll probably remove this later, to just use a list of star objects everywhere instead of this awkward dictionary)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Dictionary<int, Tuple<string, double, double, double, DateTime>> JsonToStarDictionary(string fileName)
        {
            List<Star> stars = DeserializeJsonStars(fileName);

            Dictionary<int, Tuple<string, double, double, double, DateTime>> allStars = new Dictionary<int, Tuple<string, double, double, double, DateTime>>();

            foreach (Star star in stars)
            {
                Tuple<string, double, double, double, DateTime> details = new Tuple<string, double, double, double, DateTime>(star.name, star.coords["x"], star.coords["y"], star.coords["z"], star.date);
                allStars.Add(star.id, details);
            }

            return allStars;
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
