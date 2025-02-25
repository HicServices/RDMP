// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Microsoft.Data.SqlClient;

namespace Rdmp.Core.CommandLine.DatabaseCreation;

/// <summary>
/// Command line arguments for install verb of rdmp CLI
/// </summary>
[Verb("install", HelpText = "Creates RMDP platform databases in the target database server")]
public class PlatformDatabaseCreationOptions
{
    [Value(0, Required = true,
        HelpText =
            "The Microsoft SQL Server on which to create the platform databases (does not have to be the same as your data repository server)")]
    public string ServerName { get; set; }

    [Value(1, Required = true, HelpText = "The prefix to append to all databases created")]
    public string Prefix { get; set; }

    [Option('u', HelpText = "Username for sql authentication (Optional)")]
    public string Username { get; set; }

    [Option('p', HelpText = "Password for sql authentication (Optional)")]
    public string Password { get; set; }

    [Option('b', "Binary Collation", Default = false, HelpText = "Create the databases with Binary Collation")]
    public bool BinaryCollation { get; set; }

    [Option('d', "Drop Databases First", Default = false,
        HelpText = "Drop the databases before attempting to create them")]
    public bool DropDatabases { get; set; }

    [Option('k', "Skip Pipelines", Default = false,
        HelpText =
            "Skips creating the default Pipelines and Managed Server References in the Catalogue database once created.")]
    public bool SkipPipelines { get; set; }

    [Option('l', "Create Logging Server", Default = true, HelpText = "Create the default logging server in the Catalogue database once created. Is superseded by 'Skip Pipelines'")]
    public bool CreateLoggingServer { get; set; }

    [Option('e', "ExampleDatasets", Default = false,
        HelpText = "Create example datasets, projects, extraction configurations and cohort queries")]
    public bool ExampleDatasets { get; set; }

    [Option('s', "Seed", Default = 500,
        HelpText = "When ExampleDatasets is set this is the seed that is used for generating the data")]
    public int Seed { get; set; } = 500;

    [Option("NumberOfPeople", Default = ExampleDatasetsCreation.NumberOfPeople,
        HelpText =
            "When ExampleDatasets is set this is the number of unique patients to generate before building dataset rows (patient pool)")]
    public int NumberOfPeople { get; set; } = ExampleDatasetsCreation.NumberOfPeople;

    [Option("NumberOfRowsPerDataset", Default = ExampleDatasetsCreation.NumberOfRowsPerDataset,
        HelpText = "When ExampleDatasets is set this is the number of rows to create in each dataset")]
    public int NumberOfRowsPerDataset { get; set; } = ExampleDatasetsCreation.NumberOfRowsPerDataset;

    [Option("Nightmare", Default = false, HelpText = "Create 100,000+ objects in the Catalogue database")]
    public bool Nightmare { get; set; }

    [Option("NightmareFactor", Default = 1,
        HelpText = "Set to 2 (or more) to multiply the volume of Nightmare data generated")]
    public int NightmareFactor { get; set; } = 1;

    [Option(Required = false, Default = false,
        HelpText = "Set to true to validate the SSL certificate of the server you are installing into")]
    public bool ValidateCertificate { get; set; }

    [Option(Required = false, Default = 30,
        HelpText = "Timeout in seconds for CREATE DATABASE SQL commands.  Defaults to 30")]
    public int CreateDatabaseTimeout { get; set; } = 30;

    [Option(Required = false,
        HelpText =
            "Optional connection string keywords to use e.g. \"Key1=Value1; Key2=Value2\".  When using this option you must manually specify IntegratedSecurity if required.")]
    public string OtherKeywords { get; set; }


    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Normal Scenario",
                new PlatformDatabaseCreationOptions { ServerName = @"localhost\sqlexpress", Prefix = "TEST_" });
            yield return new Example("Drop existing",
                new PlatformDatabaseCreationOptions
                { ServerName = @"localhost\sqlexpress", Prefix = "TEST_", DropDatabases = true });
            yield return new Example("Create example datasets",
                new PlatformDatabaseCreationOptions
                {
                    ServerName = @"localhost\sqlexpress",
                    Prefix = "TEST_",
                    DropDatabases = true,
                    ExampleDatasets = true
                });
            yield return new Example("Binary Collation",
                new PlatformDatabaseCreationOptions
                {
                    ServerName = @"localhost\sqlexpress",
                    Prefix = "TEST_",
                    DropDatabases = true,
                    BinaryCollation = true
                });
            yield return new Example("Drop existing",
                new PlatformDatabaseCreationOptions
                {
                    ServerName = @"localhost\sqlexpress",
                    Prefix = "TEST_",
                    Username = "sa",
                    Password = "lawl",
                    DropDatabases = true
                });
        }
    }

    public SqlConnectionStringBuilder GetBuilder(string databaseName)
    {
        var builder = string.IsNullOrWhiteSpace(OtherKeywords)
            ? new SqlConnectionStringBuilder()
            : new SqlConnectionStringBuilder(OtherKeywords);

        builder.DataSource = ServerName;
        builder.InitialCatalog = (Prefix ?? "") + databaseName;
        builder.TrustServerCertificate = !ValidateCertificate;

        if (!string.IsNullOrWhiteSpace(Username))
        {
            builder.UserID = Username;
            builder.Password = Password;
        }
        else
        {
            // if they are specifying other keywords they might be auth related so we don't want to blindly turn this on
            builder.IntegratedSecurity = string.IsNullOrWhiteSpace(OtherKeywords);
        }

        return builder;
    }
}