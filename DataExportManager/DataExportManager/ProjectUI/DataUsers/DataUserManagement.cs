using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;


namespace DataExportManager.ProjectUI.DataUsers
{
    /// <summary>
    /// Allows you to configure the names of researchers who will be receiving project extractions (forename, surname, email).  Each project can have zero or more users registered as
    /// using the project data.  This has no effect other than documenting them and including their names as users in the Release Document (generated when a project extraction is released).
    /// 
    /// <para>You can safely skip doing this if you have other policies in place for documenting/managing project coordinators and data users.</para>
    /// </summary>
    public partial class DataUserManagement : Form
    {
        private DataExportRepository repository;

        public DataUserManagement(Project project)
        {
            Project = project;
            

            InitializeComponent();

            if(project == null)
                return;

            repository = (DataExportRepository)project.Repository;

            RefreshUIFromDatabase();
        }

        protected Project Project { get; set; }

        private void lbKnownUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = lbKnownUsers.SelectedObject != null;
        }

        private void lbUsersRegisteredOnProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = lbUsersRegisteredOnProject.SelectedItem != null;
        }

        private void RefreshUIFromDatabase()
        {
            lbKnownUsers.ClearObjects();
            lbUsersRegisteredOnProject.Items.Clear();

            IEnumerable<DataUser> availableUsers = repository.GetAllObjects<DataUser>();

            lbUsersRegisteredOnProject.Items.AddRange(Project.DataUsers.ToArray());
            lbKnownUsers.AddObjects(availableUsers.ToArray());

            btnAdd.Enabled = false;
            btnRemove.Enabled = false;
        }
        
        private void btnAddNewUser_Click(object sender, EventArgs e)
        {
            new DataUser((IDataExportRepository)Project.Repository,Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            RefreshUIFromDatabase();
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            var user = lbKnownUsers.SelectedObject as DataUser;
            if (user != null)
                AddToProject(user);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var user = lbUsersRegisteredOnProject.SelectedItem as DataUser;
            if(user != null)
                RemoveFromProject(user);
        }

        private void AddToProject(DataUser dataUser)
        {
            try
            {
                //if user is not yet on project
                if (!Project.DataUsers.Contains(dataUser))
                {
                    dataUser.RegisterAsDataUserOnProject(Project);
                    RefreshUIFromDatabase();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        private void RemoveFromProject(DataUser dataUser)
        {
            try
            {
                //if user is in project (they might just have dragged him off of the lbKnownUsers and back onto it!
                if (Project.DataUsers.Contains(dataUser))
                {
                    dataUser.UnRegisterAsDataUserOnProject(Project);
                    RefreshUIFromDatabase();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void lbKnownUsers_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            var saveable = (ISaveable)e.RowObject;
            e.Column.PutAspectByName(e.RowObject,e.Value);
            saveable.SaveToDatabase();
        }

        private void lbKnownUsers_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var deletable = lbKnownUsers.SelectedObject as IDeleteable;

                if (deletable != null && MessageBox.Show("Are you sure you want to delete " + deletable + "?", "Confirm Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    deletable.DeleteInDatabase();
                    RefreshUIFromDatabase();
                }

            }

        }

    }
}
