using System;
using System.Collections.Generic;
using Rdmp.Core.Repositories;
using RDMP.Avalonia.UI.Services.BootstrapService;
using RDMP.Avalonia.UI.Services.RepositoryLocatorService;
using ReactiveUI;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
namespace RDMP.Avalonia.UI.ViewModels
{
    public class RDMPSetupViewModel : ViewModelBase
    {

        public string Greeting => "This is a setup page - todo";

        public ICommand SubmitSetup { get; }

        public RDMPSetupViewModel()
        {
            SubmitSetup = ReactiveCommand.Create(() =>
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
        }

    }
}