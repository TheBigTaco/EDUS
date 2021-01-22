using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDUS.Models;
using System.Data;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;

namespace EDUS_Unit_Test
{
    [TestClass]
    public class JsonParseTests
    {
        [TestMethod]
        public void JsonDeserializationWorksProperly_NewFileToDeserialize_FileDeserializedIntoSystemObjects()
        {
            // Arrange
            string fileName = "systemsWithCoordinates_test.json";
            List<Star> stars;
            // Act
            stars = JsonParse.DeserializeJsonStars(fileName);

            // Assert
            Assert.IsTrue(stars[0].Id == 1 && stars[1].Id == 2312);
        }

        [TestMethod]
        public void JsonNightlyUploadWorks_NewNightlyFileToUpload_TwoNewSystemsInsertedIntoDatabaseFromJsonFile()
        {
            // Arrange
            string fileName = "systemsWithCoordinates_test.json";

            // Act
            JsonParse.LoadNightlyStarsFromJson(fileName);

            // Assert
            Assert.IsTrue(DataRefreshTests.SelectStarsName(1) == "testSystem" && DataRefreshTests.SelectStarsName(2312) == "testSystem2");
            List<int> ids = new List<int> { 1, 2312 };
            DataRefresh.DeleteStars(ids);
        }

        //[TestMethod]
        //public void JsonFullRefresh_FullStarRefreshRequired_UploadAllSixtyMillionRecords()
        //{
        //    // Arrange
        //    //string fileName = "systemsWithCoordinates_test.json";
        //    string fileName = "systemsWithCoordinates.json";

        //    // Act
        //    JsonParse.LoadAllStarsFromJson(fileName);

        //    // Assert
        //    Assert.IsTrue(DataRefreshTests.SelectStarsCount() > 50000000);
        //    //Assert.IsTrue(DataRefreshTests.SelectStarsName(1) == "testSystem" && DataRefreshTests.SelectStarsName(2312) == "testSystem2");
        //    //List<int> ids = new List<int> { 1, 2312 };
        //    //DataRefresh.DeleteStars(ids);
        //}
    }
}
