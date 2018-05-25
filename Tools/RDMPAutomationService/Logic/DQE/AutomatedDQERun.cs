using System;
using System.Threading;
using CatalogueLibrary.Data;

using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Migration;
using DataQualityEngine.Reports;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.DQE
{
    /// <summary>
    /// Automation task that runs a the Data Quality Engine on a single Catalogue.
    /// </summary>
    public class AutomatedDQERun
    {
        private readonly Catalogue _catalogueToRun;
        
        public AutomatedDQERun(Catalogue catalogueToRun)
        {
            _catalogueToRun = catalogueToRun;
        }
        
        public void RunTask()
        {
            new CatalogueConstraintReport(_catalogueToRun, SpecialFieldNames.DataLoadRunID).GenerateReport(_catalogueToRun, new ToMemoryDataLoadEventListener(true), new CancellationToken());
        }
    }
}