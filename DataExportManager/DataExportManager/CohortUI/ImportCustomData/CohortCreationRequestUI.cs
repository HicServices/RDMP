using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTableUI;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using Point = System.Drawing.Point;

namespace DataExportManager.CohortUI.ImportCustomData
{
    /// <summary>
    /// Once you have created a cohort database, this dialog lets you upload a new cohort into it.  You will already have selected a file which contains the private patient identifiers of
    /// those you wish to be in the cohort.  Next you must create or choose an existing Project for which the cohort belongs.  
    /// 
    /// Once you have chosen the project you can choose to either create a new cohort for use with the project (use this if you have multiple cohorts in the project e.g. 'Cases' and 
    /// 'Controls').  Or 'Revised version of existing cohort' for if you made a mistake with your first version of a cohort or if you are doing a refresh of the cohort (e.g. after 5 years
    /// it is likely there will be different patients that match the research study criteria so a new version of the cohort is appropriate).
    /// </summary>
    public partial class CohortCreationRequestUI : RDMPForm
    {
        private readonly ExternalCohortTable _target;
        private DataExportRepository _repository;

        public CohortCreationRequestUI(ExternalCohortTable target, Project project =null)
        {
            _target = target;

            InitializeComponent();
            
            if (_target == null)
                return;

            _repository = (DataExportRepository)_target.Repository;

            lblExternalCohortTable.Text = _target.ToString();

            SetProject(project);
            
            pbProject.Image = CatalogueIcons.Project;
            pbCohortSource.Image = CatalogueIcons.ExternalCohortTable;
        }


        public CohortCreationRequest Result { get; set; }
        public IActivateItems Activator { get; set; }
        public Project Project { get; set; }

