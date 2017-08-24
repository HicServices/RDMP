using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using CatalogueLibrary.Data;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace CatalogueManager.Issues
{
    /// <summary>
    /// Lets you create simple name/email records of users of the RDMP software, this is solely (currently) used by the Issue system and may be replaced in due course for 
    /// Windows User Accounts.
    /// </summary>
    public partial class SelectIssueSystemUser : RDMPForm
    {
        private IssueSystemUser _selectedUser;

        public SelectIssueSystemUser()
        {
            InitializeComponent();
            
        }

        public IssueSystemUser SelectedUser { get; set; }
        //this.DialogResult = DialogResult.OK;


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            RefresUIFromDatabase();
        }

        private void RefresUIFromDatabase()
        {
            IssueSystemUser[] issueSystemUsers = RepositoryLocator.CatalogueRepository.GetAllObjects<IssueSystemUser>().ToArray();
            

            lbUsers.ClearObjects();
            lbUsers.AddObjects(issueSystemUsers);
        }

        private bool bLoading;

        
        private void btnNewUser_Click(object sender, EventArgs e)
        {
            new IssueSystemUser(RepositoryLocator.CatalogueRepository);
            RefresUIFromDatabase();
        }

        private void lbUsers_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            var user = e.RowObject as IssueSystemUser;
            
            if(user != null)
            {
                e.Column.PutAspectByName(user, e.NewValue);
                user.SaveToDatabase(); 
            }
            
        }

        private void lbUsers_ItemActivate(object sender, EventArgs e)
        {
            SelectUserAndClose();
        }

        private void SelectUserAndClose()
        {
            SelectedUser = lbUsers.SelectedObject as IssueSystemUser;

            if (SelectedUser != null)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectUserAndClose();
        }

        private void lbUsers_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var deletable = lbUsers.SelectedObject as IDeleteable;

                if(deletable != null && MessageBox.Show("Are you sure you want to delete " + deletable + "?", "Confirm Delete?",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    deletable.DeleteInDatabase();
                    RefresUIFromDatabase();
                }

            }
        }
    }
}
