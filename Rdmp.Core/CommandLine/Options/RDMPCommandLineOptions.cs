// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Data.SqlClient;
using CommandLine;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.Options
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
                
        [Option(Required =false, HelpText = @"Log StartUp output")]
        public bool LogStartup{get;set;}
        
        [Option(Required = false, HelpText = @"Command to run on the engine: 'run' or 'check' ", Default = CommandLineActivity.run)]
        public CommandLineActivity Command { get; set; } = CommandLineActivity.run;

        [Option(Required = false, Default = false, HelpText = "Process returns errorcode '1' (instead of 0) if there are warnings")]
        public bool FailOnWarnings { get; set; }
        
        protected const string ExampleCatalogueConnectionString = "Server=myServer;Database=RDMP_Catalogue;User Id=myUsername;Password=myPassword;";
        protected const string ExampleDataExportConnectionString = "Server=myServer;Database=RDMP_DataExport;User Id=myUsername;Password=myPassword;";

        public IRDMPPlatformRepositoryServiceLocator DoStartup(EnvironmentInfo env,ICheckNotifier checkNotifier)
        {
            Startup.Startup startup = new Startup.Startup(env,GetRepositoryLocator());
            startup.DoStartup(checkNotifier);
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

        /// <summary>
        /// Returns true if none of the settings relating to connection strings have been set.  This is used to determine whether system defaults should
        /// be used (e.g. from Databases.yaml).
        /// </summary>
        /// <returns></returns>
        public bool NoConnectionStringsSpecified()
        {
            return string.IsNullOrWhiteSpace(ServerName) &&
            string.IsNullOrWhiteSpace(CatalogueDatabaseName) &&
            string.IsNullOrWhiteSpace(DataExportDatabaseName) &&
            string.IsNullOrWhiteSpace(CatalogueConnectionString) &&
            string.IsNullOrWhiteSpace(DataExportConnectionString);
        }


    }
}
