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

        private string _CatalogueConnectionString ="Server=(localdb)\\MSSQLLocalDB;Database=RDMP_Catalogue;Trusted_Connection=True;TrustServerCertificate=true;";
        private string _DataExportConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=RDMP_DataExport;Trusted_Connection=True;TrustServerCertificate=true";

        public string CatalogueConnectionString { get => _CatalogueConnectionString; set => _CatalogueConnectionString = value; }
        public string DataExportConnectionString { get => _DataExportConnectionString; set => _DataExportConnectionString = value; }

        public void SubmitSetup()
        {
            RepositoryLocatorService repositoryLocatorService = new RepositoryLocatorService();
            try
            {
                repositoryLocatorService.StartScan(_CatalogueConnectionString, _DataExportConnectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public RDMPSetupViewModel()
        {

        }

    }
}