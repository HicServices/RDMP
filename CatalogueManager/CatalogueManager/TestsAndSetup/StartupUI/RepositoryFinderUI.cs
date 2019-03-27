// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.LocationsMenu;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableLibraryCode.Checks;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    /// <summary>
    /// Part of StartupUIMainForm, this control is visible only in the 'Technical' view.  It describes the state of the users connection strings for RDMP.  This is how the software finds
    /// the connection strings for tier 1 databases (Data Catalogue and optionally Data Export Manager).  Clicking 'Change...' will launch a dialog that lets you change the connection strings
    /// or create new catalogue / data export manager databases
    /// 
    /// <para>Clicking 'Change...' will launch ChoosePlatformDatabases.</para>
    /// </summary>
    public partial class RepositoryFinderUI : UserControl
    {
        private UserSettingsRepositoryFinder _finder;
        private IRDMPPlatformRepositoryServiceLocator _locator;
        
        public RepositoryFinderUI()
        {
            InitializeComponent();
        }

        public void SetRepositoryFinder(UserSettingsRepositoryFinder finder)
        {
            _finder = finder;
            _locator = finder;

            //set these because calls to .CatalogueRepository property on finder can fail because of late load/dodgy connection string formats etc
            lblCatalogue.ForeColor = Color.Red;
            lblCatalogue.Text = "Catalogue:Broken";
            lblExport.ForeColor = Color.Red;
            lblExport.Text = "Export:Broken";


            lblCatalogue.Text = "Catalogue:" + Describe((CatalogueRepository) _finder.CatalogueRepository, lblCatalogue);
            lblCatalogue.ForeColor = Color.Black;

            lblExport.Text = "Export:" + Describe((DataExportRepository)_finder.DataExportRepository, lblExport);
            lblExport.ForeColor = Color.Black;
        }

        private string Describe(TableRepository repository, Label lbl)
        {
            if (repository == null)
            {
                lbl.ForeColor = Color.Red;
                return "Not Set";
            }
            
            var builderAsMsOnly = ((SqlConnectionStringBuilder)repository.ConnectionStringBuilder);
            
            lbl.ForeColor = Color.Black;
            return builderAsMsOnly.InitialCatalog + " (" + builderAsMsOnly.DataSource + ")";
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            new ExecuteCommandChoosePlatformDatabase(_locator).Execute();
        }

        public void SetRepositoryFinder(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _locator = repositoryLocator;

            //set these because calls to .CatalogueRepository property on finder can fail because of late load/dodgy connection string formats etc
            lblCatalogue.ForeColor = Color.Red;
            lblCatalogue.Text = "Catalogue:Broken";
            lblExport.ForeColor = Color.Red;
            lblExport.Text = "Export:Broken";


            lblCatalogue.Text = "Catalogue:" + Describe((CatalogueRepository) repositoryLocator.CatalogueRepository, lblCatalogue);
            lblCatalogue.ForeColor = Color.Black;

            lblExport.Text = "Export:" + Describe((DataExportRepository)repositoryLocator.DataExportRepository, lblExport);
            lblExport.ForeColor = Color.Black;
        }
    }
}
