// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using Diagnostics;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.TestsAndSetup
{
    internal enum ConnectionTypes
    {
        Logging,
        ANO,
        IdentifierDump,
        Master
    }

    /// <summary>
    /// This super technical  screen appears when things go irrecoverably wrong or when the user launches it from the CatalogueManager main menu.  It includes a variety of tests that 
    /// can be run on the current platform databases to check the contents/configurations.  It also includes 'Create Test Dataset' which can be used as a user acceptance test to make
    /// sure that their installation is successfully working (See UserManual.docx)
    /// 
    /// <para>The 'Evaluate MEF Exports' button is particularly useful if you are trying to debug a plugin you have written to find out why your class isn't appearing in the relevant part of
    /// the RDMP.</para>
    /// </summary>
    public partial class DiagnosticsUI : RDMPForm
    {
        private readonly Exception _originalException;
        
        public DiagnosticsUI(IActivateItems activator,Exception exception):base(activator)
        {
            _originalException = exception;

            InitializeComponent();
            
            if (_originalException != null)
                btnViewOriginalException.Enabled = true;
            
        }

        private void btnCatalogueFields_Click(object sender, EventArgs e)
        {
            StartChecking(new MissingFieldsChecker(MissingFieldsChecker.ThingToCheck.Catalogue, Activator.RepositoryLocator.CatalogueRepository, Activator.RepositoryLocator.DataExportRepository));
        }
        private void btnDataExportManagerFields_Click(object sender, EventArgs e)
        {
            StartChecking(new MissingFieldsChecker(MissingFieldsChecker.ThingToCheck.DataExportManager, Activator.RepositoryLocator.CatalogueRepository, Activator.RepositoryLocator.DataExportRepository));
        }

        private void btnCheckANOConfigurations_Click(object sender, EventArgs e)
        {
            StartChecking(new ANOConfigurationChecker(Activator.RepositoryLocator.CatalogueRepository));
        }
        private void btnCohortDatabase_Click(object sender, EventArgs e)
        {
            StartChecking(new CohortConfigurationChecker(Activator.RepositoryLocator.DataExportRepository));
        }
        private void btnListBadAssemblies_Click(object sender, EventArgs e)
        {
            StartChecking(new BadAssembliesChecker(Activator.RepositoryLocator.CatalogueRepository.MEF));
        }

        private void StartChecking(ICheckable checkable)
        {
            checksUI1.Clear();
            checksUI1.StartChecking(checkable);
        }
        
        private void btnViewOriginalException_Click(object sender, EventArgs e)
        {
            ExceptionViewer.Show(_originalException);
        }
        
        private void btnCatalogueCheck_Click(object sender, EventArgs e)
        {
            checksUI1.Clear();
            try
            {
                foreach (Catalogue c in Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
                    c.Check(checksUI1);
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Catalogue checking crashed completely", CheckResult.Fail,exception));
            }
        }
    }
}
