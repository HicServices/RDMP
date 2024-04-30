// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Microsoft.Data.SqlClient;
using NLog.LayoutRenderers;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.ItemActivation;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class RedactChisInCatalogueDialog : Form
    {

        private readonly IActivateItems _activator;
        private readonly ICatalogue _catalogue;
        private DataTable _results;
        private bool _firstTime = true;
        private CHIRedactionHelpers redactionHelper;

        public RedactChisInCatalogueDialog(IActivateItems activator, ICatalogue catalogue)
        {
            InitializeComponent();
            _activator = activator;
            _catalogue = catalogue;
            dgResults.Visible = false;
            lbResults.Visible = false;
            lblNoResultsFound.Visible = false;
            redactionHelper = new CHIRedactionHelpers(activator, catalogue);


        }

        private void Redact(int rowIndex)
        {
            try
            {
                redactionHelper.Redact(_results.Rows[rowIndex]);
                _results.Rows[rowIndex].Delete();
                _results.AcceptChanges();
                dgResults.DataSource = _results;
            } catch { 
                //todo some warning about it having not worked
            }
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
                lblNoResultsFound.Visible = true;
                return;
            }
            lblNoResultsFound.Visible = false;
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
            confirmColumn.Dispose();
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
            lblNoResultsFound.Visible = false;
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
