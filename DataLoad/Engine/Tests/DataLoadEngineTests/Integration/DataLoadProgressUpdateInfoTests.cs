using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using DataLoadEngine.Job.Scheduling;
using HIC.Logging;
using LoadModules.Generic.LoadProgressUpdating;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class DataLoadProgressUpdateInfoTests :DatabaseTests
    {
        private ScheduledDataLoadJob _job;

        #region Setup Methods
        public DataLoadProgressUpdateInfoTests()
        {
            ICatalogue cata = MockRepository.GenerateMock<ICatalogue>();
            cata.Expect(p => p.LoggingDataTask).Return("NothingTask");

            cata.Expect(m => m.GetTableInfoList(false)).Return(new TableInfo[0]);
            cata.Expect(m => m.GetLookupTableInfoList()).Return(new TableInfo[0]);

            var lmd = MockRepository.GenerateMock<ILoadMetadata>();
            lmd.Expect(m => m.GetAllCatalogues()).Return(new[] { cata });

            _job = new ScheduledDataLoadJob("fish", MockRepository.GenerateMock<ILogManager>(), lmd, null, new ThrowImmediatelyDataLoadJob());
        }
        #endregion

        [Test]
        public void AddBasicNormalStrategy_NoDates()
        {
            var updateInfo = new DataLoadProgressUpdateInfo();
            var ex = Assert.Throws<DataLoadProgressUpdateException>(()=>updateInfo.AddAppropriateDisposeStep(_job,null));
            Assert.IsTrue(ex.Message.StartsWith("Job does not have any DatesToRetrieve"));
        }

        [Test]
        public void AddBasicNormalStrategy_MaxDate()
        {
            var updateInfo = new DataLoadProgressUpdateInfo();
            Assert.AreEqual(DataLoadProgressUpdateStrategy.UseMaxRequestedDay,updateInfo.Strategy);

            _job.DatesToRetrieve = new List<DateTime>();
            _job.DatesToRetrieve.Add(new DateTime(2001,1,1));
            _job.DatesToRetrieve.Add(new DateTime(2001, 1, 2));
            _job.DatesToRetrieve.Add(new DateTime(2001, 1, 3));
            try
            {
                var added = (UpdateProgressIfLoadsuccessful)updateInfo.AddAppropriateDisposeStep(_job, null);
                
                
                Assert.AreEqual(new DateTime(2001, 1, 3), added.DateToSetProgressTo);
            }
            finally
            {
                _job.DatesToRetrieve.Clear();    
            }
            
        }

        [Test]
        public void AddRAWSQLStrategy_NoSQL()
        {
            var updateInfo = new DataLoadProgressUpdateInfo();
            updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
            var ex = Assert.Throws<Exception>(()=>updateInfo.AddAppropriateDisposeStep(_job, DiscoveredDatabaseICanCreateRandomTablesIn));

            Assert.IsTrue(ex.Message.StartsWith("Strategy is ExecuteScalarSQLInRAW but there is no ExecuteScalarSQL"));
        }

        [Test]
        public void AddRAWSQLStrategy_SQLDodgy_SqlCrashes()
        {
            var updateInfo = new DataLoadProgressUpdateInfo();
            updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
            updateInfo.ExecuteScalarSQL = "SELECT Top 1 BarrelORum from CaptainMorgansSpicedRumBarrel";
            var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, DiscoveredDatabaseICanCreateRandomTablesIn));

            Assert.IsTrue(ex.Message.StartsWith("Failed to execute the following SQL in the RAW database"));
            Assert.IsInstanceOf<SqlException>(ex.InnerException);
        }

        [Test]
        public void AddRAWSQLStrategy_SQLDodgy_SqlReturnsNull()
        {
            var updateInfo = new DataLoadProgressUpdateInfo();
            updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
            updateInfo.ExecuteScalarSQL = "SELECT null";
            var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, DiscoveredDatabaseICanCreateRandomTablesIn));

            Assert.IsTrue(ex.Message.Contains("ExecuteScalarSQL"));
            Assert.IsTrue(ex.Message.Contains("returned null"));
        }

        [Test]
        public void AddRAWSQLStrategy_SQLDodgy_SqlReturnsNonDate()
        {
            var updateInfo = new DataLoadProgressUpdateInfo();
            updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            
            updateInfo.ExecuteScalarSQL = "SELECT 'fishfish'";
            var ex = Assert.Throws<DataLoadProgressUpdateException>(() => updateInfo.AddAppropriateDisposeStep(_job, DiscoveredDatabaseICanCreateRandomTablesIn));

            Assert.AreEqual("ExecuteScalarSQL specified for determining the maximum date of data loaded returned a value that was not a Date:fishfish",ex.Message);
            Assert.IsInstanceOf<FormatException>(ex.InnerException);
        }

        [Test]
        public void AddRAWSQLStrategy_SQLCorrect()
        {
            _job.DatesToRetrieve = new List<DateTime>();
            _job.DatesToRetrieve.Add(new DateTime(2001,1,6));
            _job.DatesToRetrieve.Add(new DateTime(2001,1,7));
            _job.DatesToRetrieve.Add(new DateTime(2001,1,8));

            var updateInfo = new DataLoadProgressUpdateInfo();
            updateInfo.Strategy = DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            updateInfo.ExecuteScalarSQL = "SELECT '2001-01-07'";

            var added = (UpdateProgressIfLoadsuccessful)updateInfo.AddAppropriateDisposeStep(_job, DiscoveredDatabaseICanCreateRandomTablesIn);
            
            Assert.AreEqual(new DateTime(2001, 1, 7), added.DateToSetProgressTo);

            _job.DatesToRetrieve.Clear();
        }
    }
}