        private void btnOk_Click(object sender, EventArgs e)
        {

            if (Project == null)
            {
                MessageBox.Show("You must select a project, if you do not have one yet then create one");
                return;
            }

            if (Project.ProjectNumber == null)
            {
                MessageBox.Show("Project " + Project +
                                " does not have a project number yet, you must asign it one before it can be involved in cohort creation");
                return;
            }

            string name;
            int version;
            if (rbNewCohort.Checked)
            {
                name = tbName.Text;
                version = 1;

                if (string.IsNullOrWhiteSpace(tbName.Text))
                {
                    MessageBox.Show("You must enter a name for your cohort");
                    return;
                }

            }
            else if (rbRevisedCohort.Checked)
            {

                var existing = ddExistingCohort.SelectedItem as CohortDefinition;
                if (existing == null)
                {
                    MessageBox.Show("You must select an existing cohort");
                    return;
                }

                name = existing.Description;
                version = int.Parse(lblNewVersionNumber.Text);
            }
            else
            {
                MessageBox.Show("Select either new or existing cohort");
                return;
            }

            
            //construct the result
            Result = new CohortCreationRequest(Project, new CohortDefinition(null, name, version, (int)Project.ProjectNumber, _target), (DataExportRepository)Project.Repository, tbDescription.Text); 

            //see if it is passing checks
            ToMemoryCheckNotifier notifier = new ToMemoryCheckNotifier();
            Result.Check(notifier);
            if (notifier.GetWorst() == CheckResult.Success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                //if it is not passing checks display the results of the failing checking
                ragSmiley1.Reset();
                Result.Check(ragSmiley1);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Result = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void rbNewCohort_CheckedChanged(object sender, EventArgs e)
        {
            gbNewCohort.Enabled = true;
            gbRevisedCohort.Enabled = false;
        }

        private void rbRevisedCohort_CheckedChanged(object sender, EventArgs e)
        {
            gbNewCohort.Enabled = false;
            gbRevisedCohort.Enabled = true;

            
            RefreshCohortsDropdown();
        }

        private void CohortCreationRequestUI_Load(object sender, EventArgs e)
        {
            _target.Check(ragSmiley1);
        }

        private void ddExistingCohort_SelectedIndexChanged(object sender, EventArgs e)
        {
            var def = ddExistingCohort.SelectedItem as CohortDefinition;

            if (def != null)
                lblNewVersionNumber.Text = (def.Version + 1).ToString();
        }

        private void cbShowEvenWhenProjectNumberDoesntMatch_CheckedChanged(object sender, EventArgs e)
        {
            RefreshCohortsDropdown();
        }
        private void RefreshCohortsDropdown()
        {
            ddExistingCohort.Items.Clear();

            var cohorts = ExtractableCohort.GetImportableCohortDefinitions(_target).ToArray();
            var maxVersionCohorts = cohorts.Where(c => //get cohorts where
                !cohorts.Any(c2 => c2.Description.Equals(c.Description) //there are not any other cohorts with the same name
                    && c2.Version > c.Version)//and a higher version
                    ).ToArray();

            if (Project == null)
            {
                MessageBox.Show("You must select a Project");
                return;
            }

            if(cbShowEvenWhenProjectNumberDoesntMatch.Checked)
                ddExistingCohort.Items.AddRange(maxVersionCohorts);
            else
                ddExistingCohort.Items.AddRange(maxVersionCohorts.Where(c => c.ProjectNumber == Project.ProjectNumber).ToArray());
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            try
            {
                ProjectUI.ProjectUI p = new ProjectUI.ProjectUI();
                RDMPForm dialog = new RDMPForm();

                p.SwitchToCutDownUIMode();

                Button ok = new Button();
                ok.Click += (s, ev) => { dialog.Close(); dialog.DialogResult = DialogResult.OK; };
                ok.Location = new Point(0,p.Height + 10);
                ok.Width = p.Width/2;
                ok.Height = 30;
                ok.Text = "Ok";

                Button cancel = new Button();
                cancel.Click += (s, ev) =>{dialog.Close();dialog.DialogResult = DialogResult.Cancel;};
                cancel.Location = new Point(p.Width / 2, p.Height + 10);
                cancel.Width = p.Width / 2;
                cancel.Height = 30;
                cancel.Text = "Cancel";

                dialog.Controls.Add(ok);
                dialog.Controls.Add(cancel);
                    
                dialog.Height = p.Height + 80;
                dialog.Width = p.Width + 10;
                dialog.Controls.Add(p);

                dialog.RepositoryLocator = RepositoryLocator;

                ok.Anchor = AnchorStyles.Bottom;
                cancel.Anchor  = AnchorStyles.Bottom;
                    
                p.Project = new Project(_repository,"New Project");
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    p.Project.SaveToDatabase();
                    SetProject(p.Project);
                    Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(p.Project));
                }
                else
                    p.Project.DeleteInDatabase();

            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void SetProject(Project project)
        {
            Project = project;

            lblProject.Text = Project != null ? Project.Name : "????";
            
            btnNewProject.Left = lblProject.Right;
            btnExisting.Left = btnNewProject.Right;

            btnClear.Left = lblProject.Right;

            btnNewProject.Visible = Project == null;
            btnExisting.Visible = Project == null;
            btnClear.Visible = Project != null;
            
            //if a project is selected and the project has no project number
            lblErrorNoProjectNumber.Visible = Project != null && Project.ProjectNumber == null;
            tbSetProjectNumber.Visible = Project != null && Project.ProjectNumber == null;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnExisting_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(RepositoryLocator.DataExportRepository.GetAllObjects<Project>(), false, false);
            if(dialog.ShowDialog()== DialogResult.OK)
                SetProject((Project)dialog.Selected);
        }

        private void tbSetProjectNumber_TextChanged(object sender, EventArgs e)
        {
            try
            {
                tbSetProjectNumber.ForeColor = Color.Black;
                int newProjectNumber = int.Parse(tbSetProjectNumber.Text);
                Project.ProjectNumber = newProjectNumber;
                Project.SaveToDatabase();
            }
            catch (Exception exception)
            {
                tbSetProjectNumber.ForeColor = Color.Red;
            }

            lblErrorNoProjectNumber.ForeColor = tbSetProjectNumber.ForeColor;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SetProject(null);
        }
        
    }
}
