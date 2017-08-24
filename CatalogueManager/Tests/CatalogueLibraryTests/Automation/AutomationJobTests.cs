using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Automation
{
    public class AutomationJobTests : DatabaseTests
    {
        private AutomationServiceSlot _parent;
        private AutomationJob _instance;

        
        [SetUp]
        public void CreateInstance()
        {
            _parent = new AutomationServiceSlot(CatalogueRepository, AutomationFailureStrategy.TryNext,
              AutomationFailureStrategy.TryNext,AutomationFailureStrategy.TryNext, AutomationDQEJobSelectionStrategy.MostRecentlyLoadedDataset);

            _instance = _parent.AddNewJob(AutomationJobType.DLE, "Loading Fishes Into Barrels");
        }

        [TearDown]
        public void DeleteRemnants()
        {
            if(_instance.HasLocalChanges().Evaluation != ChangeDescription.DatabaseCopyWasDeleted)
                _instance.DeleteInDatabase();

            _parent.DeleteInDatabase();
        }

        [Test]
        public void TestChangingParameters()
        {

            _instance.AutomationJobType = AutomationJobType.DQE;
            
            var diff = _instance.HasLocalChanges();

            Assert.AreEqual(1, diff.Differences.Count());
            _instance.SaveToDatabase();
            Assert.AreEqual(0, _instance.HasLocalChanges().Differences.Count());

            var ex = Assert.Throws<SqlException>(_parent.DeleteInDatabase);
            Assert.IsTrue(ex.Message.Contains("FK_AutomationJob_AutomationServiceSlot"));
        }

        [Test]
        public void TestLockingACatalogue()
        {
            Catalogue cata = new Catalogue(CatalogueRepository, "MyCata");
            try
            {
                _instance.LockCatalogues(new []{cata});
            
                Assert.AreEqual(cata,_instance.GetLockedCatalogues().Single());
                Assert.AreEqual(cata, CatalogueRepository.GetAllAutomationLockedCatalogues().Single());
            }
            finally 
            {
                _instance.DeleteInDatabase();
                cata.DeleteInDatabase();
            }
        }


        [Test]
        public void TestLockingACatalogueTwice()
        {
            var cata = new Catalogue(CatalogueRepository, "MyCata");

            try
            {
                _instance.LockCatalogues(new[] { cata });
                var ex = Assert.Throws<SqlException>(() => _instance.LockCatalogues(new[] {cata}));
                Assert.IsTrue(ex.Message.Contains("ix_CataloguesCanOnlyBeLockedByOneAutomationJobAtATime"));
            }
            finally
            {
                _instance.DeleteInDatabase();
                cata.DeleteInDatabase();
            }
            
        }

    }
}
