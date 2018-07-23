using System.Data.SqlClient;
using CatalogueLibrary.Repositories;
using CommandLine;
using RDMPAutomationService.Properties;
using RDMPStartup;
using ReusableLibraryCode.Checks;

namespace RDMPAutomationService.Options.Abstracts
{
    /// <summary>
    /// Abstract base class for all command line options that can be supplied to the rdmp cli (includes overriding app.config to get connection strings to platform metadata databases)
    /// </summary>
    public abstract class RDMPCommandLineOptions
    {
        private LinkedRepositoryProvider _repositoryLocator;

        [Option(Required = false, HelpText = @"Name of the Metadata server (where Catalogue and Data Export are stored) e.g. localhost\sqlexpress")]
        public string ServerName { get; set; }

        [Option(Required = false, HelpText = "Name of the Catalogue database e.g. RDMP_Catalogue")]
        public string CatalogueDatabaseName { get; set; }

        [Option( Required = false, HelpText = "Name of the Data Export database e.g. RDMP_DataExport")]
        public string DataExportDatabaseName { get; set; }

        [Value(0, HelpText = @"Command to run on the engine: 'run' or 'check' ", Required = true)]
        public CommandLineActivity Command { get; set; }

        public void LoadFromAppConfig()
        {
            if (ServerName == null)
                ServerName = Settings.Default.ServerName;

            if(CatalogueDatabaseName == null)
                CatalogueDatabaseName = Settings.Default.CatalogueDB;

            if(DataExportDatabaseName == null)
                DataExportDatabaseName = Settings.Default.DataExportDB;
        }
        public IRDMPPlatformRepositoryServiceLocator DoStartup()
        {
            Startup startup = new Startup(GetRepositoryLocator());
            startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
            return startup.RepositoryLocator;
        }

        public virtual IRDMPPlatformRepositoryServiceLocator GetRepositoryLocator()
        {
            if(_repositoryLocator == null)
            {
                var c = new SqlConnectionStringBuilder();
                c.DataSource = ServerName;
                c.IntegratedSecurity = true;
                c.InitialCatalog = CatalogueDatabaseName;

                SqlConnectionStringBuilder d = null;
                if (DataExportDatabaseName != null)
                {
                    d = new SqlConnectionStringBuilder();
                    d.DataSource = ServerName;
                    d.IntegratedSecurity = true;
                    d.InitialCatalog = DataExportDatabaseName;
                }

                CatalogueRepository.SuppressHelpLoading = true;

                _repositoryLocator = new LinkedRepositoryProvider(c.ConnectionString, d != null ? d.ConnectionString : null);
            }

            return _repositoryLocator;
        }
    }
}
