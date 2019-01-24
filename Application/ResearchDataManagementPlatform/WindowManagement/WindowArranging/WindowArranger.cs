using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
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
        private readonly WindowManager _windowManager;
        private readonly DockPanel _mainDockPanel;

        public WindowArranger(IActivateItems activator, WindowManager windowManager, DockPanel mainDockPanel)
        {
            _activator = activator;
            _windowManager =windowManager;
            _mainDockPanel = mainDockPanel;
        }

        public void SetupEditCatalogue(object sender, Catalogue catalogue)
        {
            var tableInfo = catalogue.GetTableInfoList(false).FirstOrDefault();

            _windowManager.CloseAllToolboxes();
            _windowManager.CloseAllWindows();

            _activator.RequestItemEmphasis(this, new EmphasiseRequest(catalogue,2));
            new ExecuteCommandActivate(_activator,catalogue).Execute();

            _windowManager.Create(RDMPCollection.Tables, DockState.DockRight);

            if (tableInfo != null)
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(tableInfo,1));
        }

        public void SetupEditAnything(object sender, IMapsDirectlyToDatabaseTable o)
        {
            _windowManager.CloseAllToolboxes();
            _windowManager.CloseAllWindows();
            
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(o, int.MaxValue));

            var activate = new ExecuteCommandActivate(_activator,o);

            if(!activate.IsImpossible)
                activate.Execute();
        }

        public void Setup(WindowLayout target)
        {
            //Do not reload an existing layout
            string oldXml = _windowManager.MainForm.GetCurrentLayoutXml();
            string newXml = target.LayoutData;

            if(AreBasicallyTheSameLayout(oldXml, newXml))
                return;
            
            _windowManager.CloseAllToolboxes();
            _windowManager.CloseAllWindows();
            _windowManager.MainForm.LoadFromXml(target);
        }

        private bool AreBasicallyTheSameLayout(string oldXml, string newXml)
        {
            var patStripActive = @"Active.*=[""\-\d]*";
            oldXml = Regex.Replace(oldXml, patStripActive, "");
            newXml = Regex.Replace(newXml, patStripActive, "");

            return oldXml.Equals(newXml, StringComparison.CurrentCultureIgnoreCase);
        }

        public void SetupEditDataExtractionProject(object sender, Project project)
        {
            _windowManager.CloseAllToolboxes();
            _windowManager.CloseAllWindows();

            _windowManager.Create(RDMPCollection.DataExport, DockState.DockLeft);

            _activator.RequestItemEmphasis(this, new EmphasiseRequest(project, int.MaxValue));
            var activateDataExportItems = _activator as IActivateItems;

            if (activateDataExportItems != null)
            {
                //execute all unreleased configurations... what could possibly go wrong?
                foreach (var config in project.ExtractionConfigurations.Cast<ExtractionConfiguration>())
                    if (!config.IsReleased)
                    {
                        var cmd = new ExecuteCommandExecuteExtractionConfiguration(_activator).SetTarget(config);
                    
                        if(!cmd.IsImpossible)
                            cmd.Execute();
                    }
            }
        }

        public void SetupEditLoadMetadata(object sender, LoadMetadata loadMetadata)
        {
            if(!_windowManager.IsVisible(RDMPCollection.DataLoad))
                _windowManager.Create(RDMPCollection.DataLoad, DockState.DockLeft);

            var diagram = (Control)_activator.ActivateViewLoadMetadataDiagram(this, loadMetadata);
            ((DockContent)diagram.Parent).DockTo(_mainDockPanel,DockStyle.Right);

            _activator.Activate<ExecuteLoadMetadataUI, LoadMetadata>(loadMetadata);
            _activator.RequestItemEmphasis(this,new EmphasiseRequest(loadMetadata,int.MaxValue));
        }
    }
}