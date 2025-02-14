// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using ScintillaNET;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

public partial class LookupBrowserUI : LookupBrowserUI_Design
{
    public LookupBrowserUI()
    {
        InitializeComponent();

        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
    }

    private ColumnInfo _keyColumn;
    private ColumnInfo _descriptionColumn;
    private TableInfo _tableInfo;
    private Scintilla _scintilla;

    public override void SetDatabaseObject(IActivateItems activator, Lookup databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _keyColumn = databaseObject.PrimaryKey;
        _descriptionColumn = databaseObject.Description;
        _tableInfo = _keyColumn.TableInfo;

        lblCode.Text = _keyColumn.GetRuntimeName();
        lblDescription.Text = _descriptionColumn.GetRuntimeName();

        var factory = new ScintillaTextEditorFactory();
        _scintilla = factory.Create();

        gbScintilla.Controls.Add(_scintilla);

        try
        {
            SendQuery();
        }
        catch (System.Exception ex)
        {
            CommonFunctionality.Fatal("Could not connect to database", ex);
        }
    }

    public string GetCommand()
    {
        var repo = new MemoryCatalogueRepository();

        var qb = new QueryBuilder("distinct", null);
        qb.AddColumn(new ColumnInfoToIColumn(repo, _keyColumn) { Order = 0 });
        qb.AddColumn(new ColumnInfoToIColumn(repo, _descriptionColumn) { Order = 1 });
        qb.TopX = 100;


        var container = new SpontaneouslyInventedFilterContainer(repo, null, null, FilterContainerOperation.AND);

        if (!string.IsNullOrWhiteSpace(tbCode.Text))
        {
            var codeFilter = new SpontaneouslyInventedFilter(repo, container,
                $"{_keyColumn.GetFullyQualifiedName()} LIKE '{tbCode.Text}%'", "Key Starts", "", null);
            container.AddChild(codeFilter);
        }

        if (!string.IsNullOrWhiteSpace(tbDescription.Text))
        {
            var codeFilter = new SpontaneouslyInventedFilter(repo, container,
                $"{_descriptionColumn.GetFullyQualifiedName()} LIKE '%{tbDescription.Text}%'", "Description Contains",
                "", null);
            container.AddChild(codeFilter);
        }

        qb.RootFilterContainer = container;

        return qb.SQL;
    }

    private void tb_TextChanged(object sender, System.EventArgs e)
    {
        SendQuery();
    }

    private void SendQuery()
    {
        var tbl = _tableInfo.Discover(DataAccessContext.InternalDataProcessing);
        var server = tbl.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        var sql = GetCommand();

        _scintilla.ReadOnly = false;
        _scintilla.Text = sql;
        _scintilla.ReadOnly = true;

        var da = server.GetDataAdapter(sql, con);

        var dt = new DataTable();
        da.Fill(dt);

        dataGridView1.DataSource = dt;


        //set autosize mode
        foreach (DataGridViewColumn column in dataGridView1.Columns)
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
    }
}