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
using NLog;
using System.Reflection;
using System.IO;
using System;
using YamlDotNet.RepresentationModel;   

namespace Rdmp.Core.CommandLine.Options
{
    /// <summary>
    /// Abstract base class for all command line options that can be supplied to the rdmp cli (includes overriding app.config to get connection strings to platform metadata databases)
    /// </summary>
    public abstract class RDMPCommandLineOptions
    {
        private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

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

        [Option(Required = false, HelpText = "Path to the yaml file containing database connection strings.  Defaults to 'Databases.yaml'.  Explicit command line arguments (e.g. --CatalogueConnectionString) override this", Default = "Databases.yaml")]
        public string ConnectionStringsFile { get; set; }

        [Option(Required =false, HelpText = @"Log StartUp output")]
        public bool LogStartup{get;set;}
        
        [Option(Required = false, HelpText = @"Command to run on the engine: 'run' or 'check' ", Default = CommandLineActivity.run)]
        public CommandLineActivity Command { get; set; } = CommandLineActivity.run;

        [Option(Required = false, Default = false, HelpText = "Process returns errorcode '1' (instead of 0) if there are warnings")]
        public bool FailOnWarnings { get; set; }

        [Option(Required = false, HelpText = "Connect to an RDMP platform 'database' stored on the file system at this folder")]
        public string Dir { get; set; }

        /// <summary>
        /// If <see cref="ConnectionStringsFile"/> was specified and that file existed and was succesfully loaded
        /// using <see cref="PopulateConnectionStringsFromYamlIfMissing()"/> then this property will store the
        /// file used including name and description
        /// </summary>
        public ConnectionStringsYamlFile ConnectionStringsFileLoaded { get; private set; }

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
                if (!string.IsNullOrWhiteSpace(Dir))
                {
                    return _repositoryLocator = new RepositoryProvider(new YamlRepository(new DirectoryInfo(Dir)));
                }
                else
                {
                    GetConnectionStrings(out var c, out var d);
                    _repositoryLocator = new LinkedRepositoryProvider(c?.ConnectionString, d?.ConnectionString);
                }
            }

            return _repositoryLocator;
        }

        protected virtual bool ShouldLoadHelp()
        {
            return false;
        }

        /// <summary>
        /// Gets the connection strings that have been defined on the command line or by providing a Databases.yaml file
        /// </summary>
        /// <param name="c">Catalogue database connection string or null if no explicit value has been defined.  E.g. if consumers are expected to get this elsewhere like user settings</param>
        /// <param name="d">Data export database connection string or null if no explicit value has been defined.</param>
        public void GetConnectionStrings(out SqlConnectionStringBuilder c, out SqlConnectionStringBuilder d)
        {
            CatalogueRepository.SuppressHelpLoading = !ShouldLoadHelp();

            if (CatalogueConnectionString != null)
                try
                {
                    c = new SqlConnectionStringBuilder(CatalogueConnectionString);
                }
                catch (Exception ex)
                {
                    throw new Exception("CatalogueConnectionString is invalid",ex);
                }
            else
            if (CatalogueDatabaseName != null)
            {
                c = new SqlConnectionStringBuilder();
                c.DataSource = ServerName;
                c.IntegratedSecurity = true;
                c.InitialCatalog = CatalogueDatabaseName;
            }
            else
                c = null;

            if (DataExportConnectionString != null)
                try
                {
                    d = new SqlConnectionStringBuilder(DataExportConnectionString);
                }
                catch (Exception ex)
                {
                    throw new Exception("DataExportConnectionString is invalid", ex);
                }
            else
            if (DataExportDatabaseName != null)
            {
                d = new SqlConnectionStringBuilder();
                d.DataSource = ServerName;
                d.IntegratedSecurity = true;
                d.InitialCatalog = DataExportDatabaseName;
            }
            else
                d = null;
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

        public void PopulateConnectionStringsFromYamlIfMissing()
        {
            var logger = LogManager.GetCurrentClassLogger();

            if (!NoConnectionStringsSpecified())
            {
                logger.Info("Connection string options have been specified on command line, yaml config values will be ignored");
                return;
            }

            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var yaml = Path.Combine(assemblyFolder, ConnectionStringsFile);

            if (File.Exists(yaml))
            {
                try
                {
                    var cstrs = ConnectionStringsYamlFile.LoadFrom(new FileInfo(yaml));
                    CatalogueConnectionString = cstrs.CatalogueConnectionString;
                    DataExportConnectionString = cstrs.DataExportConnectionString;

                    ConnectionStringsFileLoaded = cstrs;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to read yaml file '" + yaml + "'");
                }
            }
        }
    }
}
