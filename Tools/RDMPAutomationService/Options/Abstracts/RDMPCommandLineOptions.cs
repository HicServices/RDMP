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
        
        [Option(Required = false, HelpText = "Full connection string to the Catalogue database, this overrides CatalogueDatabaseName and allows custom ports, username/password etc")]
        public string CatalogueConnectionString { get; set; }

        [Option(Required = false, HelpText = "Full connection string to the DataExport database, this overrides DataExportDatabaseName and allows custom ports, username/password etc")]
        public string DataExportConnectionString { get; set; }
        
        [Option(Required = true, HelpText = @"Command to run on the engine: 'run' or 'check' ")]
        public CommandLineActivity Command { get; set; }

        [Option(Required = false, Default = false, HelpText = "Process returns errorcode '1' (instead of 0) if there are warnings")]
        public bool FailOnWarnings { get; set; }
        
        protected const string ExampleCatalogueConnectionString = "Server=myServer;Database=RDMP_Catalogue;User Id=myUsername;Password=myPassword;";
        protected const string ExampleDataExportConnectionString = "Server=myServer;Database=RDMP_DataExport;User Id=myUsername;Password=myPassword;";

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
                SqlConnectionStringBuilder c;

                if (CatalogueConnectionString != null)
                    c = new SqlConnectionStringBuilder(CatalogueConnectionString);
                else
                {
                    c = new SqlConnectionStringBuilder();
                    c.DataSource = ServerName;
                    c.IntegratedSecurity = true;
                    c.InitialCatalog = CatalogueDatabaseName;
                }

                SqlConnectionStringBuilder d = null;
                if(DataExportConnectionString != null)
                    d = new SqlConnectionStringBuilder(DataExportConnectionString);
                else
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
