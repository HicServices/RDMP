// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Rdmp.Core.Startup;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

using ScintillaNET;

namespace Rdmp.UI.TestsAndSetup
{
    public class RDMPBootStrapper<T> where T : RDMPForm, new()
    {
        private readonly EnvironmentInfo _environmentInfo;
        private readonly string catalogueConnection;
        private readonly string dataExportConnection;
        public static ResearchDataManagementPlatformOptions ApplicationArguments;
        private T _mainForm;

        
        public RDMPBootStrapper(EnvironmentInfo environmentInfo, ResearchDataManagementPlatformOptions args)
        {
            ApplicationArguments = args;
            args.GetConnectionStrings(out var c, out var d);

            this._environmentInfo = environmentInfo;
            this.catalogueConnection = c?.ConnectionString;
            this.dataExportConnection = d?.ConnectionString;

        }

        public HashSet<string> IgnoreExceptions = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase){ 
            
            // This error seems to come from ObjectTreeView but seems harmless
            "Value cannot be null. (Parameter 'owningItem')",
            };

        public void Show(bool requiresDataExportDatabaseToo)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Scintilla.SetDestroyHandleBehavior(true);

            //tell me when you blow up somewhere in the windows API instead of somewhere sensible
            Application.ThreadException += (s,e)=>{

                var msg = e.Exception?.Message;
                if(msg != null && IgnoreExceptions.Contains(msg))
                {
                    return;
                }

                GlobalExceptionHandler.Instance.Handle(s,e);
                };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler.Instance.Handle;

            try
            {
                //show the startup dialog
                Startup startup = new Startup(_environmentInfo);
                if (!String.IsNullOrWhiteSpace(catalogueConnection) && !String.IsNullOrWhiteSpace(dataExportConnection))
                {
                    startup.RepositoryLocator = new LinkedRepositoryProvider(catalogueConnection, dataExportConnection);
                    startup.RepositoryLocator.CatalogueRepository.TestConnection();
                    startup.RepositoryLocator.DataExportRepository.TestConnection();
                }
                var startupUI = new StartupUI(startup);
                startupUI.ShowDialog();

                if (startup.RepositoryLocator == null)
                {
                    MessageBox.Show("Platform databases could not be reached.  Application will exit");
                    return;
                }

                //launch the main application form T
                _mainForm = new T();

                typeof (T).GetMethod("SetRepositoryLocator").Invoke(_mainForm,new object[]{startup.RepositoryLocator});

                if (startupUI.DoNotContinue)
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
