using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using NUnit.Framework;
using RDMPStartup;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class AutomationTests:DatabaseTests
    {
        protected Action<EventLogEntryType, string> logAction = ((type, s) => { Console.WriteLine("{0}: {1}", type.ToString().ToUpper(), s); });

        protected MockAutomationServiceOptions mockOptions;

        [TestFixtureSetUp]
        public void ClearSlotsAndJobs()
        {
            foreach (AutomateablePipeline automateablePipeline in CatalogueRepository.GetAllObjects<AutomateablePipeline>())
            {
                var pipe = automateablePipeline.Pipeline;
                automateablePipeline.DeleteInDatabase();
                pipe.DeleteInDatabase();
            }

            foreach (var job in CatalogueRepository.GetAllObjects<AutomationJob>())
                job.DeleteInDatabase();

            foreach (var slot in CatalogueRepository.GetAllObjects<AutomationServiceSlot>())
            {
                slot.Unlock();
                slot.DeleteInDatabase();
            }

            foreach (var ex in CatalogueRepository.GetAllObjects<AutomationServiceException>())
                ex.DeleteInDatabase();

            foreach (LoadPeriodically loadPeriodically in CatalogueRepository.GetAllObjects<LoadPeriodically>())
                loadPeriodically.DeleteInDatabase();
            
            mockOptions = new MockAutomationServiceOptions(RepositoryLocator)
            {
                ServerName = _serverName,
                CatalogueDatabaseName = TestDatabaseNames.GetConsistentName("Catalogue"),
                DataExportDatabaseName = TestDatabaseNames.GetConsistentName("DataExport"),
                ForceSlot = 0
            };
        }
        
    }
}
