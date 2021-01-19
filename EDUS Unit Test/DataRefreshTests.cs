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
        public Dictionary<int, Tuple<string, double, double, double, DateTime>> systems = new Dictionary<int, Tuple<string, double, double, double, DateTime>>();
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
        public void FullRefreshSingleInsertSucceeds_MergeSingleWithInsert_OneRowInserted()
        {
            // Arrange
            systems.Clear();
            Tuple<string, double, double, double, DateTime> systemDetails = new Tuple<string, double, double, double, DateTime>("testSystem", 1.2, 1, 0.2, DateTime.Now);
            systems.Add(1, systemDetails);

            // Act
            DataRefresh.FullRefresh(systems);

            // Assert
            Assert.IsTrue(SelectSystemsCount() == 1);
            FullRefreshSingleUpdateSucceeds_MergeSingleWithUpdate_OneRowUpdated();
        }

        [TestMethod]
        public void FullRefreshSingleUpdateSucceeds_MergeSingleWithUpdate_OneRowUpdated()
        {
            // Arrange
            systems.Clear();
            Tuple<string, double, double, double, DateTime> systemDetails = new Tuple<string, double, double, double, DateTime>("testSystemUpdated", 1.2, 1, 0.2, DateTime.Now);
            systems.Add(1, systemDetails);

            // Act
            DataRefresh.FullRefresh(systems);

            // Assert
            Assert.IsTrue(SelectSystemsCount() == 1 && SelectSystemsName(1) == "testSystemUpdated");
            FullRefreshSingleDeleteSucceeds_MergeSingleWithDelete_OneRowDeleted();
        }

        [TestMethod]
        public void FullRefreshSingleDeleteSucceeds_MergeSingleWithDelete_OneRowDeleted()
        {
            // Arrange
            systems.Clear();

            // Act
            DataRefresh.FullRefresh(systems);

            // Assert
            Assert.IsTrue(SelectSystemsCount() == 0);
            FullRefreshMultipleInsertSucceeds_MergeMultipleWithInsert_ThreeRowsInserted();
        }

        [TestMethod]
        public void FullRefreshMultipleInsertSucceeds_MergeMultipleWithInsert_ThreeRowsInserted()
        {
            // Arrange
            systems.Clear();
            Tuple<string, double, double, double, DateTime> system1Details = new Tuple<string, double, double, double, DateTime>("testSystem1", 1.2, 1, 0.2, DateTime.Now);
            Tuple<string, double, double, double, DateTime> system2Details = new Tuple<string, double, double, double, DateTime>("testSystem2", 1.2, 1, 0.2, DateTime.Now);
            Tuple<string, double, double, double, DateTime> system3Details = new Tuple<string, double, double, double, DateTime>("testSystem3", 1.2, 1, 0.2, DateTime.Now);
            systems.Add(1, system1Details);
            systems.Add(2, system2Details);
            systems.Add(3, system3Details);

            // Act
            DataRefresh.FullRefresh(systems);

            // Assert
            Assert.IsTrue(SelectSystemsCount() == 3);
            FullRefreshMultipleUpdateSucceeds_MergeMultipleWithUpdate_TwoRowsUpdatedOneStaysTheSame();
        }

        [TestMethod]
        public void FullRefreshMultipleUpdateSucceeds_MergeMultipleWithUpdate_TwoRowsUpdatedOneStaysTheSame()
        {
            // Arrange
            systems.Clear();
            Tuple<string, double, double, double, DateTime> system1Details = new Tuple<string, double, double, double, DateTime>("testSystem1Updated", 1.2, 1, 0.2, DateTime.Now);
            Tuple<string, double, double, double, DateTime> system2Details = new Tuple<string, double, double, double, DateTime>("testSystem2Updated", 1.2, 1, 0.2, DateTime.Now);
            Tuple<string, double, double, double, DateTime> system3Details = new Tuple<string, double, double, double, DateTime>("testSystem3", 1.2, 1, 0.2, DateTime.Now);
            systems.Add(1, system1Details);
            systems.Add(2, system2Details);
            systems.Add(3, system3Details);

            // Act
            DataRefresh.FullRefresh(systems);

            // Assert
            Assert.IsTrue(SelectSystemsCount() == 3 && SelectSystemsName(1) == "testSystem1Updated" && SelectSystemsName(2) == "testSystem2Updated" && SelectSystemsName(3) == "testSystem3");
            FullRefreshMultipleDeleteSucceeds_MergeMultipleWithDelete_TwoRowsDeletedOneStays();
        }

        [TestMethod]
        public void FullRefreshMultipleDeleteSucceeds_MergeMultipleWithDelete_TwoRowsDeletedOneStays()
        {
            // Arrange
            systems.Clear();
            Tuple<string, double, double, double, DateTime> system2Details = new Tuple<string, double, double, double, DateTime>("testSystem2Updated", 1.2, 1, 0.2, DateTime.Now);
            systems.Add(2, system2Details);

            // Act
            DataRefresh.FullRefresh(systems);
            
            // Assert
            Assert.IsTrue(SelectSystemsCount() == 1 && SelectSystemsName(2) == "testSystem2Updated");
            InsertUpdateInsertTwoUpdateOne_UpdateSystemAndAddNewSystems_TwoRowsAddedOneUpdated();
        }

        [TestMethod]
        public void InsertUpdateInsertTwoUpdateOne_UpdateSystemAndAddNewSystems_TwoRowsAddedOneUpdated()
        {
            // Arrange
            systems.Clear();
            Tuple<string, double, double, double, DateTime> system2Details = new Tuple<string, double, double, double, DateTime>("testSystem2UpdatedAgain", 1.2, 1, 0.2, DateTime.Now);
            Tuple<string, double, double, double, DateTime> system4Details = new Tuple<string, double, double, double, DateTime>("testSystem4", 1.2, 1, 0.2, DateTime.Now);
            Tuple<string, double, double, double, DateTime> system5Details = new Tuple<string, double, double, double, DateTime>("testSystem5", 1.2, 1, 0.2, DateTime.Now);
            systems.Add(2, system2Details);
            systems.Add(4, system4Details);
            systems.Add(5, system5Details);

            // Act
            DataRefresh.InsertUpdateStars(systems);

            // Assert
            Console.WriteLine(SelectSystemsCount());
            Console.WriteLine(SelectSystemsName(2));
            Assert.IsTrue(SelectSystemsCount() == 3 && SelectSystemsName(2) == "testSystem2UpdatedAgain");
            DeleteSystemsMultipleDeleteSucceeds_DeleteMultipleSystems_TwoRowDeleted();
        }

        [TestMethod]
        public void DeleteSystemsMultipleDeleteSucceeds_DeleteMultipleSystems_TwoRowDeleted()
        {
            // Arrange
            systems.Clear();
            List<int> ids = new List<int>();
            ids.Add(5);
            ids.Add(4);
            ids.Add(2);

            // Act
            DataRefresh.DeleteStars(ids);

            // Assert
            Assert.IsTrue(SelectSystemsCount() == 0);
        }

        public static int? SelectSystemsCount()
        {
            // Open connection to DB
            SqlConnection con = DataRefresh.GetConnection();
            con.Open();

            // Execute staging insert command
            SqlCommand sqlCommand = new SqlCommand("select COUNT(*) from discovered_systems", con);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            int? result = null;

            while(reader.Read())
            {
                result = (int?)reader[0];
            }

            return result;
        }

        public static string SelectSystemsName(int id)
        {
            // Open connection to DB
            SqlConnection con = DataRefresh.GetConnection();
            con.Open();

            // Execute staging insert command
            SqlCommand sqlCommand = new SqlCommand($"select name from discovered_systems where id = {id}", con);
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
