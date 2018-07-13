using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace DatabaseCreation
{
    /// <summary>
    /// Command line arguments for DatabaseCreation.exe
    /// </summary>
    public class DatabaseCreationProgramOptions
    {
        [Value(0, Required = true, HelpText = "The Microsoft SQL Server on which to create the platform databases (does not have to be the same as your data repository server)")]
        public string ServerName { get; set; }
        
        [Value(1, Required = true, HelpText = "The prefix to append to all databases created")]
        public string Prefix { get; set; }

        [Option('b', "Binary Collation", Default = false, HelpText = "Create the databases with Binary Collation")]
        public bool BinaryCollation { get; set; }

        [Option('d', "Drop Databases First",  Default = false, HelpText = "Drop the databases before attempting to create them")]
        public bool DropDatabases { get; set; }

        [Option('k', "Skip Pipelines", Default = false, HelpText = "Skips creating the default Pipelines and Managed Server References in the Catalogue database once created.")]
        public bool SkipPipelines { get; set; }
        
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal Scenario", new DatabaseCreationProgramOptions { ServerName = @"localhost\sqlexpress", Prefix = "TEST_"});
                yield return new Example("Drop existing", new DatabaseCreationProgramOptions { ServerName = @"localhost\sqlexpress", Prefix = "TEST_", DropDatabases = true });
                yield return new Example("Binary Collation", new DatabaseCreationProgramOptions { ServerName = @"localhost\sqlexpress",Prefix =  "TEST_" , DropDatabases = true,BinaryCollation =true });
            }
        }
    }
}