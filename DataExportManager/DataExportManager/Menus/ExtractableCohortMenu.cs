using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Menus;
using DataExportLibrary.Data.DataTables;
using DataExportManager.DataViewing.Collections;

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

            Add(new ExecuteCommandDeprecate(args.ItemActivator, cohort, !cohort.IsDeprecated));
        }
        
        private void ViewTop100()
        {
            _activator.ViewDataSample(new ViewCohortExtractionUICollection(_cohort));
        }

    }
}
