using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDUS.Models;
using System.Data;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;

namespace EDUS_Unit_Test
{
    [TestClass]
    public class NeuralNetworkTrainingTests
    {
        [TestMethod]
        public void test()
        {
            Training.Train();

            Assert.IsTrue(1 == 1);
        }
        
    }
}
