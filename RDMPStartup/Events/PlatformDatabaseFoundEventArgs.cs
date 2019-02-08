// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace RDMPStartup.Events
{
    /// <summary>
    /// Event Args for when the .Database assembly (e.g. CatalogueLibrary.Database) managed database is located during Startup.cs
    /// 
    /// <para>Includes the evaluated status of the database (does it need patching etc) and the Assemblies responsible for managing the database
    /// (The DatabaseAssembly and the HostAssembly - which contains the object definitions).</para>
    /// 
    /// <para>It is important that all platform Databases exactly match the runtime libraries for managing saving/loading objects therefore if the Status is 
    /// RequiresPatching it is imperative that you patch the database and restart the application (happens automatically with StartupUI).</para>
    /// </summary>
    public class PlatformDatabaseFoundEventArgs
    {
        public ITableRepository Repository { get; set; }
        public Assembly HostAssembly { get; set; }
        public Assembly DatabaseAssembly { get; set; }

        public int Tier { get; set; }
        public RDMPPlatformType DatabaseType { get; set; }

        public RDMPPlatformDatabaseStatus Status { get; set; }
        public Exception Exception { get; set; }

        public PlatformDatabaseFoundEventArgs(ITableRepository repository, Assembly hostAssembly, Assembly databaseAssembly, int tier, RDMPPlatformDatabaseStatus status, RDMPPlatformType databaseType,Exception exception=null)
        {
            Repository = repository;
            HostAssembly = hostAssembly;
            DatabaseAssembly = databaseAssembly;
            Tier = tier;
            Status = status;
            Exception = exception;
            DatabaseType = databaseType;
        }

        public string SummariseAsString()
        {
            return "RDMPPlatformDatabaseStatus is " + Status + " for tier " + Tier + " database of type " +
                   DatabaseType + " with connection string " +
                   (Repository == null ? "Unknown" : Repository.ConnectionString) + Environment.NewLine +
                   (Exception == null
                       ? "No exception"
                       : ExceptionHelper.ExceptionToListOfInnerMessages(Exception));
        }
    }
}
