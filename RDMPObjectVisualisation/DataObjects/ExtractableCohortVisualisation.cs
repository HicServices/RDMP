using System;
using System.Windows.Forms;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// Input object for visualizing existing cohorts.  Used by PipelineDiagram and ConfigureAndExecutePipeline
    /// </summary>
    public partial class ExtractableCohortVisualisation : UserControl
    {
        private readonly ExtractableCohort _value;

        public ExtractableCohortVisualisation(ExtractableCohort value)
        {
            _value = value;
            InitializeComponent();

            if(value == null)
                return;
            
            lblName.Text = value.GetExternalData().ExternalDescription;
            lblID.Text = "ID=" + value.ID + " (External ID=" + value.GetExternalData().ExternalVersion+")";
            lblSize.Text = "Size:"+ value.Count;

            this.Width = 20 + lblName.PreferredWidth;
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            ShowCohort();
        }

        private void ShowCohort()
        {
            try
            {
                var repository = _value.Repository;
                var externalCohortTable = repository.GetObjectByID<ExternalCohortTable>(_value.ExternalCohortTable_ID);
                string sql = "SELECT * FROM " + externalCohortTable.TableName +
                             Environment.NewLine
                             + " WHERE " + _value.WhereSQL();

                DataTableViewer viewer = new DataTableViewer(externalCohortTable, sql, "Cohort " + _value);
                viewer.Show();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}
