using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using CommandLine;
using CommandLine.Text;
using RDMPStartup;

namespace RDMPAutomationService
{
    public class AutomationServiceOptions
    {
        [Option('s', @"localhost\sqlexpress", Required = true,
   HelpText = @"Name of the Metadata server (where Catalogue and Data Export are stored) e.g. localhost\sqlexpress")]
        public string ServerName { get; set; }

        [Option('c', "Catalogue", Required = true,
   HelpText = "Name of the Catalogue database e.g. RDMP_Catalogue")]
        public string CatalogueDatabaseName { get; set; }

        [Option('e', "DataExport", Required = false,
   HelpText = "Name of the Data Export database e.g. RDMP_DataExport")]
        public string DataExportDatabaseName { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public IRDMPPlatformRepositoryServiceLocator GetRepositoryLocator()
        {
            var c = new SqlConnectionStringBuilder();
            c.DataSource = ServerName;
            c.IntegratedSecurity = true;
            c.InitialCatalog = CatalogueDatabaseName;

            SqlConnectionStringBuilder d = null;
            if(DataExportDatabaseName != null)
            {
                d = new SqlConnectionStringBuilder();
                d.DataSource = ServerName;
                d.IntegratedSecurity = true;
                d.InitialCatalog = DataExportDatabaseName;
            }
            
            CatalogueRepository.SuppressHelpLoading = true;

            return new LinkedRepositoryProvider(c.ConnectionString,d != null ? d.ConnectionString:null);
        }
    }
}
