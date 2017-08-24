using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace DatabaseCreation
{
    public class DatabaseCreationProgramOptions
    {
        [ValueList(typeof(List<string>), MaximumElements = 2)]
        public IList<string> Items { get; set; }

        [Option('b', "Binary Collation", DefaultValue = false, HelpText = "Create the databases with Binary Collation")]
        public bool BinaryCollation { get; set; }

        [Option('d', "Drop Databases First",  DefaultValue = false, HelpText = "Drop the databases before attempting to create them")]
        public bool DropDatabases { get; set; }

        [Option('k', "Skip Pipelines", DefaultValue = false, HelpText = "Skips creating the default Pipelines and Managed Server References in the Catalogue database once created.")]
        public bool SkipPipelines { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var advice = 
            @"USAGE: DatabaseCreation.exe Servername Prefix [optional Flags]
EXAMPLE 1: DatabaseCreation.exe localhost\sqlexpress TEST_ -D
EXAMPLE 2: DatabaseCreation.exe localhost\sqlexpress TEST_
EXAMPLE 3: DatabaseCreation.exe localhost\sqlexpress -D";

            var arguments = HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

            return advice + Environment.NewLine + arguments;
        }
    }
}