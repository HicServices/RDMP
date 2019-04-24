// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.TestsAndSetup.StartupUI;
using Rdmp.Core.Startup;
using ReusableUIComponents.Dialogs;
using ScintillaNET;


namespace CatalogueManager.TestsAndSetup
{
    public class RDMPBootStrapper<T> where T : RDMPForm, new()
    {
        private readonly string catalogueConnection;
        private readonly string dataExportConnection;
        private T _mainForm;

        
        public RDMPBootStrapper(string catalogueConnection, string dataExportConnection)
        {
            this.catalogueConnection = catalogueConnection;
            this.dataExportConnection = dataExportConnection;
        }

        public void Show(bool requiresDataExportDatabaseToo)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Scintilla.SetDestroyHandleBehavior(true);

            //tell me when you blow up somewhere in the windows API instead of somewhere sensible
            Application.ThreadException += (sender, args) => ExceptionViewer.Show(args.Exception,false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => ExceptionViewer.Show((Exception)args.ExceptionObject,false);

            try
            {
                //show the startup dialog
                Startup startup = new Startup();
                if (!String.IsNullOrWhiteSpace(catalogueConnection) && !String.IsNullOrWhiteSpace(dataExportConnection))
                {
                    startup.RepositoryLocator = new LinkedRepositoryProvider(catalogueConnection, dataExportConnection);
                    startup.RepositoryLocator.CatalogueRepository.TestConnection();
                    startup.RepositoryLocator.DataExportRepository.TestConnection();
                }
                var startupUI = new StartupUIMainForm(startup);
                startupUI.ShowDialog();

                if (startup.RepositoryLocator == null)
                {
                    MessageBox.Show("Platform databases could not be reached.  Application will exit");
                    return;
                }

                //launch the main application form T
                _mainForm = new T();

                typeof (T).GetMethod("SetRepositoryLocator").Invoke(_mainForm,new object[]{startup.RepositoryLocator});

                if (startupUI.AppliedPatch)
                    return;

                Application.Run(_mainForm);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }
    }
}
