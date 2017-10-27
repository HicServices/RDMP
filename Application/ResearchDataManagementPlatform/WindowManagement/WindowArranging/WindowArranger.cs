using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Arranging;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.LoadExecutionUIs;
using DataExportLibrary.Data.DataTables;
using DataExportManager.ItemActivation;
using DataExportManager.ProjectUI;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.WindowArranging
{
    public class WindowArranger : IArrangeWindows
    {
        private readonly IActivateItems _activator;
        private readonly ToolboxWindowManager _toolboxWindowManager;
        private readonly DockPanel _mainDockPanel;

        public WindowArranger(IActivateItems activator, ToolboxWindowManager toolboxWindowManager, DockPanel mainDockPanel)
        {
            _activator = activator;
            _toolboxWindowManager =toolboxWindowManager;
            _mainDockPanel = mainDockPanel;
        }

        public void SetupEditCatalogue(object sender, Catalogue catalogue)
        {
            var tableInfo = catalogue.GetTableInfoList(false).FirstOrDefault();

            _toolboxWindowManager.CloseAllToolboxes();
            _toolboxWindowManager.CloseAllWindows();

            _activator.RequestItemEmphasis(this, new EmphasiseRequest(catalogue,2));
            new ExecuteCommandActivate(_activator,catalogue).Execute();

            _toolboxWindowManager.Create(RDMPCollection.Tables, DockState.DockRight);

            if (tableInfo != null)
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(tableInfo,1));
        }

        public void SetupEditCohortIdentificationConfiguration(object sender, CohortIdentificationConfiguration cohortIdentificationConfiguration)
        {
            _toolboxWindowManager.CloseAllToolboxes();
            _toolboxWindowManager.CloseAllWindows();

            _toolboxWindowManager.Create(RDMPCollection.Cohort, DockState.DockLeft);

            _activator.RequestItemEmphasis(this, new EmphasiseRequest(cohortIdentificationConfiguration, int.MaxValue));
            new ExecuteCommandActivate(_activator, cohortIdentificationConfiguration).Execute();
        }

        public void SetupEditDataExtractionProject(object sender, Project project)
        {
            _toolboxWindowManager.CloseAllToolboxes();
            _toolboxWindowManager.CloseAllWindows();

            _toolboxWindowManager.Create(RDMPCollection.DataExport, DockState.DockLeft);

            _activator.RequestItemEmphasis(this, new EmphasiseRequest(project, int.MaxValue));
            var activateDataExportItems = _activator as IActivateDataExportItems;

            bool foundAtLeastOneConfiguration = false;

            if (activateDataExportItems != null)
            {
                // activateDataExportItems.ActivateProject(this, project);
                foreach (var config in project.ExtractionConfigurations.Cast<ExtractionConfiguration>())
                    if (!config.IsReleased)
                    {
                        activateDataExportItems.ExecuteExtractionConfiguration(this,new ExecuteExtractionUIRequest(config));
                        foundAtLeastOneConfiguration = true;
                    }

                if(!foundAtLeastOneConfiguration)
                    activateDataExportItems.ActivateProject(this,project);
            }
        }

        public void SetupEditLoadMetadata(object sender, LoadMetadata loadMetadata)
        {
            _toolboxWindowManager.CloseAllToolboxes();
            _toolboxWindowManager.CloseAllWindows();

            _toolboxWindowManager.Create(RDMPCollection.DataLoad, DockState.DockLeft);

            _activator.Activate<ExecuteLoadMetadataUI, LoadMetadata>(loadMetadata);

            var diagram = (Control)_activator.ActivateViewLoadMetadataDiagram(this, loadMetadata);
            ((DockContent)diagram.Parent).DockTo(_mainDockPanel,DockStyle.Right);
        }
    }
}