// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using NPOI.SS.Formula.Functions;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs.RegexRedactionConfigurationForm;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Lets you check for and redact entries in a catalogue
/// </summary>
public partial class RedactCatalogueUI : RedactCatalogueUI_Design
{
    private IActivateItems _activator;
    private Catalogue _catalogue;
    public RedactCatalogueUI()
    {
        InitializeComponent();
        this.comboBox1.Items.Clear();
        AssociatedCollection = RDMPCollection.Tables;
        this.folv.ButtonClick += Redact;
        this.btnNewRegex.Click += HandleNewRegex;
    }

    public void HandleNewRegex(object sender, EventArgs e)
    {
        //_activator
        var ui = new CreateNewRegexRedactionConfigurationUI(_activator);
        var res = ui.ShowDialog();
        var cmd = new ExecuteCommandRefreshObject(_activator, _catalogue);
        cmd.Execute();

    }

    public void Redact(object sender, CellClickEventArgs e)
    {
        Debug.WriteLine(String.Format("Button clicked: ({0}, {1}, {2})", e.RowIndex, e.SubItem, e.Model));
       //todo want to run a "redact single" with the PKS
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        Bind(tbCatalogueName, "Text", "Name", c => c.Name);

        _activator = activator;
        _catalogue = databaseObject;
        var regexConfigurations = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RegexRedactionConfiguration>().ToArray();
        this.comboBox1.Items.Clear();
        comboBox1.Items.AddRange(regexConfigurations);
        comboBox1.DisplayMember = "Name";

        var nonPKColumns = databaseObject.CatalogueItems.Where(c => !c.ColumnInfo.IsPrimaryKey).ToArray();
        checkedListBox1.Items.AddRange(nonPKColumns);
    }

    private void btnNewRegex_Click(object sender, EventArgs e)
    {
        var form =  new RegexRedactionConfigurationCreationUI(_activator);
        form.ShowDialog();
    }
    private void btnIdentify_Click(object sender, EventArgs e)
    {
        int? max = string.IsNullOrWhiteSpace(tbMaxCount.Text) ? null : int.Parse(tbMaxCount.Text);
        var columns = new List<ColumnInfo>();
        foreach (object item in checkedListBox1.CheckedItems)
        {
            columns.Add(((CatalogueItem)item).ColumnInfo);
        }
        var config = comboBox1.SelectedItem as RegexRedactionConfiguration;
        var cmd = new ExecuteCommandIdentifyRegexRedactionsInCatalogue(_activator, _catalogue, config, columns, max);
        cmd.Execute();
        if(cmd.results is null)
        {
            //Do Something!
            return;
        }
        foreach(DataRow row in cmd.results.Rows)
        {
            object o = new
            {
                FoundValue = row[0],
                RedactionValue = row["RedactionValue"],
                Redact = "Redact",
                PKs = row.ItemArray.Skip(1).SkipLast(1).ToArray()
            };
            folv.AddObject(o);
        }
    }

    private void btnRedact_Click(object sender, EventArgs e)
    {
        if (_activator.YesNo("Are you sure?", "TODO"))
        {
            int? max = string.IsNullOrWhiteSpace(tbMaxCount.Text) ? null : int.Parse(tbMaxCount.Text);
            var columns = new List<ColumnInfo>();
            foreach (object item in checkedListBox1.CheckedItems)
            {
                columns.Add(((CatalogueItem)item).ColumnInfo);
            }
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, (RegexRedactionConfiguration)comboBox1.SelectedItem, columns, max);
            cmd.Execute();
            //TODO some sort up update to let the user know how many we updated?
        }
    }

}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RedactCatalogueUI_Design, UserControl>))]
public abstract class RedactCatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}