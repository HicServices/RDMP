using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using DataExportLibrary.Data.DataTables;

namespace CatalogueManager.ItemActivation.Arranging
{
    public interface IArrangeWindows
    {
        void SetupEditCatalogue(object sender, Catalogue catalogue);
        void SetupEditCohortIdentificationConfiguration(object sender, CohortIdentificationConfiguration cohortIdentificationConfiguration);
        void SetupEditDataExtractionProject(object sender, Project project);
        void SetupEditLoadMetadata(object sender, LoadMetadata loadMetadata);
    }
}
