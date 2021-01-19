﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace EDUS.Models
{
    public class DataRefresh
    {
        // Fun super unsafe connection string that I'll make dynamic with env variables later
        static private string connectionString = "Data Source=THEMASSIVETACO;Initial Catalog=Elite_Dangerous;Integrated Security=True";

        /// <summary>
        /// Fully refresh all stars in database from systemsWithCoordinates json
        /// </summary>
        /// <param name="stars"></param>
        public static void FullRefresh(Dictionary<int,Tuple<string, double, double, double, DateTime>> stars)
        {
            // Insert into staging first for merge statement
            InsertIntoStaging(stars);

            // Merge statement to merge staging and production tables
            string mergeStatement = @"
USE Elite_Dangerous;

MERGE Discovered_Systems AS t
USING Discovered_Systems_Staging AS s
ON t.id = s.id
WHEN MATCHED AND t.name != s.name OR t.x_coor != s.x_coor OR t.y_coor != s.y_coor OR t.z_coor != s.z_coor OR t.date_discovered != s.date_discovered
	THEN UPDATE SET 
	t.id = s.id
	, t.name = s.name
	, t.x_coor = s.x_coor
	, t.y_coor = s.y_coor
	, t.z_coor = s.z_coor
	, t.date_discovered = s.date_discovered
WHEN NOT MATCHED
	THEN INSERT VALUES (s.id, s.name, s.x_coor, s.y_coor, s.z_coor, s.date_discovered)
WHEN NOT MATCHED BY SOURCE
	THEN DELETE;

TRUNCATE TABLE Discovered_Systems_Staging;";

            // Execute merge
            ExecuteQuery(mergeStatement);
        }

        /// <summary>
        /// Insert and update for nightly dumps of new stars.
        /// </summary>
        /// <param name="stars"></param>
        public static void InsertUpdateStars(Dictionary<int, Tuple<string, double, double, double, DateTime>> stars)
        {

            InsertIntoStaging(stars);

            string updateStatement = @"
USE Elite_Dangerous;

MERGE Discovered_Systems AS t
USING Discovered_Systems_Staging AS s
ON t.id = s.id
WHEN MATCHED AND t.name != s.name OR t.x_coor != s.x_coor OR t.y_coor != s.y_coor OR t.z_coor != s.z_coor OR t.date_discovered != s.date_discovered
	THEN UPDATE SET 
	t.id = s.id
	, t.name = s.name
	, t.x_coor = s.x_coor
	, t.y_coor = s.y_coor
	, t.z_coor = s.z_coor
	, t.date_discovered = s.date_discovered
WHEN NOT MATCHED
	THEN INSERT VALUES (s.id, s.name, s.x_coor, s.y_coor, s.z_coor, s.date_discovered);

TRUNCATE TABLE Discovered_Systems_Staging;";

            // Execute merge
            ExecuteQuery(updateStatement);
        }

        /// <summary>
        /// Manually delete systems by ID
        /// </summary>
        /// <param name="ids">list of integer ids of stars</param>
        public static void DeleteStars(List<int> ids)
        {
            string deleteStatement = @"DELETE FROM discovered_systems WHERE id IN (";
            foreach (int id in ids)
            {
                deleteStatement += $"{id},";
            }
            // Remove trailing comma on final value set
            deleteStatement = deleteStatement.Remove(deleteStatement.Length - 1);
            // Close in statement parenthesis
            deleteStatement += ")";

            Console.WriteLine(deleteStatement);
            ExecuteQuery(deleteStatement);
        }

        /// <summary>
        /// Loop through dictionary to add stars into insert statement to be inserted into staging table
        /// </summary>
        /// <param name="stars">Dictionary of id, tuple keyvalue pair. Tuple contains name, coords, and date discovered.</param>
        public static void InsertIntoStaging(Dictionary<int, Tuple<string, double, double, double, DateTime>> stars)
        {
            if (stars.Count() > 0)
            {
                // Build staging table upsert query
                string stagingInsertStatement = @"
USE Elite_Dangerous;

INSERT INTO Discovered_Systems_Staging (id, name, x_coor, y_coor, z_coor, date_discovered)
VALUES ";
                foreach (KeyValuePair<int, Tuple<string, double, double, double, DateTime>> star in stars)
                {
                    // Add in values for every system
                    stagingInsertStatement += @$"({star.Key}, '{star.Value.Item1}', {star.Value.Item2}, {star.Value.Item3}, {star.Value.Item4}, '{star.Value.Item5}'),";
                }
                // Remove trailing comma on final value set
                stagingInsertStatement = stagingInsertStatement.Remove(stagingInsertStatement.Length - 1);

                ExecuteQuery(stagingInsertStatement);
            }
        }

        /// <summary>
        /// Execute SQL Query. No idea if this is a good way to do it, but it's what works.
        /// </summary>
        /// <param name="query">String containing SQL query</param>
        public static void ExecuteQuery(string query)
        {
            // Open connection to DB
            SqlConnection con = GetConnection();
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// Get connection using connection string
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
