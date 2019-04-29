// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.Reports.DatabaseAccessPrivileges;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.SimpleDialogs.Reports
{
    /// <summary>
    /// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  In this context it is a good idea to know who has access to 
    /// the data.   This dialog assists you with that (if you need it) by helping you set up an SQL Agent Job which enumerates all the database tables and database users and audits the 
    /// access levels over time (giving a longitudinal history of user access).
    /// 
    /// <para>To use this functionality click 'Display Prerequisite SQL...' and run the script on the server you want to audit user permissions on.  This will create a database called Audit
    /// with several tables and stored procedures for auditing user access.  Test that it works by running 'exec Audit.dbo.UpdatePrivilegesAudit' and look at the contents of the table.</para>
    /// 
    /// <para>Next you should set up an SQL Agent job to run nightly/weekly that executes that stores procedure (Audit.dbo.UpdatePrivilegesAudit).</para>
    /// 
    /// <para>Once it has run once or twice you can use this dialog to Generate a word document report of user access either:
    /// Per User - What databases does each user have access to
    /// Per Database - What databases / explicit permissions are set on each database</para>
    /// 
    /// <para>TECHNICAL: All the information generated from the stored procedure comes from Sql Servers inbuilt user/database/permissions tables but these tables are not longitudinal which 
    /// is why we create the Audit database at all and need the periodic update.  If you don't want the database to be called Audit (e.g. because you already have a database called that)
    /// then you can do a find/replace on the Prerequisite SQL and set the Database correctly in this dialog when generating reports.</para>
    /// </summary>
    public partial class WordAccessRightsByUserUI : Form
    {
        public WordAccessRightsByUserUI()
        {
            InitializeComponent();
            serverDatabaseTableSelector1.HideTableComponents();
        }

        private void btnDisplayPrerequisites_Click(object sender, EventArgs e)
        {
            ShowSQLUI dialog = new ShowSQLUI(AccessRightsReportPrerequisites.SQL);
            dialog.Show();
        }

        private void btnGeneratePerUser_Click(object sender, EventArgs e)
        {

            try
            {
                WordAccessRightsByUser r = new WordAccessRightsByUser(serverDatabaseTableSelector1.GetDiscoveredDatabase(), cbCurrentUsersOnly.Checked);
                r.GenerateWordFile();

            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void btnGeneratePerDatabase_Click(object sender, EventArgs e)
        {

            try
            {
                WordAccessRightsByDatabase r = new WordAccessRightsByDatabase(serverDatabaseTableSelector1.GetDiscoveredDatabase());
                r.GenerateWordFile();
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void ConfigureAccessRightsReport_Load(object sender, EventArgs e)
        {
            serverDatabaseTableSelector1.Database = "Audit";
        }
    }
}
