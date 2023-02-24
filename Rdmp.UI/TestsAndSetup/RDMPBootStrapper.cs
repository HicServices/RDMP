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
using ReusableLibraryCode;
using ScintillaNET;

namespace Rdmp.UI.TestsAndSetup;

public class RDMPBootStrapper<T> where T : RDMPForm, new()
{
    private readonly EnvironmentInfo _environmentInfo;
    private string catalogueConnection;
    private string dataExportConnection;
        
    /// <summary>
    /// The last used connection string arguments when launching using this factory class.  Typically the boot strapper
    /// should only ever be used once so you can safely query this field but best to check that it is not null anyway.
    /// </summary>
    public static ResearchDataManagementPlatformOptions ApplicationArguments;
    private readonly ResearchDataManagementPlatformOptions _args;
    private T _mainForm;

        
    public RDMPBootStrapper(EnvironmentInfo environmentInfo, ResearchDataManagementPlatformOptions args)
    {
        ApplicationArguments = args;
        _args = args;
        _environmentInfo = environmentInfo;

    }

    public HashSet<string> IgnoreExceptions = new(StringComparer.CurrentCultureIgnoreCase){ 
            
        // This error seems to come from ObjectTreeView but seems harmless
        "Value cannot be null. (Parameter 'owningItem')"
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
            _args.GetConnectionStrings(out var c, out var d);
            catalogueConnection = c?.ConnectionString;
            dataExportConnection = d?.ConnectionString;
        }
        catch (Exception ex)
        {
            if(!string.IsNullOrWhiteSpace(_args.ConnectionStringsFile))
            {
                var viewer = new ExceptionViewer("Failed to get connection strings", $"ConnectionStringsFile was '{_args.ConnectionStringsFile}'{Environment.NewLine}{ExceptionHelper.ExceptionToListOfInnerMessages(ex)}", ex);
                viewer.ShowDialog();
            }
            else
                ExceptionViewer.Show("Failed to get connection strings", ex);
            return;
        }

        try
        {
            //show the startup dialog
            var startup = new Startup(_environmentInfo) { SkipPatching = _args.SkipPatching };

            if(!string.IsNullOrWhiteSpace(_args.Dir))
            {
                startup.RepositoryLocator = _args.GetRepositoryLocator();
            }
            else
            if (!string.IsNullOrWhiteSpace(catalogueConnection) && !string.IsNullOrWhiteSpace(dataExportConnection))
            {
                startup.RepositoryLocator = new LinkedRepositoryProvider(catalogueConnection, dataExportConnection);
                startup.RepositoryLocator.CatalogueRepository.TestConnection();
                startup.RepositoryLocator.DataExportRepository.TestConnection();
            }

            var startupUI = new StartupUI(startup);
            startupUI.ShowDialog();

            if (startupUI.CouldNotReachTier1Database)
            {
                MessageBox.Show("Platform databases could not be reached.");
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
            if (!string.IsNullOrWhiteSpace(_args.ConnectionStringsFile))
            {
                ExceptionViewer.Show($"Startup failed for '{_args.ConnectionStringsFile}'",e);
            }
            else
                ExceptionViewer.Show(e);
            return;
        }
    }
}