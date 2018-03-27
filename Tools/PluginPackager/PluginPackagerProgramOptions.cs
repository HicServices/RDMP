using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace PluginPackager
{
    /// <summary>
    /// Command line arguments for PluginPackager.exe
    /// </summary>
    public class PluginPackagerProgramOptions
    {
        [ValueList(typeof(List<string>), MaximumElements = 2)]
        public IList<string> Items { get; set; }

        [Option('n', "No Source Code", DefaultValue = false, HelpText = "Skip the source code embedding section of packaging the plugin")]
        public bool SkipSourceCodeCollection { get; set; }

        [Option('s', "Catalogue Server", DefaultValue = null, HelpText = "Sets the catalogue server to upload the zip file to (only used if Database is also specified).")]
        public string Server { get; set; }
        
        [Option('d', "Catalogue Database", DefaultValue = null, HelpText = "Sets the catalogue database to upload the zip file to (only used if Server is also specified).")]
        public string Database { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var advice =
            @"USAGE: PluginPackager.exe SolutionFile ZipOutputFile [optional Flags]
EXAMPLE 1: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip
EXAMPLE 2: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip -n
EXAMPLE 3: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip -n -s localhost\sqlexpress -d RDMP_Catalogue";

            var arguments = HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

            return advice + Environment.NewLine + arguments;
        }
    }
}