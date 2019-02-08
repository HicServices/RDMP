// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using CatalogueManager.Refreshing;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportLibrary.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

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
        private readonly WindowManager _windowManager;
        private readonly AtomicCommandUIFactory _uiFactory;

        public HomeUI(WindowManager windowManager)
        {
            this._windowManager = windowManager;
            _uiFactory = new AtomicCommandUIFactory(windowManager.ActivateItems);
            InitializeComponent();
        }

        private void BuildCommandLists()
        {
            tlpDataManagement.Controls.Clear();
            tlpCohortCreation.Controls.Clear();
            tlpDataExport.Controls.Clear();
            tlpDataLoad.Controls.Clear();
            tlpAdvanced.Controls.Clear();

            var activator = _windowManager.ActivateItems;
            
            /////////////////////////////////////Data Management/////////////////////////////////
            //AddLabel("New Catalogue", tlpDataManagement);
            
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingFile(activator){OverrideCommandName = "New Catalogue From File"},tlpDataManagement);

            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(activator, true) { OverrideCommandName = "New Catalogue From Existing Database Table" }, tlpDataManagement);

            AddCommand(new ExecuteCommandEditExistingCatalogue(activator),
                activator.CoreChildProvider.AllCatalogues,
                cata => cata.Name,
                tlpDataManagement);

            AddCommand(
                new ExecuteCommandRunDQEOnCatalogue(activator),
                activator.CoreChildProvider.AllCatalogues, cata => cata.Name,
                tlpDataManagement);

            /////////////////////////////////////Cohort Creation/////////////////////////////////

            AddCommand(new ExecuteCommandCreateNewCohortFromFile(activator),tlpCohortCreation);

            AddCommand(new ExecuteCommandCreateNewCohortIdentificationConfiguration(activator)
            {
                OverrideCommandName = "Create New Cohort Identification Query"
            },tlpCohortCreation);

            AddCommand(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(activator)
            {
                OverrideCommandName = "Create New Cohort From Cohort Identification Query"
            },
                    activator.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);

            AddCommand(new ExecuteCommandEditExistingCohortIdentificationConfiguration(activator)
            {
                OverrideCommandName = "Edit Cohort Identification Query"
            },
                    activator.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);


            AddCommand(new ExecuteCommandCreateNewCohortFromCatalogue(activator)
            {
                OverrideCommandName = "Create New Cohort From Dataset"
            },
                activator.CoreChildProvider.AllCatalogues,
                c=>c.Name,
tlpCohortCreation);
            
            /////////////////////////////////////Data Export/////////////////////////////////
            
            var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;
            if (dataExportChildProvider != null)
            {
                AddCommand(new ExecuteCommandCreateNewDataExtractionProject(activator), tlpDataExport);
                AddCommand(new ExecuteCommandEditDataExtractionProject(activator),
                        dataExportChildProvider.Projects,
                        cic => cic.Name,
                        tlpDataExport);

                AddCommand(new ExecuteCommandMakeCatalogueProjectSpecific(activator),
                    dataExportChildProvider.AllCatalogues.Where(c=>!c.IsProjectSpecific(null)).ToArray(),
                    c=>c.Name,tlpDataExport );
            }

            //////////////////////////////////Data Loading////////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewLoadMetadata(activator),tlpDataLoad);
            AddCommand(new ExecuteCommandExecuteLoadMetadata(activator), 
                activator.CoreChildProvider.AllLoadMetadatas,
                lmd=>lmd.Name,
                tlpDataLoad);
            
            FixSizingOfTableLayoutPanel(tlpDataManagement);
            FixSizingOfTableLayoutPanel(tlpCohortCreation);
            FixSizingOfTableLayoutPanel(tlpDataExport);
            FixSizingOfTableLayoutPanel(tlpDataLoad);


            //////////////////////////////////Advanced////////////////////////////////////
            AddCommand(new ExecuteCommandManagePlugins(activator),tlpAdvanced);
        }

        private void AddLabel(string text,TableLayoutPanel tableLayoutPanel)
        {
            var label = new Label();
            label.BackColor = Color.LightBlue;
            label.ForeColor = Color.Black;

            label.Dock = DockStyle.Top;
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label.Location = new System.Drawing.Point(3, 0);
            label.Name = "label5";
            label.Size = new System.Drawing.Size(294, 18);
            label.TabIndex = 0;
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            tableLayoutPanel.Controls.Add(label);
        }

        private void AddCommand<T>(IAtomicCommandWithTarget command, IEnumerable<T> selection, Func<T, string> propertySelector, TableLayoutPanel tableLayoutPanel)
        {
            var control = _uiFactory.CreateLinkLabelWithSelection(command, selection, propertySelector);

            SetBackgroundColor(tableLayoutPanel, control);
            
            tableLayoutPanel.Controls.Add(control, 0, tableLayoutPanel.Controls.Count);

            //extend the size to match
            var panel = (Panel)tableLayoutPanel.Parent;
            panel.Width = Math.Max(panel.Width, control.Width + 10);
        }
        
        private void AddCommand(IAtomicCommand command, TableLayoutPanel tableLayoutPanel)
        {
            var control = _uiFactory.CreateLinkLabel(command);

            SetBackgroundColor(tableLayoutPanel, control);

            tableLayoutPanel.Controls.Add(control, 0, tableLayoutPanel.Controls.Count);
            
            //extend the size to match
            var panel = (Panel)tableLayoutPanel.Parent;
            panel.Width = Math.Max(panel.Width, control.Width + 10);
        }

        readonly Dictionary<TableLayoutPanel, int> _alternateBackgroundColours = new Dictionary<TableLayoutPanel, int>();

        private void SetBackgroundColor(TableLayoutPanel tableLayoutPanel, Control control)
        {
            if (!_alternateBackgroundColours.ContainsKey(tableLayoutPanel))
                _alternateBackgroundColours.Add(tableLayoutPanel, 0);

            control.BackColor = _alternateBackgroundColours[tableLayoutPanel]++ % 2 == 0 ? Color.AliceBlue : Color.White;
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

            _windowManager.ActivateItems.RefreshBus.EstablishLifetimeSubscription(this);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            BuildCommandLists();
        }
    }
}
