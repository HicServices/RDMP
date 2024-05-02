// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     The high level type of a ProcessTask, defines what the property Path contains.  If the ProcessTaskType is
///     Executable then Path contains the path to an exe to run, If
///     ProcessTaskType is Attacher then Path will be a class name etc.
/// </summary>
public enum ProcessTaskType
{
    /// <summary>
    ///     ProcessTask is to launch an executable file with parameters telling it about the load stage being operated on
    ///     (servername, database name etc)
    /// </summary>
    Executable,

    /// <summary>
    ///     ProcessTask is to run an SQL file directly on the server
    /// </summary>
    SQLFile,

    /// <summary>
    ///     ProcessTask is to instantiate the IAttacher class Type specified in Path and hydrate its [DemandsInitialization]
    ///     properties with values matching
    ///     ProcessTaskArguments and run it in the specified load stage in an AttacherRuntimeTask wrapper.
    /// </summary>
    Attacher,

    /// <summary>
    ///     ProcessTask is to instantiate the IDataProvider class Type specified in Path and hydrate its
    ///     [DemandsInitialization] properties with values matching
    ///     ProcessTaskArguments and run it in the specified load stage in an DataProviderRuntimeTask wrapper.
    /// </summary>
    DataProvider,

    /// <summary>
    ///     ProcessTask is to instantiate the IMutilateDataTables class Type specified in Path and hydrate its
    ///     [DemandsInitialization] properties with values matching
    ///     ProcessTaskArguments and run it in the specified load stage in an MutilateDataTablesRuntimeTask wrapper.
    /// </summary>
    MutilateDataTable,

    /// <summary>
    ///     ProcessTask is to import a SQL backup file directly to the server
    /// </summary>
    SQLBakFile
}