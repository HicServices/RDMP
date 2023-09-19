// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Startup;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.TestsAndSetup;

public class RDMPBootStrapper
{
    private string _catalogueConnection;
    private string _dataExportConnection;

    /// <summary>
    /// The last used connection string arguments when launching using this factory class.  Typically the boot strapper
    /// should only ever be used once so you can safely query this field but best to check that it is not null anyway.
    /// </summary>
    public static ResearchDataManagementPlatformOptions ApplicationArguments;

    private readonly Func<IRDMPPlatformRepositoryServiceLocator, RDMPForm> _formConstructor;

    public RDMPBootStrapper(ResearchDataManagementPlatformOptions args,
        Func<IRDMPPlatformRepositoryServiceLocator, RDMPForm> constructor)
    {
        ApplicationArguments = args;
        _formConstructor = constructor;
    }

    private static readonly HashSet<string> IgnoreExceptions = new(StringComparer.InvariantCultureIgnoreCase)
    {
        // This error seems to come from ObjectTreeView but seems harmless
        "Value cannot be null. (Parameter 'owningItem')"
    };

    public void Show()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        //tell me when you blow up somewhere in the windows API instead of somewhere sensible
        Application.ThreadException += (s, e) =>
        {
            if (!IgnoreExceptions.Contains(e.Exception?.Message)) GlobalExceptionHandler.Instance.Handle(s, e);
        };
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler.Instance.Handle;

        try
        {
            ApplicationArguments.GetConnectionStrings(out var c, out var d);
            _catalogueConnection = c?.ConnectionString;
            _dataExportConnection = d?.ConnectionString;
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ApplicationArguments.ConnectionStringsFile))
            {
                var viewer = new ExceptionViewer("Failed to get connection strings",
                    $"ConnectionStringsFile was '{ApplicationArguments.ConnectionStringsFile}'{Environment.NewLine}{ExceptionHelper.ExceptionToListOfInnerMessages(ex)}",
                    ex);
                viewer.ShowDialog();
            }
            else
            {
                ExceptionViewer.Show("Failed to get connection strings", ex);
            }

            return;
        }

        try
        {
            //show the startup dialog
            var startup = new Startup { SkipPatching = ApplicationArguments.SkipPatching };

            if (!string.IsNullOrWhiteSpace(ApplicationArguments.Dir))
            {
                startup.RepositoryLocator = ApplicationArguments.GetRepositoryLocator();
            }
            else if (!string.IsNullOrWhiteSpace(_catalogueConnection) &&
                     !string.IsNullOrWhiteSpace(_dataExportConnection))
            {
                startup.RepositoryLocator = new LinkedRepositoryProvider(_catalogueConnection, _dataExportConnection);
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
            var mainForm = _formConstructor(startup.RepositoryLocator);

            if (startupUI.DoNotContinue)
                return;

            Application.Run(mainForm);
        }
        catch (Exception e)
        {
            if (!string.IsNullOrWhiteSpace(ApplicationArguments.ConnectionStringsFile))
                ExceptionViewer.Show($"Startup failed for '{ApplicationArguments.ConnectionStringsFile}'", e);
            else
                ExceptionViewer.Show(e);
        }
    }
}