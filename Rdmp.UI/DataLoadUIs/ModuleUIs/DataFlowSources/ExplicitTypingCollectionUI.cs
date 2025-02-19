// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.Repositories;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.DataLoadUIs.ModuleUIs.DataFlowSources;

/// <summary>
/// ExplicitlyTypedColumns are an expectation that a column of a given name will appear in the data flow pipeline DataTable and a notification to the RDMP that it must be given the
/// supplied C# Type instead of RDMP infering Types or leaving it as the default DataColumn Type as usually happens.  You should only explicitly type a few columns and usually only
/// where they are likely to be confused e.g. if you have a column with important leading zeroes that should be treated as a string despite looking like an int.  If you have too
/// many explicitly typed columns it can limit your Pipelines reusability with novel files you might receive in the future.
/// 
/// <para>For a use case of when this is useful see ExplicitColumnTypeUI (this form hosts a collection of these controls).</para>
/// </summary>
public partial class ExplicitTypingCollectionUI : Form, ICustomUI<ExplicitTypingCollection>
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ICatalogueRepository CatalogueRepository { get; set; }

    public ExplicitTypingCollectionUI()
    {
        InitializeComponent();
        DialogResult = DialogResult.Cancel;
    }

    private List<ExplicitColumnTypeUI> types = new();

    private void btnAddColumn_Click(object sender, EventArgs e)
    {
        AddColumn("", null);
        ReLayoutTable();
    }

    private void AddColumn(string name, Type t)
    {
        var ui = new ExplicitColumnTypeUI(name, t);
        types.Add(ui);
        ui.DeletePressed += (s, e) =>
        {
            types.Remove((ExplicitColumnTypeUI)s);
            ReLayoutTable();
        };
    }

    private void ReLayoutTable()
    {
        tableLayoutPanel1.Controls.Clear();
        tableLayoutPanel1.RowCount = types.Count;
        for (var i = 0; i < types.Count; i++)
            tableLayoutPanel1.Controls.Add(types[i], 1, i + 1);
    }

    public void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value)
    {
        SetUnderlyingObjectTo((ExplicitTypingCollection)value);
    }

    public ICustomUIDrivenClass GetFinalStateOfUnderlyingObject() => GetToReturn();

    private ExplicitTypingCollection GetToReturn()
    {
        var toReturn = new ExplicitTypingCollection();

        foreach (var columnTypeUI in types)
            toReturn.ExplicitTypesCSharp.Add(columnTypeUI.ColumnName, columnTypeUI.Type);

        return toReturn;
    }

    public void SetUnderlyingObjectTo(ExplicitTypingCollection value)
    {
        var child = value ?? new ExplicitTypingCollection();

        foreach (var kvp in child.ExplicitTypesCSharp)
            AddColumn(kvp.Key, kvp.Value);

        ReLayoutTable();
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        try
        {
            DialogResult = DialogResult.OK;
            GetToReturn();
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
            return;
        }

        Close();
    }

    private void ExplicitTypingCollectionUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (CatalogueRepository == null)
            return;

        if (DialogResult != DialogResult.OK)
            if (MessageBox.Show("Close without saving?", "Cancel Changes", MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                e.Cancel = true;
    }
}