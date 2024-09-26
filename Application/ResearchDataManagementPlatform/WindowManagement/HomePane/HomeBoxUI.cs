// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;

namespace ResearchDataManagementPlatform.WindowManagement.HomePane;

public partial class HomeBoxUI : UserControl
{
    private IActivateItems _activator;
    private bool _doneSetup;
    private Type _openType;

    private RDMPCollectionCommonFunctionality CommonTreeFunctionality { get; } = new();

    public HomeBoxUI()
    {
        InitializeComponent();
        olvRecent.ItemActivate += OlvRecent_ItemActivate;
    }

    public void SetUp(IActivateItems activator, string title, Type openType, AtomicCommandUIFactory factory,
        params IAtomicCommand[] newCommands)
    {
        _openType = openType;


        if (!_doneSetup)
        {
            var x = new PureDatasetProvider(activator, activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DatasetProviderConfiguration>().First());
            x.Create();
            _activator = activator;
            lblTitle.Text = title;

            btnNew.Image = FamFamFamIcons.add.ImageToBitmap();
            btnNew.Text = "New";
            btnNew.DisplayStyle = ToolStripItemDisplayStyle.Text;

            btnNewDropdown.Image = FamFamFamIcons.add.ImageToBitmap();
            btnNewDropdown.Text = "New...";
            btnNewDropdown.DisplayStyle = ToolStripItemDisplayStyle.Text;

            btnOpen.Text = "Open";
            btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOpen.Click += (s, e) =>
            {
                if (activator.SelectObject(new DialogArgs
                    {
                        WindowTitle = "Open"
                    }, activator.GetAll(openType).ToArray(), out var selected))
                    Open(selected);
            };


            //if there's only one command for new
            if (newCommands.Length == 1)
            {
                //don't use the dropdown
                toolStrip1.Items.Remove(btnNewDropdown);
                btnNew.Click += (s, e) => newCommands.Single().Execute();
            }
            else
            {
                toolStrip1.Items.Remove(btnNew);
                btnNewDropdown.DropDownItems.AddRange(newCommands.Select(factory.CreateMenuItem).Cast<ToolStripItem>()
                    .ToArray());
            }

            olvName.AspectGetter = o => ((HistoryEntry)o).Object.ToString();
            CommonTreeFunctionality.SetUp(RDMPCollection.None, olvRecent, activator, olvName, olvName,
                new RDMPCollectionCommonFunctionalitySettings
                {
                    SuppressChildrenAdder = true
                });

            _doneSetup = true;
        }

        RefreshHistory();
    }

    private void RefreshHistory()
    {
        olvRecent.ClearObjects();
        olvRecent.AddObjects(_activator.HistoryProvider.History.Where(h => h.Object.GetType() == _openType).ToArray());
    }

    private void Open(IMapsDirectlyToDatabaseTable o)
    {
        if (!((DatabaseEntity)o).Exists())
        {
            if (_activator.YesNo($"'{o}' no longer exists, remove from Recent list?", "No longer exists"))
            {
                _activator.HistoryProvider.Remove(o);
                RefreshHistory();
            }

            return;
        }

        var cmd = new ExecuteCommandActivate(_activator, o)
        {
            AlsoShow = true
        };
        cmd.Execute();
    }

    private void OlvRecent_ItemActivate(object sender, EventArgs e)
    {
        if (olvRecent.SelectedObject is HistoryEntry he)
            Open(he.Object);
    }
}