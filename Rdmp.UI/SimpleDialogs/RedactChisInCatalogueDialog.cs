// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Microsoft.Data.SqlClient;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class RedactChisInCatalogueDialog : Form
    {

        private IActivateItems _activator;
        private ICatalogue _catalogue;
        private DataTable _results;
        private bool _firstTime = true;

        public RedactChisInCatalogueDialog(IActivateItems activator, ICatalogue catalogue)
        {
            InitializeComponent();
            _activator = activator;
            _catalogue = catalogue;
            dgResults.Visible = false;
            lbResults.Visible = false;

        }

        private void Redact(int rowIndex)
        {
            var result = _results.Rows[rowIndex];
            var foundChi = result.ItemArray[0].ToString();
            var columnValue = result.ItemArray[1].ToString();
            var column = result.ItemArray[2].ToString();
            var catalogueItem = _catalogue.CatalogueItems.Where(ci => ci.Name == column).First();
            var name = catalogueItem.ColumnInfo.Name;
            var pkValue = result.ItemArray[3].ToString();
            var replacementIdex = int.Parse(result.ItemArray[5].ToString());
            var table = name.Replace($".[{column}]", "");
            var pkColumn = result.ItemArray[4].ToString().Replace(table,"").Replace(".","");
            var rc = new RedactedCHI(_catalogue.CatalogueRepository, foundChi, replacementIdex, table, pkValue,pkColumn, $"[{column}]");
            rc.SaveToDatabase();

            var redactedString = new string('#', foundChi.Length);
            var fetchSQL = $"select TOP 1 {column} from {table} where {pkColumn} = '{pkValue}' and charindex('{foundChi}',{column},{replacementIdex}) >0";
            var existingResultsDT = new DataTable();
            var columnInfo = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name == $"{table}.[{column}]").First();
            var catalogue = columnInfo.CatalogueItems.FirstOrDefault().Catalogue;
            using (var con = (SqlConnection)catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).GetConnection())
            {
                con.Open();
                var da = new SqlDataAdapter(new SqlCommand(fetchSQL, con));
                da.Fill(existingResultsDT);
                if (existingResultsDT.Rows.Count > 0 && existingResultsDT.Rows[0].ItemArray.Length > 0)
                {
                    var currentContext = existingResultsDT.Rows[0].ItemArray[0].ToString();
                    var newContext = currentContext.Replace(foundChi, redactedString);
                    var updateSQL = $"update {table} set {column}='{newContext}' where {pkColumn} = '{pkValue}' and {column}='{currentContext}'";
                    var updateCmd = new SqlCommand(updateSQL, con);
                    updateCmd.ExecuteNonQuery();
                }
            }


            result.Delete();
            _results.AcceptChanges();
            dgResults.DataSource = _results;
        }

        private void handleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgResults.Columns["Redact"].Index)
            {
                Redact(e.RowIndex);
            }
        }


        private void ShowResults()
        {
            if(_results.Rows.Count ==0) {
                //TODO show a 'no results found' message
                return;
            }
            dgResults.DataSource = _results;
            DataGridViewButtonColumn confirmColumn = new DataGridViewButtonColumn();
            confirmColumn.Text = "Redact";
            confirmColumn.Name = "Redact";
            confirmColumn.UseColumnTextForButtonValue = true;
            dgResults.CellClick += handleClick;
            if (dgResults.Columns["Redact"] == null)
            {
                dgResults.Columns.Insert(3, confirmColumn);
            }
            if (_firstTime)
            {
                dgResults.Columns[4].Visible = false;
                dgResults.Columns[5].Visible = false;
                dgResults.Columns[6].Visible = false;
                _firstTime = false;
            }
            dgResults.Visible = true;
            lbResults.Visible = false;
        }

        private void FindCHIs(object sender, EventArgs e)
        {
            dgResults.Visible = false;
            lbResults.Visible = false;
            if (this.cbDoRedaction.Checked && _activator.YesNo("Are you sure you want to blindly redact?", "Redact Catalogue"))
            {
                var cmd = new ExecuteCommandRedactCHIsFromCatalogue(_activator, _catalogue, tbAllowList.Text is not null ? tbAllowList.Text : "");
                cmd.Execute();
                lbResults.Text = $"Redacted {cmd.redactionCount} CHIs in the catalogue";
                dgResults.Visible = false;
                lbResults.Visible = true;
            }
            else
            {
                var cmd = new ExecuteCommandIdentifyCHIInCatalogue(_activator, _catalogue, false, tbAllowList.Text is not null ? tbAllowList.Text : "");
                cmd.Execute();
                _results = cmd.foundChis;
                ShowResults();
            }

        }

    }



}
