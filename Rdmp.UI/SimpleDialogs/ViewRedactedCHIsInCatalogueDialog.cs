using HICPlugin.Curation.Data;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class ViewRedactedCHIsInCatalogueDialog : Form
    {
        private bool _isLoading = true;
        private IBasicActivateItems _activator;
        private ICatalogue _catalogue;
        private DataTable _results;


        public ViewRedactedCHIsInCatalogueDialog(IBasicActivateItems activator, ICatalogue catalogue)
        {
            InitializeComponent();
            _activator = activator;
            _catalogue = catalogue;
            lblLoading.Visible = _isLoading;
            dtResults.Visible = !_isLoading;
            FindChis();
        }

        private void RevertButtonClick(int itemIndex)
        {
            var result = _results.Rows[itemIndex];
            var potentialCHI = result.ItemArray[0].ToString();
            var context = result.ItemArray[1].ToString();
            var column = result.ItemArray[3].ToString();
            var redactedChi = _catalogue.CatalogueRepository.GetAllObjects<RedactedCHI>().Where(rc => rc.PotentialCHI == potentialCHI && rc.CHIContext == context && rc.CHILocation == column).First();
            if (redactedChi is not null)
            {
                var cmd = new ExecuteCommandRevertRedactedCHI(_activator, redactedChi);
                cmd.Execute();
                result.Delete();
                _results.AcceptChanges();
                dtResults.DataSource = _results;
                dtResults.Columns[5].Visible = false;//todo this isn't quite right

            }
        }

        private void ConfirmButtonClick(int itemIndex)
        {
            var result = _results.Rows[itemIndex];
            var potentialCHI = result.ItemArray[0].ToString();
            var context = result.ItemArray[1].ToString();
            var column = result.ItemArray[3].ToString();
            var redactedChi = _catalogue.CatalogueRepository.GetAllObjects<RedactedCHI>().Where(rc => rc.PotentialCHI == potentialCHI && rc.CHIContext == context && rc.CHILocation == column).First();
            if (redactedChi is not null)
            {
                var cmd = new ExecuteCommandConfirmRedactedCHI(_activator, redactedChi);
                cmd.Execute();
                result.Delete();
                _results.AcceptChanges();
                dtResults.DataSource = _results;
                dtResults.Columns[5].Visible = false;//todo this isn't quite right

            }
        }

        private void handleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dtResults.Columns["Revert"].Index)
            {
                RevertButtonClick(e.RowIndex);
            }
            if (e.ColumnIndex == dtResults.Columns["Confirm"].Index)
            {
                ConfirmButtonClick(e.RowIndex);
            }

        }

        private void RevertAll(object sender, EventArgs e)
        {
            if (_activator.YesNo("Do you want to revert all these redactions?", "Revert All"))
            {
                foreach (var rIndex in Enumerable.Range(0,_results.Rows.Count))
                {
                    RevertButtonClick(rIndex);
                }
            }
        }
        private void ConfirmAll(object sender, EventArgs e)
        {
            if (_activator.YesNo("Do you want to confirm all these redactions?", "Confirm All"))
            {
                foreach (var rIndex in Enumerable.Range(0, _results.Rows.Count))
                {
                    ConfirmButtonClick(rIndex);
                }
            }
        }

        private string locationToColumn(string location)
        {
            var lastIdx = location.LastIndexOf('.');
            return location[(lastIdx + 1)..];
        }

        private void FindChis()
        {
            _isLoading = true;
            lblLoading.Visible = _isLoading;
            dtResults.Visible = !_isLoading;
            List<string> columns = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).Select(ci => ci.Name).ToList();
            List<RedactedCHI> redactedChis = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RedactedCHI>().Where(rc => columns.Contains(rc.CHILocation)).ToList();//



            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("Potental CHI", typeof(string)));
            dt.Columns.Add(new DataColumn("Context", typeof(string)));
            dt.Columns.Add(new DataColumn("Column", typeof(string)));
            dt.Columns.Add(new DataColumn("_hiddenFullLocation", typeof(string)));
            foreach (var rc in redactedChis)
            {
                dt.Rows.Add(new object[] { rc.PotentialCHI, rc.CHIContext, locationToColumn(rc.CHILocation), rc.CHILocation });
            }
            dtResults.DataSource = dt;
            dtResults.Columns[3].Visible = false;
            _results = dt;
            DataGridViewButtonColumn revertColumn = new DataGridViewButtonColumn();
            revertColumn.Text = "Revert";
            revertColumn.Name = "Revert";
            revertColumn.UseColumnTextForButtonValue = true;

            DataGridViewButtonColumn confirmColumn = new DataGridViewButtonColumn();
            confirmColumn.Text = "Confirm";
            confirmColumn.Name = "Confirm";
            confirmColumn.UseColumnTextForButtonValue = true;
            dtResults.CellClick += handleClick;
            if (dtResults.Columns["Revert"] == null)
            {
                dtResults.Columns.Insert(3, revertColumn);
            }
            if (dtResults.Columns["Confirm"] == null)
            {
                dtResults.Columns.Insert(4, confirmColumn);
            }
            _isLoading = false;
            lblLoading.Visible = _isLoading;
            dtResults.Visible = !_isLoading;
        }


        private void searchButtonClick(object sender, EventArgs e)
        {
            _isLoading = true;
            lblLoading.Visible = _isLoading;
            dtResults.Visible = !_isLoading;

            FindChis();
        }
    }
}
