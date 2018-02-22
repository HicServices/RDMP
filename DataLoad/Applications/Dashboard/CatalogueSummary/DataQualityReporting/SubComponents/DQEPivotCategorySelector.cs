using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using DataQualityEngine.Data;

namespace Dashboard.CatalogueSummary.DataQualityReporting.SubComponents
{
    /// <summary>
    /// Data Quality Engine records all validation results in a relational database, this includes recording with each result the Pivot column value found when evaluating the row.  A Pivot
    /// column is a single categorical field in the dataset that is the most useful way of slicing the dataset e.g. Healthboard.  If your dataset has a pivot column
    /// then this control will let you change which results are displayed in any IDataQualityReportingCharts from either All rows in the dataset or only 
    /// those where the pivot column has a specific value.  If your pivot column contains nulls then these records will only be audited under the ALL category.
    /// </summary>
    public partial class DQEPivotCategorySelector : UserControl
    {
        public event Action PivotCategorySelectionChanged;
        public string SelectedPivotCategory { get; private set; }

        public DQEPivotCategorySelector()
        {
            InitializeComponent();
        }

        public void LoadOptions(Evaluation evaluation)
        {
           flowLayoutPanel1.Controls.Clear();
           SelectedPivotCategory = "ALL";

           if (evaluation == null)
                return;

            foreach (string category in evaluation.GetPivotCategoryValues())
            {
                RadioButton rb = new RadioButton();
                rb.Tag = category;
                rb.Text = category;
                
                if(rb.Text== "ALL")
                    rb.Checked = true;//always select this one by default first
                    

                rb.CheckedChanged+= OnCheckedChanged;
                flowLayoutPanel1.Controls.Add(rb);
            }
        }

        private void OnCheckedChanged(object sender, EventArgs eventArgs)
        {
            SelectedPivotCategory = (string) ((RadioButton)sender).Tag;
            PivotCategorySelectionChanged();
        }

    }
}
