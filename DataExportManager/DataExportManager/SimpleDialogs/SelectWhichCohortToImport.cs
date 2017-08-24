using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using ReusableUIComponents;


namespace DataExportManager.SimpleDialogs
{
    /// <summary>
    /// Allows you to select a cohort form your Cohort Database which is not yet been imported into the RDMP.  The RDMP lets you import cohorts directly into the cohort database through
    /// its user interface in which case a reference is created to the cohort in the cohort database once it has been successfully committed (so you won't need this dialog) (See 
    /// CohortCreationRequestUI).
    /// 
    /// Only use this dialog if you have manually added a cohort into your cohort database yourself and RDMP does not show it in ExtractableCohortManagementUI.
    /// </summary>
    public partial class SelectWhichCohortToImport : RDMPForm
    {
        private readonly ExternalCohortTable _source;
        public int IDToImport { get; private set; }

        string displayMember;
        string valueMember;

        public SelectWhichCohortToImport(ExternalCohortTable source)
        {
            _source = source;
            InitializeComponent();
            
            if(source == null)
                return;

            string projectNumberMemberName,versionMemberName;
            DataTable dt = ExtractableCohort.GetImportableCohortDefinitionsTable(source,out displayMember,out valueMember,out versionMemberName, out projectNumberMemberName);

            dataGridView1.DataSource = dt;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                MessageBox.Show("Select a cohort to import or click Cancel");
                return;
            }

            var idUserPlansToImport = (int) dataGridView1.SelectedRows[0].Cells[valueMember].Value;

            var existing = RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableCohort>("WHERE ExternalCohortTable_ID = " + _source.ID + " AND OriginID =" + idUserPlansToImport);

            if (existing.Any())
            {
                MessageBox.Show("That cohort has already been imported");
                return;
            }

            IDToImport = idUserPlansToImport;

            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            try
            {

                int? id = null;

                try
                {
                    id = int.Parse(tbFilter.Text);
                }
                catch (Exception)
                {
                }

                string filter = string.Format("{0} LIKE '%{1}%'", displayMember, tbFilter.Text);

                if(id!= null)
                    filter +=  string.Format(" OR {0} = {1}", valueMember, id);

                ((DataTable) dataGridView1.DataSource).DefaultView.RowFilter = filter;
                
                tbFilter.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbFilter.ForeColor = Color.Red;
            }
        }


    }
}
