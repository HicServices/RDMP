// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using NPOI.POIFS.Crypt.Agile;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs.SubComponents;
using Rdmp.UI.Menus;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using static System.Linq.Enumerable;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using Point = System.Drawing.Point;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

/// <summary>
/// A Lookup in RDMP is a relationship between three columns.  The 'Foreign Key' column must come from a normal dataset table e.g. 'Prescribing.DrugCode', the 'Primary Key' must come
/// from a different table (usually prefixed z_ to indicate it is a lookup table) e.g. 'z_DrugsLookup.DrugCode' and then a 'Description' column from the same table e.g.
/// 'z_DrugsLookup.DrugName'.  This is maintained in the RDMP Catalogue database and does not result in any changes / constraints on your actual data repository.
/// 
/// <para>While it might seem redundant to have to configure this logic in the RDMP as well as (if you choose to) constraints in your data repository, this approach allows for
/// flexibility when it comes to incomplete/corrupt lookup tables (common in the research data management domain) as well as letting us bundle lookups with data extracts etc.</para>
/// 
/// <para>This window is a low level alternative to LookupConfiguration (the recommended way of creating these Lookup relationships), this form lets you explicitly create a Lookup
/// relationship using the supplied columns.  First of all you should make sure that the column you right clicked to activate the Form is the Description column.  Then select the
/// 'Primary Key' and 'Foreign Key' as described above.  </para>
/// 
/// <para>If you have a particularly insane database design you can configure composite joins (where there are multiple columns that make up a composite 'Foreign Key' / 'Primary Key'.  For
/// example if there was crossover in 'DrugCode' between two countries then the Lookup relationship would need 'Primary Key' Prescribing.DrugCode + Prescribing.Country and the
/// 'Foreign Key' would need to be z_DrugsLookup.DrugCode + z_DrugsLookup.Country.</para>
///
/// <para>Allows you to rapidly import and configure lookup table relationships into the RDMP.  This has two benefits, firstly lookup tables will be automatically included in project extracts
/// of the dataset you are editing.  Secondly lookup columns will be available for inclusion directly into the extraction on a per row basis (for researchers who can't deal with having
/// to lookup the meaning of codes in separate files).</para>
/// 
/// <para>Start by identifying a lookup table and click Import Lookup.  Then drag the primary key of the lookup into the PrimaryKey box.  Then drag the description column of the lookup onto the
/// Foreign key field in the dataset you are modifying.  If you have multiple foreign keys (e.g. two columns SendingLocation and DischargeLocation both of which are location codes) then
/// join them both up (this will give you two lookup description fields SendingLocation_Desc and DischargeLocation_Desc).  </para>
/// 
/// <para>All Lookups and Lookup column description configurations are artifacts in the RDMP database and no actual changes will take place on your data repository (i.e. no constraints will be added
/// to the underlying data database). </para>
/// </summary>
public partial class LookupConfigurationUI : LookupConfiguration_Design
{
    private Catalogue _catalogue;
    private ToolTip toolTip = new();
    private string _errorMessage = null;
    private List<ExtractionInformation> _allExtractionInformationFromCatalogue = new();

    //constructor
    public LookupConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        lblTitle.Text = $"Create Lookup For {_catalogue.Name}";
        lblTitle.Visible = true;
        var tableInfo = activator.CoreChildProvider.AllTableInfos;
        if (tableInfo.Length == 0)
        {
            HandleError("No Table Infos Available");
            return;
        }
        cbSelectLookupTable.Enabled = true;
        foreach (var tb in tableInfo)
        {
            cbSelectLookupTable.Items.Add(tb);
        }
        _allExtractionInformationFromCatalogue =
           new List<ExtractionInformation>(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
        _allExtractionInformationFromCatalogue.Sort();
        AddRelationOption();
        AddDescriptionOption();
    }

    private List<ComboBox> PKRelations = new();
    private List<ComboBox> FKRelations = new();
    private List<Label> Labels = new();

    private void ClearDescriptions()
    {
        foreach (var desc in Descriptions)
        {
            gbDescription.Controls.Remove(desc);
        }
        Descriptions = new List<ComboBox>();
    }

    private void ClearRelations()
    {
        foreach (var pk in PKRelations)
        {
            gbAddRelation.Controls.Remove(pk);
        }
        foreach (var fk in FKRelations)
        {
            gbAddRelation.Controls.Remove(fk);
        }
        foreach (var l in Labels)
        {
            gbAddRelation.Controls.Remove(l);
        }
        PKRelations = new();
        FKRelations = new();
        Labels = new();
    }

