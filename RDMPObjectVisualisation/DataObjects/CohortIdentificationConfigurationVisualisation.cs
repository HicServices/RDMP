using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.QueryBuilding;
using ReusableUIComponents.SqlDialogs;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// TECHNICAL: Provides the visualisation of a cohort identification configuration (SQL that results in INTERSECT / UNION / EXCEPT of SETS of patient identifiers - each of which is an
    /// AggregateConfiguration).  If you see this component it means that the cohort identification query is available for use in the Pipeline e.g. you have chosen to execute the query and
    /// create a new cohort in data export manager.
    /// </summary>
    public partial class CohortIdentificationConfigurationVisualisation : UserControl
    {
        private readonly CohortIdentificationConfiguration _cic;
        private string _sql;
        private Exception _exception;

        public CohortIdentificationConfigurationVisualisation(CohortIdentificationConfiguration cic)
        {
            _cic = cic;
            InitializeComponent();

            lblID.Text = _cic.ID.ToString();
            lblName.Text = _cic.Name;

            CohortQueryBuilder builder = new CohortQueryBuilder(_cic);

            try
            {
                _sql = builder.SQL;
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PopupSQL();
        }

        private void CohortIdentificationConfigurationVisualisation_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PopupSQL();
        }

        private void lblName_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PopupSQL();
        }

        private void PopupSQL()
        {
            if (_sql != null)
                new SQLPreviewWindow("Cohort Identification Configuration",
                    "SQL that will be executed for '" + _cic +
                    "' if it looks like it will take a long time to run then you should consider caching some of the subqueries (See CohortManager.exe)",
                    _sql).ShowDialog();

        }
    }
}
