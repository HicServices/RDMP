// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Repositories;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using RDMPStartup.PluginManagement;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace PluginPackager
{
    class Program
    {
        static int Main(string[] args)
        {
            return UsefulStuff.GetParser().ParseArguments<PluginPackagerProgramOptions>(args).MapResult(RunOptionsAndReturnExitCode, errs => 1);
        }

        private static int RunOptionsAndReturnExitCode(PluginPackagerProgramOptions opts)
        {
            FileInfo f = new FileInfo(opts.SolutionFile);
            if (!f.Exists)
            {
                Console.WriteLine("Could not find Solution File");
                return -58;
            }

            if (f.Extension != ".sln")
            {
                Console.WriteLine("SolutionFile must be a .sln file");
                return -57;
            }

            Packager p = new Packager(f, opts.ZipFileName, opts.SkipSourceCodeCollection,opts.Release);
            p.PackageUpFile(new ThrowImmediatelyCheckNotifier());

            if (!string.IsNullOrWhiteSpace(opts.Server))
            {
                ImplementationManager.Load(typeof(MicrosoftSQLImplementation).Assembly);
                var builder = new SqlConnectionStringBuilder() { DataSource = opts.Server, InitialCatalog = opts.Database, IntegratedSecurity = true };

                CatalogueRepository.SuppressHelpLoading = true;
                var processor = new PluginProcessor(new ThrowImmediatelyCheckNotifier(), new CatalogueRepository(builder));
                processor.ProcessFileReturningTrueIfIsUpgrade(new FileInfo(opts.ZipFileName));
            }

            return 0;
        }
    }
}
