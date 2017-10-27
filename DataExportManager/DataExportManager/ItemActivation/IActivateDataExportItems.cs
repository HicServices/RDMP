using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.Collections.Providers;
using DataExportManager.Menus;
using DataExportManager.ProjectUI;
using DataExportManager.ProjectUI.Graphs;

namespace DataExportManager.ItemActivation
{
    public interface IActivateDataExportItems:IActivateItems
    {
        void ActivateExternalCohortTable(object sender, ExternalCohortTable externalCohortTable);
        void ActivateCohort(object sender, ExtractableCohort cohort);
        void ActivateProject(object sender, Project project);
        void ExecuteExtractionConfiguration(object sender, ExecuteExtractionUIRequest settings);
        void ExecuteRelease(object sender, Project project);

        void ActivateEditExtractionConfigurationDataset(SelectedDataSets selectedDataSet);
        void ActivateViewExtractionSQL(object sender, SelectedDataSets selectedDataSet);

        void ExecuteExtractionExtractionAggregateGraph(object sender, ExtractionAggregateGraphObjectCollection objectCollection);
    }
}
