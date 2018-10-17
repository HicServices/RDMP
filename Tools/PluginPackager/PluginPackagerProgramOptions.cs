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
        [Value(0,Required =true,HelpText = "Path to the Visual Studio .sln file which contains your Plugin")]
        public string SolutionFile { get; set; }

        [Value(1, Required = true, HelpText = "The filename to give your plugin once it has been gathered and packaged, must end with .zip")]
        public string ZipFileName { get; set; }

        [Option('n', "No Source Code", Default = false, HelpText = "Skip the source code embedding section of packaging the plugin")]
        public bool SkipSourceCodeCollection { get; set; }

        [Option('s', "Catalogue Server",  HelpText = "Sets the catalogue server to upload the zip file to (only used if Database is also specified).")]
        public string Server { get; set; }
        
        [Option('d', "Catalogue Database", HelpText = "Sets the catalogue database to upload the zip file to (only used if Server is also specified).")]
        public string Database { get; set; }

        [Option('r', "Release", HelpText = "Package the 'Release' folder instead of 'Debug'")]
        public bool Release { get; set; }


        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return 
                    new Example("Normal Scenario", new PluginPackagerProgramOptions { SolutionFile = @"c:\MyPlugins\MyPlugin\CoolPlugin.sln", ZipFileName = "CoolPlugin.zip" });

                yield return
                    new Example("No source code, Release mode", new PluginPackagerProgramOptions { SolutionFile = @"c:\MyPlugins\MyPlugin\CoolPlugin.sln", ZipFileName = "CoolPlugin.zip" , SkipSourceCodeCollection =  true});
                yield return
                    new Example("Commit zip to RDMP database", new PluginPackagerProgramOptions
                    {
                        SolutionFile = @"c:\MyPlugins\MyPlugin\CoolPlugin.sln", 
                        ZipFileName = "CoolPlugin.zip", 
                        SkipSourceCodeCollection = true,
                        Server = @"localhost\sqlexpress",
                        Database = "RDMP_Catalogue",
                        Release = true
                    });
                    

            }
        }

        /*
        [HelpOption]
        public string GetUsage()
        {
            var advice =
            @"USAGE: PluginPackager.exe SolutionFile ZipOutputFile [optional Flags]
EXAMPLE 1: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip
EXAMPLE 2: 
EXAMPLE 3: PluginPackager.exe c:\MyPlugins\MyPlugin\CoolPlugin.sln CoolPlugin.zip -n -s localhost\sqlexpress -d RDMP_Catalogue";

            var arguments = HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

            return advice + Environment.NewLine + arguments;
        }*/
    }
}