    private void RemoveSingleRelation(object sender, EventArgs e)
    {
        if (sender.GetType() == typeof(Button))
        {
            var btn = (Button)sender;
            var index = btn.ImageIndex;
            gbAddRelation.Controls.Remove(PKRelations[index]);
            gbAddRelation.Controls.Remove(FKRelations[index]);
            gbAddRelation.Controls.Remove(Labels[index]);
            PKRelations.RemoveAt(index);
            FKRelations.RemoveAt(index);
            Labels.RemoveAt(index);
        }
    }

    private void AddRelationOption()
    {
        if (cbSelectLookupTable.SelectedItem == null) return;
        var selectedLookup = (TableInfo)cbSelectLookupTable.SelectedItem;
        if (PKRelations.Count == selectedLookup.ColumnInfos.Length)
        {
            //no possible entries
            btnAddAnotherRelation.Enabled = false;
            return;
        }
        var pk = new ComboBox();
        foreach (var item in selectedLookup.ColumnInfos)
        {
            pk.Items.Add(item);
        }
        pk.Height = 20;
        pk.Top = 50 + (PKRelations.Count * 25);
        pk.Width = 400;
        pk.Text = "(Primary Key)";
        var fk = new ComboBox();
        foreach (var item in _catalogue.CatalogueItems)
        {
            fk.Items.Add(item.ColumnInfo);
        }
        fk.Height = 20;
        fk.Top = 50 + (FKRelations.Count * 25);
        fk.Left = 440;
        fk.Width = 400;
        fk.Text = "(Foreign Key)";
        var label = new Label();
        label.Text = "=>";
        label.Width = 20;
        label.Left = 410;
        label.Top = 50 + (FKRelations.Count * 25);
        PKRelations.Add(pk);
        Labels.Add(label);
        FKRelations.Add(fk);
        gbAddRelation.Controls.Add(pk);
        gbAddRelation.Controls.Add(label);
        gbAddRelation.Controls.Add(fk);
        gbDescription.Top = gbDescription.Top + 25;
        gbSubmit.Top = gbSubmit.Top + 25;
        if (PKRelations.Count > _allExtractionInformationFromCatalogue.Count)
        {
            //no possible entries
            btnAddAnotherRelation.Enabled = true;
        }
        else
        {
            btnAddAnotherRelation.Enabled = true;
        }
    }


    public void SetLookupTableInfo(TableInfo tableInfo) { }

    private List<ComboBox> Descriptions = new();

    private void AddDescriptionOption()
    {
        var selectedLookup = (TableInfo)cbSelectLookupTable.SelectedItem;
        if (selectedLookup == null) return;
        var cb = new ComboBox();
        foreach (var item in selectedLookup.ColumnInfos)
        {
            cb.Items.Add(item);
        }
        cb.Width = 400;
        cb.Top = 50 + (Descriptions.Count * 25);
        Descriptions.Add(cb);
        gbDescription.Controls.Add(cb);
        gbSubmit.Top = gbSubmit.Top + 25;
        if (Descriptions.Count == selectedLookup.ColumnInfos.Count())
        {
            btnAddDescription.Enabled = false;
        }
        else
        {
            btnAddDescription.Enabled = true;
        }
    }

    public override string GetTabName() => $"Add Lookup ({base.GetTabName()})";

