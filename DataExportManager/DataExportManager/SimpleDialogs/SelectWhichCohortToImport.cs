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
    /// Allows you to select a cohort from your Cohort Database which is not yet been imported into the RDMP.  The RDMP lets you import cohorts directly into the cohort database through
    /// its user interface in which case a reference is created to the cohort in the cohort database once it has been successfully committed (so you won't need this dialog) (See 
    /// CohortCreationRequestUI).
    /// 
    /// Only use this dialog if you have manually added a cohort into your cohort database yourself and RDMP does not show it in SavedCohortsCollectionUI.
    /// </summary>
    public partial class SelectWhichCohortToImport : RDMPForm
    {
        private readonly ExternalCohortTable _source;
        public int IDToImport { get; private set; }

        private readonly string _projectNumberMemberName;
        private readonly string _versionMemberName;
        private readonly string _displayMember;
        private readonly string _valueMember;

        public SelectWhichCohortToImport(ExternalCohortTable source)
        {
            _source = source;
            InitializeComponent();
            
            if(source == null)
                return;

            
            DataTable dt = ExtractableCohort.GetImportableCohortDefinitionsTable(source,out _displayMember,out _valueMember,out _versionMemberName, out _projectNumberMemberName);

            dataGridView1.DataSource = dt;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                MessageBox.Show("Select a cohort to import or click Cancel");
                return;
            }

            var idUserPlansToImport = (int) dataGridView1.SelectedRows[0].Cells[_valueMember].Value;

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

                if (string.IsNullOrWhiteSpace(tbFilter.Text))
                {
                    lblFilteringBy.Text = "";
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = null;
                    return;
                }
                
                int? number = null;

                try
                {
                    number = int.Parse(tbFilter.Text);
                }
                catch (Exception)
                {
                    //it's not a number
                    number = null;
                }

                string filter = null;


                //filter is numerical
                if (number != null)
                {
                    filter = string.Format("{0} LIKE '%{3}%' OR {1} = {3} OR {2} = {3}",
                        _displayMember,
                        _projectNumberMemberName,
                        _valueMember,
                        number);

                    lblFilteringBy.Text = "Filtering by Number";
                }
                    else
                {
                    filter = string.Format("{0} LIKE '%{1}%'",
                        _displayMember,
                        tbFilter.Text);

                    lblFilteringBy.Text = "Filtering by Text";
                }
                
                ((DataTable) dataGridView1.DataSource).DefaultView.RowFilter = filter;
                
                tbFilter.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbFilter.ForeColor = Color.Red;
                lblFilteringBy.Text = "Filter Error";
            }
        }


    }
}
