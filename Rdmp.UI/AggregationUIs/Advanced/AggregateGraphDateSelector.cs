using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.AggregationUIs.Advanced
{
    public partial class AggregateGraphDateSelector : Form
    {
        public AggregateGraphDateSelector(string startDate = "", string endDate = "")
        {
          
            StartDate = startDate;
            EndDate = endDate;
            InitializeComponent();
            tbStartDate.Text = startDate;
            tbEndDate.Text = endDate;
            validateUpdate();
        }

        public string StartDate;
        public string EndDate;

        private bool validateDate(string date)
        {
            return DateRegex().Match(date.Trim()).Success;
        }


        private void validateUpdate()
        {
            var startDateValid = validateDate(tbStartDate.Text);
            var endDateValid = validateDate(tbEndDate.Text);
            if (!startDateValid)
            {
                tbStartDate.ForeColor = Color.Red;
            }
            else
            {
                tbStartDate.ForeColor = Color.Black;
            }
            if (!endDateValid)
            {
                tbEndDate.ForeColor = Color.Red;
            }
            else
            {
                tbEndDate.ForeColor = Color.Black;
            }
            if (!startDateValid || !endDateValid)
            {
                btnRefresh.Enabled = false;
            }
            else
            {
                btnRefresh.Enabled = true;
            }

        }

        private void AggregateGraphDateSelector_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void tbStartDate_TextChanged(object sender, EventArgs e)
        {
            StartDate = tbStartDate.Text;
            validateUpdate();

        }

        [GeneratedRegex("^'\\d{4}-(0?[1-9]|1[012])-(0?[1-9]|[12][0-9]|3[01])'$")]
        private static partial Regex DateRegex();

        private void tbEndDate_TextChanged(object sender, EventArgs e)
        {
            EndDate = tbEndDate.Text;
            validateUpdate();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
