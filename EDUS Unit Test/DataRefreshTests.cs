using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDUS.Models;
using System.Data;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;

namespace EDUS_Unit_Test
{
    [TestClass]
    public class DataRefreshTests
    {
        [TestMethod]
        public void ConnectionCanOpenAndClose_ConnectionGet_ConnectionOpensThenCloses()
        {
            // Arrange
            SqlConnection con = DataRefresh.GetConnection();

            // Act
            con.Open();

            // Assert
            Assert.IsTrue(con.State == ConnectionState.Open);

            // Act
            con.Close();

            // Assert
            Assert.IsTrue(con.State == ConnectionState.Closed);
        }

        [TestMethod]
        public void InsertUpdateInsertOneNew_InsertNewStar_OneStarAdded()
        {
            // Arrange
            List<Star> stars = new List<Star>();

            Star star1 = new Star(2, null, "testSystem2", new Dictionary<string, double>{
                { "x", 1.2 }, { "y", 1 }, { "z", 0.2 } }, DateTime.Now);

            stars.Add(star1);

            // Act
            DataRefresh.InsertUpdateStars(stars);

            // Assert
            Assert.IsTrue(SelectStarsName(2) == "testSystem2");
            InsertUpdateInsertTwoUpdateOne_UpdateStarAndAddNewStar_TwoRowsAddedOneUpdated();
        }

        [TestMethod]
        public void InsertUpdateInsertTwoUpdateOne_UpdateStarAndAddNewStar_TwoRowsAddedOneUpdated()
        {
            // Arrange
            List<Star> stars = new List<Star>();
            Star star1 = new Star(2, null, "testSystem2Updated", new Dictionary<string, double>{
                { "x", 1.2 }, { "y", 1 }, { "z", 0.2 } }, DateTime.Now);

            Star star2 = new Star(4, null, "testSystem4", new Dictionary<string, double>{
                { "x", 1.2 }, { "y", 1 }, { "z", 0.2 } }, DateTime.Now);

            Star star3 = new Star(5, null, "testSystem5", new Dictionary<string, double>{
                { "x", 1.2 }, { "y", 1 }, { "z", 0.2 } }, DateTime.Now);

            stars.Add(star1);
            stars.Add(star2);
            stars.Add(star3);

            // Act
            DataRefresh.InsertUpdateStars(stars);

            // Assert
            Assert.IsTrue(SelectStarsName(2) == "testSystem2Updated" && SelectStarsName(4) == "testSystem4" && SelectStarsName(5) == "testSystem5");
            DeleteStarsMultipleDeleteSucceeds_DeleteMultipleStars_TwoRowDeleted();
        }

        [TestMethod]
        public void DeleteStarsMultipleDeleteSucceeds_DeleteMultipleStars_TwoRowDeleted()
        {
            // Arrange
            List<int> ids = new List<int> { 5, 4, 2 };

            // Act
            DataRefresh.DeleteStars(ids);

            // Assert
            Assert.IsTrue(SelectStarsName(2) == "" && SelectStarsName(4) == "" && SelectStarsName(5) == "");
        }

        public static int? SelectStarsCount()
        {
            // Open connection to DB
            SqlConnection con = DataRefresh.GetConnection();
            con.Open();

            // Execute staging insert command
            using SqlCommand sqlCommand = new SqlCommand("select COUNT(*) from discovered_systems", con);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            int? result = null;

            while(reader.Read())
            {
                result = (int?)reader[0];
            }

            return result;
        }

        public static string SelectStarsName(int id)
        {
            // Open connection to DB
            SqlConnection con = DataRefresh.GetConnection();
            con.Open();

            // Execute staging insert command
            using SqlCommand sqlCommand = new SqlCommand($"select name from discovered_systems where id = {id}", con);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            string result = "";

            while (reader.Read())
            {
                result = reader[0].ToString();
            }

            return result;
        }
    }
}
