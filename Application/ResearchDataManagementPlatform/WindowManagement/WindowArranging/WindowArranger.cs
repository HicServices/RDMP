// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.ItemActivation.Emphasis;
using Rdmp.UI.LoadExecutionUIs;
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

            //activate it if possible
            if (!activate.IsImpossible)
                activate.Execute();
            else
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(o, 1)); //otherwise just show it
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