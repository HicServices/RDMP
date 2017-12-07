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
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI;
using MapsDirectlyToDatabaseTable;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.WindowArranging
{
    /// <summary>
    /// Facilitates opening/closing lots of windows at once to achieve a specific goal (e.g. running a data load).  Basically sets up the tabs for a user friendly
    /// consistent experience for the called user task.
    /// </summary>
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

        public void SetupEditAnything(object sender, IMapsDirectlyToDatabaseTable o)
        {
            _toolboxWindowManager.CloseAllToolboxes();
            _toolboxWindowManager.CloseAllWindows();
            
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(o, int.MaxValue));

            var activate = new ExecuteCommandActivate(_activator,o);

            if(!activate.IsImpossible)
                activate.Execute();
        }

        public void SetupEditDataExtractionProject(object sender, Project project)
        {
            _toolboxWindowManager.CloseAllToolboxes();
            _toolboxWindowManager.CloseAllWindows();

            _toolboxWindowManager.Create(RDMPCollection.DataExport, DockState.DockLeft);

            _activator.RequestItemEmphasis(this, new EmphasiseRequest(project, int.MaxValue));
            var activateDataExportItems = _activator as IActivateItems;

            bool foundAtLeastOneConfiguration = false;

            if (activateDataExportItems != null)
            {
                //execute all unreleased configurations... what could possibly go wrong?
                foreach (var config in project.ExtractionConfigurations.Cast<ExtractionConfiguration>())
                    if (!config.IsReleased)
                        new ExecuteCommandExecuteExtractionConfiguration(_activator, true).SetTarget(config).Execute();

                if(!foundAtLeastOneConfiguration)
                    new ExecuteCommandActivate(_activator,project).Execute();
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