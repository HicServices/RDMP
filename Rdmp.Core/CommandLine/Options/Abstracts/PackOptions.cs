// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RDMPStartup.Options
{
    /// <summary>
    /// Command line arguments for PluginPackager.exe
    /// </summary>
    [Verb("pack",HelpText = "Packs a given directory into a new RDMP plugin")]
    public class PackOptions
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
                    new Example("Normal Scenario", new PackOptions { SolutionFile = @"c:\MyPlugins\MyPlugin\CoolPlugin.sln", ZipFileName = "CoolPlugin.zip" });

                yield return
                    new Example("No source code, Release mode", new PackOptions { SolutionFile = @"c:\MyPlugins\MyPlugin\CoolPlugin.sln", ZipFileName = "CoolPlugin.zip" , SkipSourceCodeCollection =  true});
                yield return
                    new Example("Commit zip to RDMP database", new PackOptions
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