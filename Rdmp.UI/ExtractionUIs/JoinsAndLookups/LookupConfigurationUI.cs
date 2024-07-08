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
    }

    private List<ComboBox> PKRelations = new();
    private List<ComboBox> FKRelations = new();
    private List<Label> Labels = new();
    private List<Button> RemoveButtons = new();


    private void ClearRelations()
    {

        //todo remove all relations
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
        var removeBtn = new Button();
        removeBtn.Text = "Remove";
        removeBtn.ImageIndex = RemoveButtons.Count;
        removeBtn.Width = 100;
        removeBtn.Left = 850;
        removeBtn.Top = 50 + (FKRelations.Count * 25);
        PKRelations.Add(pk);
        Labels.Add(label);
        FKRelations.Add(fk);
        RemoveButtons.Add(removeBtn);
        gbAddRelation.Controls.Add(pk);
        gbAddRelation.Controls.Add(label);
        gbAddRelation.Controls.Add(fk);
        gbAddRelation.Controls.Add(removeBtn);
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


    public override string GetTabName() => $"Add Lookup ({base.GetTabName()})";

    private void HandleError(string msg)
    {
        _errorMessage = msg;
    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void cbSelectLookupTable_SelectedIndexchanged(object sender, EventArgs e)
    {
        ClearRelations();
        AddRelationOption();
    }

    private void btnAddAnotherRelation_Click(object sender, EventArgs e)
    {
        AddRelationOption();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupConfiguration_Design, UserControl>))]
public abstract class LookupConfiguration_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}