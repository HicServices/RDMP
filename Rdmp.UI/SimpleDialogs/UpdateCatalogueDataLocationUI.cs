// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.SimpleDialogs;

public partial class UpdateCatalogueDataLocationUI : Form
{
    private readonly Catalogue _catalogue;
    private readonly ColumnInfo _columnInfo;
    private bool _firstTime = true;
    private readonly IActivateItems _activator;

    public UpdateCatalogueDataLocationUI(IActivateItems activator, Catalogue catalogue)
    {
        InitializeComponent();
        _catalogue = catalogue;
        olvState.AspectGetter = State_AspectGetter;
        GetCurrentDataLocation();
        RefreshData();
        _activator = activator;
    }

    public UpdateCatalogueDataLocationUI(IActivateItems activator, ColumnInfo columnInfo)
    {
        InitializeComponent();
        _columnInfo = columnInfo;
        olvState.AspectGetter = State_AspectGetter;
        GetCurrentDataLocation();
        RefreshData();
        _activator = activator;
    }


    private void RefreshData()
    {
        helpIcon2.SetHelpText("Column Mapping", """
                                                Optionally add a mapping for your columns.
                                                '$column' is the current column value.
                                                e.g. "$column_old" would turn "myColumn" into "myColumn_old"
                                                """);
        if (_catalogue is not null)
        {
            tlvDatasets.AddObjects(_catalogue.CatalogueItems);
            tlvDatasets.EnableObjects(tlvDatasets.Objects);
        }
        else
        {
            splitContainer1.SplitterDistance = 0;

            panel1.Width = 0;
            panel1.Visible = false;
            tlvDatasets.Visible = false;
        }

        if (_firstTime)
        {
            tlvDatasets.CheckAll();
            _firstTime = false;
        }
    }

    private void GetCurrentDataLocation()
    {
        if (_columnInfo is not null)
        {
            tbCurrentLocation.Text = _columnInfo.Name;
        }
        else
        {
            var location = _catalogue.CatalogueItems.Where(ci => ci.ColumnInfo is not null).Select(ci => DropColumnIdentifierFromName(ci.ColumnInfo.Name))
                .ToList();

            tbCurrentLocation.Text =
                location.Distinct().Skip(1).Any() ? "Multiple Locations Found" : location.First();
        }
    }

    private string DropColumnIdentifierFromName(string name)
    {
        return string.Join('.', name.Split('.')[..^1]);
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        tlvDatasets.ModelFilter = new TextMatchFilter(tlvDatasets, tbFilter.Text);
        tlvDatasets.UseFiltering = true;
    }

    private object State_AspectGetter(object rowobject)
    {
        var item = (CatalogueItem)rowobject;
        return item.ColumnInfo.Name;
    }

    private void label1_Click(object sender, EventArgs e)
    {
    }

    private void label1_Click_1(object sender, EventArgs e)
    {
    }

    private void label2_Click(object sender, EventArgs e)
    {
    }

    private void Run()
    {
        //should be disabled until things are set
        var catalogueItems = _catalogue is not null
            ? tlvDatasets.CheckedObjects.Cast<CatalogueItem>().ToArray()
            : _columnInfo.CatalogueItems.ToArray();
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(_activator, catalogueItems,
            serverDatabaseTableSelector1.GetDiscoveredTable(), tbMapping.Text);
        var check = cmd.Check();
        if (check is null)
        {
            label3.Text = null;
            cmd.Execute();
            if (_catalogue is not null)
                _activator.RefreshBus.Publish(_catalogue, new RefreshObjectEventArgs(_catalogue));
            if (_columnInfo is not null)
                _activator.RefreshBus.Publish(_columnInfo, new RefreshObjectEventArgs(_columnInfo));
            this.Close();
        }
        else
        {
            label3.Text = check;
        }
    }

    private void btnConfirm_Click(object sender, EventArgs e)
    {
        var catalogueItems = _catalogue is not null
            ? tlvDatasets.CheckedObjects.Cast<CatalogueItem>()
            : _columnInfo.CatalogueItems;
        var location = catalogueItems.Select(ci => DropColumnIdentifierFromName(ci.ColumnInfo.Name)).ToList();
        if (location.Distinct().Skip(1).Any())
        {
            if (_activator.YesNo("Catalogue uses multiple tables. Are you sure you want to proceed?",
                    "Update all Tables")) Run();
            Run();
        }
        else
        {
            Run();
        }
    }

    private void serverDatabaseTableSelector1_Load(object sender, EventArgs e)
    {
    }

    private void label3_Click(object sender, EventArgs e)
    {
    }

    private void UpdateCatalogueDataLocationUI_Load(object sender, EventArgs e)
    {
    }
}