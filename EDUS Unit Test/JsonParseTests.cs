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
            Assert.IsTrue(stars[0].id == 1 && stars[1].id == 2312);
        }

        [TestMethod]
        public void JsonNightlyUploadWorks_NewNightlyFileToUpload_TwoNewSystemsInsertedIntoDatabaseFromJsonFile()
        {
            // Arrange
            string fileName = "systemsWithCoordinates_test.json";

            // Act
            JsonParse.LoadNightlyStarsFromJson(fileName);

            // Assert
            Assert.IsTrue(DataRefreshTests.SelectSystemsName(1) == "testSystem" && DataRefreshTests.SelectSystemsName(2312) == "testSystem2" && DataRefreshTests.SelectSystemsCount() == 2);
        }

        [TestMethod]
        public void JsonFullRefresh_FullStarRefreshRequired_UploadAllSixtyMillionRecords()
        {
            // Arrange
            //string fileName = "systemsWithCoordinates_test.json";
            string fileName = "systemsWithCoordinates.json";

            // Act
            JsonParse.LoadAllStarsFromJson(fileName);

            // Assert
            Assert.IsTrue(DataRefreshTests.SelectSystemsCount() > 50000000);
            //Assert.IsTrue(DataRefreshTests.SelectSystemsCount() == 2);
        }
    }
}
