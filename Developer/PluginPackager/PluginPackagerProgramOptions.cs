using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace PluginPackager
{
    public class PluginPackagerProgramOptions
    {
        [ValueList(typeof(List<string>), MaximumElements = 2)]
        public IList<string> Items { get; set; }

        [Option('n', "No Source Code", DefaultValue = false, HelpText = "Skip the source code embedding section of packaging the plugin")]
        public bool SkipSourceCodeCollection { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var advice =
            @"USAGE: PluginPackager.exe SolutionFile ZipOutputFile [optional Flags]
EXAMPLE 1: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip
EXAMPLE 2: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip -n";

            var arguments = HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

            return advice + Environment.NewLine + arguments;
        }
    }
}