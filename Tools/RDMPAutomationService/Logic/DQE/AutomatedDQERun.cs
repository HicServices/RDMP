using System;
using System.Threading;
using CatalogueLibrary.Data;

using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Migration;
using DataQualityEngine.Reports;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.DQE
{
    /// <summary>
    /// Automation task that runs a the Data Quality Engine on a single Catalogue.
    /// </summary>
    internal class AutomatedDQERun
    {
        private readonly Catalogue _catalogueToRun;
        
        public AutomatedDQERun(DqeOptions options)
        {
            _catalogueToRun = options.GetRepositoryLocator().CatalogueRepository.GetObjectByID<Catalogue>(options.Catalogue);
        }

        public void RunTask()
        {
            new CatalogueConstraintReport(_catalogueToRun, SpecialFieldNames.DataLoadRunID).GenerateReport(_catalogueToRun, new ToMemoryDataLoadEventListener(true), new CancellationToken());
        }
    }
}