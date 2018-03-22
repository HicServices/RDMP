using System;
using System.Diagnostics;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Providers.Nodes.UsedByProject;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.DataViewing.Collections;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractableCohortMenu:RDMPContextMenuStrip
    {
        private readonly ExtractableCohort _cohort;

        public ExtractableCohortMenu(RDMPContextMenuStripArgs args, ExtractableCohort cohort)
            : base(args,cohort)
        {
            _cohort = cohort;
            Items.Add("View TOP 100 identifiers",null, (s, e) => ViewTop100());

            Add(new ExecuteCommandImportFileAsCustomDataForCohort(_activator,cohort));

            Add(new ExecuteCommandImportPatientIndexTableAsCustomData(_activator, cohort));

        }

        public ExtractableCohortMenu(RDMPContextMenuStripArgs args, ExtractableCohortUsedByProjectNode cohortNode)
            : this(args, cohortNode.Cohort)
        {

        }
        
        private void ViewTop100()
        {
            _activator.ViewDataSample(new ViewCohortExtractionUICollection(_cohort));
        }

    }
}
