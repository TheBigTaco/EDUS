using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
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
            DataRefresh.BulkRefresh(DeserializeJsonFullRefresh(fileName));
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
            string line;
            List<Star> stars = new List<Star>();
            using (StreamReader file = File.OpenText(fileLocation + fileName))
            {
                while((line = file.ReadLine()) != null)
                {
                    if(!line.Contains("[") && !line.Contains("]"))
                    {
                        Star star = JsonConvert.DeserializeObject<Star>(line.TrimEnd(','));
                        stars.Add(star);
                    }
                }
                
            }

            return stars;
        }

        /// <summary>
        /// Reads a large Json file and deserializes it into a data table for the full refresh
        /// </summary>
        /// <param name="fileName">string of filename of stars to be deserialized</param>
        /// <returns>data table of stars</returns>
        public static DataTable DeserializeJsonFullRefresh(string fileName)
        {
            string fileLocation = "C:\\Users\\Adam\\source\\repos\\Elite Dangerous Undiscovered Systems\\Elite Dangerous Undiscovered Systems\\NightlyDumps\\";
            string line;
            DataTable dt = new DataTable();
            dt.Columns.Add("id");
            dt.Columns.Add("name");
            dt.Columns.Add("x_coor");
            dt.Columns.Add("y_coor");
            dt.Columns.Add("z_coor");
            dt.Columns.Add("date_discovered");

            DataRefresh.ExecuteQuery("USE Elite_Dangerous; Truncate Table Discovered_Systems;");

            using (StreamReader file = File.OpenText(fileLocation + fileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.Contains("[") && !line.Contains("]"))
                    {
                        Star star = JsonConvert.DeserializeObject<Star>(line.TrimEnd(','));
                        DataRow dr = dt.NewRow();
                        dr["id"] = star.Id;
                        dr["name"] = star.Name;
                        dr["x_coor"] = star.Coords["x"];
                        dr["y_coor"] = star.Coords["y"];
                        dr["z_coor"] = star.Coords["z"];
                        dr["date_discovered"] = star.Date;
                        dt.Rows.Add(dr);
                        if(dt.Rows.Count == 1000000)
                        {
                            DataRefresh.BulkRefresh(dt);
                            dt.Rows.Clear();
                            Thread.Sleep(60000);
                        }
                    }
                }

            }

            return dt;
        }
    }

    public class Star
    {
        public int Id { get; set; }
        public Int64? Id64 { get; set; }
        public string Name { get; set; }
        public Dictionary<string,double> Coords { get; set; }
        public DateTime Date { get; set; }

        public Star(int id, Int64? id64, string name, Dictionary<string, double> coords, DateTime date)
        {
            Id = id;
            Id64 = id64;
            Name = name;
            Coords = coords;
            Date = date;
        }
    }
}
