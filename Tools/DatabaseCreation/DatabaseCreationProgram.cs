// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using RDMPStartup.DatabaseCreation;
using ReusableLibraryCode;

namespace DatabaseCreation
{
    /// <summary>
    /// Creates the minimum set of databases required to get RDMP working (Catalogue and Data Export databases) with an optional prefix.  Also creates satellite
    /// Tier 2 databases (logging / dqe)
    /// </summary>
    public class DatabaseCreationProgram
    {
        public static int Main(string[] args)
        {
            AssemblyResolver.SetupAssemblyResolver();
            return UsefulStuff.GetParser().ParseArguments<PlatformDatabaseCreationOptions>(args).MapResult(RunOptionsAndReturnExitCode, errs => 1);
        }

        public static int RunOptionsAndReturnExitCode(PlatformDatabaseCreationOptions options)
        {
            var serverName = options.ServerName;
            var prefix = options.Prefix;

            Console.WriteLine("About to create on server '" + serverName + "' databases with prefix '" + prefix + "'");
            
            ImplementationManager.Load(typeof(MicrosoftSQLImplementation).Assembly);

            try
            {
                var creator = new PlatformDatabaseCreation();
                creator.CreatePlatformDatabases(options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
            return 0;
        }


        
    }
}
