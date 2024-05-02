// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Startup.Events;

/// <summary>
///     Event Args for when an <see cref="IPatcher" /> is located during <see cref="Startup" />
///     <para>
///         Includes the evaluated status of the database (does it need patching etc) and the <see cref="Patcher" />
///         which contains the schema script definitions).
///     </para>
///     <para>
///         It is important that all platform Databases exactly match the runtime libraries for managing saving/loading
///         objects therefore if the Status is
///         RequiresPatching it is imperative that you patch the database and restart the application (happens
///         automatically with StartupUI).
///     </para>
/// </summary>
public class PlatformDatabaseFoundEventArgs
{
    public ITableRepository Repository { get; set; }
    public IPatcher Patcher { get; set; }

    public RDMPPlatformDatabaseStatus Status { get; set; }
    public Exception Exception { get; set; }

    public PlatformDatabaseFoundEventArgs(ITableRepository repository, IPatcher patcher,
        RDMPPlatformDatabaseStatus status, Exception exception = null)
    {
        Repository = repository;
        Patcher = patcher;
        Status = status;
        Exception = exception;
    }

    public string SummariseAsString()
    {
        return
            $"RDMPPlatformDatabaseStatus is {Status} for tier {Patcher.Tier} database of type {Patcher.Name} with connection string {(Repository == null ? "Unknown" : Repository.ConnectionString)}{Environment.NewLine}{(Exception == null ? "No exception" : ExceptionHelper.ExceptionToListOfInnerMessages(Exception))}";
    }
}