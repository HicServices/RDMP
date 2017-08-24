using System;
using System.Linq;
using DataExportLibrary.Data.DataTables;
using NUnit.Framework;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    [Category("Database")]
    public class DataUserTests : DatabaseTests
    {
        private const int testProjectID = 18;

        [Test]
        public void CreateNewDataUser_ChangeAndSave_AllValid()
        {
            var dataUser = new DataUser(DataExportRepository, "Captain", "America");
            try
            {
                Assert.AreEqual(dataUser.Forename, "Captain");
                Assert.AreEqual(dataUser.Surname, "America");

                dataUser.Forename = "Capn'";
                dataUser.Email = "fish@aol.com";
                dataUser.SaveToDatabase();
                
                DataUser userAfter = DataExportRepository.GetObjectByID<DataUser>(dataUser.ID);

                Assert.AreEqual(userAfter.Forename, "Capn'");
                Assert.AreEqual(userAfter.Surname,"America");
                Assert.AreEqual(userAfter.Email, "fish@aol.com");
            }
            finally
            {
                dataUser.DeleteInDatabase();
            }
        }

        [Test]
        [Ignore("Requires test data (for 'testProject')")]
        public void CreateNewDataUser_RegisterOnProject_CountRegisteredToProjectIncreases()
        {
            DataUser user = new DataUser(DataExportRepository, "unit", "test");
            Project testProject = DataExportRepository.GetObjectByID<Project>(testProjectID);
            try
            {
                int countBefore = testProject.DataUsers.Count();
            
                user.RegisterAsDataUserOnProject(testProject);

                int countAfter = testProject.DataUsers.Count();

                Assert.AreEqual(countAfter,countBefore + 1);

                user.UnRegisterAsDataUserOnProject(testProject);

                int finalCount = testProject.DataUsers.Count();

                Assert.AreEqual(countBefore, finalCount);
            }
            finally
            {
                try
                {
                    user.DeleteInDatabase();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Delete also failed with exception =" + e);
                }
            }

            
        }
    }
}
