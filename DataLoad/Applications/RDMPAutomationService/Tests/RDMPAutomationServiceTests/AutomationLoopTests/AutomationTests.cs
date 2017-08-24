using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using NUnit.Framework;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class AutomationTests:DatabaseTests
    {
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
        }
        
    }
}
