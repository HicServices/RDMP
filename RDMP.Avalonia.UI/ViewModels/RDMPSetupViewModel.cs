using System;
using System.Collections.Generic;
using Rdmp.Core.Repositories;
using RDMP.Avalonia.UI.Services.BootstrapService;
using RDMP.Avalonia.UI.Services.RepositoryLocatorService;
using ReactiveUI;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Collections.Specialized;
using Avalonia;
using NPOI.SS.Formula.Functions;
using Avalonia.Controls;

namespace RDMP.Avalonia.UI.ViewModels
{
    public class RDMPSetupViewModel : ReactiveObject
    {

        public string Greeting => "This is a setup page - todo";

        public ICommand SubmitSetup = ReactiveCommand.Create(() =>
        {
            RepositoryLocatorService repositoryLocatorService = new RepositoryLocatorService();
            try
            {
                repositoryLocatorService.StartScan("Server=(localdb)\\MSSQLLocalDB;Database=RDMP_Catalogue;Trusted_Connection=True;TrustServerCertificate=true;", "Server=(localdb)\\MSSQLLocalDB;Database=RDMP_DataExport;Trusted_Connection=True;TrustServerCertificate=true");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        });

        public RDMPSetupViewModel()
        {
        }

    }
}