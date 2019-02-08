// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
