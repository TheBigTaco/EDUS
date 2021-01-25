using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace EDUS.Models
{
    public class DataRefresh
    {
        // Fun super unsafe connection string that I'll make dynamic with env variables later
        readonly static private string connectionString = "Data Source=THEMASSIVETACO;Initial Catalog=Elite_Dangerous;Integrated Security=True;MultipleActiveResultSets=True;";

        public static void BulkRefresh(DataTable dt)
        {
            using SqlConnection con = GetConnection();

            using SqlBulkCopy bulkCopy = new SqlBulkCopy(con)
            {
                DestinationTableName = "Discovered_Systems",
                BulkCopyTimeout = 0
            };

            bulkCopy.ColumnMappings.Add("id", "id");
            bulkCopy.ColumnMappings.Add("name", "name");
            bulkCopy.ColumnMappings.Add("x_coor", "x_coor");
            bulkCopy.ColumnMappings.Add("y_coor", "y_coor");
            bulkCopy.ColumnMappings.Add("z_coor", "z_coor");
            bulkCopy.ColumnMappings.Add("date_discovered", "date_discovered");
            try
            {
                con.Open();
                bulkCopy.WriteToServer(dt);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// Insert and update for nightly dumps of new stars.
        /// </summary>
        /// <param name="stars">List of Star to be inserted or updated</Star></param>
        public static void InsertUpdateStars(List<Star> stars)
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
        /// Loop through List of Star to add stars into insert statement to be inserted into staging table
        /// </summary>
        /// <param name="stars">List of Star</param>
        public static void InsertIntoStaging(List<Star> stars)
        {
            if (stars.Count() > 0)
            {
                // Build staging table upsert query
                string stagingInsertStatement = @"
USE Elite_Dangerous;

INSERT INTO Discovered_Systems_Staging (id, name, x_coor, y_coor, z_coor, date_discovered)
VALUES ";
                foreach (Star star in stars)
                {
                    // Add in values for every system
                    stagingInsertStatement += @$"({star.Id}, '{star.Name}', {star.Coords["x"]}, {star.Coords["y"]}, {star.Coords["z"]}, '{star.Date}'),";
                }
                // Remove trailing comma on final value set
                stagingInsertStatement = stagingInsertStatement.Remove(stagingInsertStatement.Length - 1);

                ExecuteQuery(stagingInsertStatement);
            }
        }

        /// <summary>
        /// Execute SQL Insert/Delete/Update Query. No idea if this is a good way to do it, but it's what works.
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
                cmd.Dispose();
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