    private void HandleError(string msg)
    {
        _errorMessage = msg;
        lblErrorText.Text = _errorMessage;
        lblErrorText.Visible = true;
    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void cbSelectLookupTable_SelectedIndexchanged(object sender, EventArgs e)
    {
        lblErrorText.Text = null;
        lblErrorText.Visible = false;
        ClearRelations();
        ClearDescriptions();
        AddRelationOption();
        AddDescriptionOption();
        btnCreateLookup.Enabled = true;
    }

    private void btnAddAnotherRelation_Click(object sender, EventArgs e)
    {
        AddRelationOption();
    }

    private void btnAddDescription_Click(object sender, EventArgs e)
    {
        AddDescriptionOption();

    }

    private void gbAddRelation_Enter(object sender, EventArgs e)
    {

    }

    private bool ValidateUserInput()
    {
        //has lookup
        if (cbSelectLookupTable.SelectedItem == null)
        {
            HandleError("No Lookup table selected");
            return false;
        }
        if (PKRelations.Where(d => d.SelectedItem != null).Count() == 0 || FKRelations.Where(d => d.SelectedItem != null).Count() == 0)
        {
            HandleError("At least one PK FK mapping must be set");
            return false;
        }
        if (PKRelations.Where(d => d.SelectedItem != null).Count() != FKRelations.Where(d => d.SelectedItem != null).Count())
        {
            HandleError("Must have a 1-to-1 mapping of PK and FK mappings");
            return false;
        }
        if (Descriptions.Where(d => d.SelectedItem != null).Count() == 0)
        {
            HandleError("At least one Description column must be set");
            return false;
        }
        var descColumnInfos = Descriptions.Where(d => d.SelectedItem != null).Select(d => ((ColumnInfo)d.SelectedItem).ID);
        var pkColumnInfos = PKRelations.Where(d => d.SelectedItem != null).Select(d => ((ColumnInfo)d.SelectedItem).ID);
        var fkColumnInfos = FKRelations.Where(d => d.SelectedItem != null).Select(d => ((ColumnInfo)d.SelectedItem).ID);
        if (pkColumnInfos.Intersect(descColumnInfos).Any() || fkColumnInfos.Intersect(descColumnInfos).Any())
        {
            HandleError("A Description Column cannot be used in the PK FK mapping");
            return false;
        }
        return true;
    }

    private void Clear()
    {
        ClearRelations();
        btnAddAnotherRelation.Enabled = false;
        ClearDescriptions();
        btnAddDescription.Enabled = false;
        cbSelectLookupTable.SelectedItem = null;
        tbCollation.Text = null;
        btnCreateLookup.Enabled = false;
    }

    private void btnCreateLookup_Click(object sender, EventArgs e)
    {
        if (ValidateUserInput())
        {
            var alsoCreateExtractionInformation =
                  Activator.YesNo(
                      $"Also create a virtual extractable column(s) in '{_catalogue}' called '<Column>_Desc'",
                      "Create Extractable Column?");
            var keyPairs = new List<Tuple<ColumnInfo, ColumnInfo>> { };
            keyPairs = FKRelations.Where(d => d.SelectedItem != null).Zip(PKRelations.Where(d => d.SelectedItem != null), (x, y) => new Tuple<ColumnInfo, ColumnInfo>((ColumnInfo)x.SelectedItem, (ColumnInfo)y.SelectedItem)).ToList();
            var descs = Descriptions.Select(d => (ColumnInfo)d.SelectedItem).ToArray();
            var foreignKeyExtractionInformation =
               _allExtractionInformationFromCatalogue.SingleOrDefault(e =>
                   e.ColumnInfo != null && e.ColumnInfo.Equals((ColumnInfo)FKRelations[0].SelectedItem)) ??
               throw new Exception("Foreign key column(s) must come from the Catalogue ExtractionInformation columns");
            var cmd = new ExecuteCommandCreateLookup(Activator.RepositoryLocator.CatalogueRepository,
                   foreignKeyExtractionInformation, descs,
                   keyPairs, tbCollation.Text, alsoCreateExtractionInformation);

            cmd.Execute();
            MessageBox.Show($"Successfully created lookup for {_catalogue.Name}");
            Clear();
        }
    }

    private void tbCollation_TextChanged(object sender, EventArgs e)
    {

    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
        WideMessageBox.Show("Selecting a Lookup Table", "Select the Table you wish to use to create a lookup for.");
    }

    private void pictureBox2_Click(object sender, EventArgs e)
    {
        WideMessageBox.Show("Lookup Relations Help",
            @"Lookup relations are between two tables.
            Select one column from the current Catalogue table and one from the selected lookup table.
            You typically only need one relation per lookup, but for more complicated cases you can add as many as required.
            ");
    }

    private void pictureBox4_Click(object sender, EventArgs e)
    {
        WideMessageBox.Show("Description Help","Select a column that provides a description of what the lookup is doing.");
    }

    private void pictureBox5_Click(object sender, EventArgs e)
    {
        WideMessageBox.Show("Creating A Lookup",
            @"Creating a lookup will generate a new lookup table linking this Catalogue with the selected table above.
            It will check that all the details you have entered are correct before proceeding.
            You will be asked if you wish to add a description field. We recommend selecting 'yes'.
            ");
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupConfiguration_Design, UserControl>))]
public abstract class LookupConfiguration_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}