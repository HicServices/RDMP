using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Refreshing;
using CatalogueManager.Tutorials;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportLibrary.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.HomePane
{
    /// <summary>
    /// The starting page of RDMP.  Provides a single easy access entry point into RDMP functionality for common tasks e.g. Data Management, Project Extraction etc.  Click the links of commands
    /// you want to carry out to access wizards that offer streamlined access to the RDMP functionality.
    /// 
    /// <para>You can access the HomeUI at any time by clicking the home icon in the top left of the RDMP tool bar.</para>
    /// </summary>
    public partial class HomeUI : UserControl,ILifetimeSubscriber
    {
        private readonly ToolboxWindowManager _windowManager;
        private readonly AtomicCommandUIFactory _uiFactory;

        public HomeUI(ToolboxWindowManager windowManager)
        {
            this._windowManager = windowManager;
            _uiFactory = new AtomicCommandUIFactory(windowManager.ContentManager.CoreIconProvider);
            InitializeComponent();
        }

        private void BuildCommandLists()
        {
            tlpDataManagement.Controls.Clear();
            tlpCohortCreation.Controls.Clear();
            tlpDataExport.Controls.Clear();
            tlpDataLoad.Controls.Clear();
            
            /////////////////////////////////////Data Management/////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingFile(_windowManager.ContentManager),tlpDataManagement);
            
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_windowManager.ContentManager, true),tlpDataManagement);

            AddCommand(new ExecuteCommandEditExistingCatalogue(_windowManager.ContentManager),
                _windowManager.ContentManager.CoreChildProvider.AllCatalogues,
                cata => cata.Name,
                tlpDataManagement);

            /////////////////////////////////////Cohort Creation/////////////////////////////////

            AddCommand(new ExecuteCommandImportFileAsNewCohort(_windowManager.ContentManager),
                tlpCohortCreation);

            AddCommand(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_windowManager.ContentManager),tlpCohortCreation);

            AddCommand(new ExecuteCommandEditExistingCohortIdentificationConfiguration(_windowManager.ContentManager),
                    _windowManager.ContentManager.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);
            
            AddCommand(new ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults(_windowManager.ContentManager), 
                    _windowManager.ContentManager.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);
            
            /////////////////////////////////////Data Export/////////////////////////////////
            
            var dataExportChildProvider = _windowManager.ContentManager.CoreChildProvider as DataExportChildProvider;
            if (dataExportChildProvider != null)
            {
                AddCommand(new ExecuteCommandCreateNewDataExtractionProject(_windowManager.ContentManager), tlpDataExport);
                AddCommand(new ExecuteCommandEditAndRunExistingDataExtractionProject(_windowManager.ContentManager),
                        dataExportChildProvider.Projects,
                        cic => cic.Name,
                        tlpDataExport);

                AddCommand(new ExecuteCommandImportFileAsCustomDataForCohort(_windowManager.ContentManager), 
                        dataExportChildProvider.Cohorts,
                        c => c.ToString(),
                        tlpDataExport);
            }

            //////////////////////////////////Data Loading////////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewLoadMetadata(_windowManager.ContentManager),tlpDataLoad);
            AddCommand(new ExecuteCommandExecuteLoadMetadata(_windowManager.ContentManager), 
                _windowManager.ContentManager.CoreChildProvider.AllLoadMetadatas,
                lmd=>lmd.Name,
                tlpDataLoad);
            
            FixSizingOfTableLayoutPanel(tlpDataManagement);
            FixSizingOfTableLayoutPanel(tlpCohortCreation);
            FixSizingOfTableLayoutPanel(tlpDataExport);
            FixSizingOfTableLayoutPanel(tlpDataLoad);


            //////////////////////////////////Advanced////////////////////////////////////
            AddCommand(new ExecuteCommandManagePlugins(_windowManager.ContentManager),tlpAdvanced);
        }

        private void AddCommand<T>(IAtomicCommandWithTarget command, IEnumerable<T> selection, Func<T, string> propertySelector, TableLayoutPanel tableLayoutPanel)
        {
            var control = _uiFactory.CreateLinkLabelWithSelection(command, selection, propertySelector);
            tableLayoutPanel.Controls.Add(control, 0, tableLayoutPanel.Controls.Count);

            //extend the size to match
            var panel = (Panel)tableLayoutPanel.Parent;
            panel.Width = Math.Max(panel.Width, control.Width + 10);
        }

        private void AddCommand(IAtomicCommand command, TableLayoutPanel tableLayoutPanel)
        {
            var control = _uiFactory.CreateLinkLabel(command);
            tableLayoutPanel.Controls.Add(control, 0, tableLayoutPanel.Controls.Count);

            //extend the size to match
            var panel = (Panel)tableLayoutPanel.Parent;
            panel.Width = Math.Max(panel.Width, control.Width + 10);
        }

        private void FixSizingOfTableLayoutPanel(TableLayoutPanel tableLayoutPanel)
        {
            foreach (RowStyle style in tableLayoutPanel.RowStyles)
                style.SizeType = SizeType.AutoSize;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_windowManager == null)
                return;

            BuildCommandLists();

            _windowManager.ContentManager.RefreshBus.EstablishLifetimeSubscription(this);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            BuildCommandLists();
        }
    }
}
