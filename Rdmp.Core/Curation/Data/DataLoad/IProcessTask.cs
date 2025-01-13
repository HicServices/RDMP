// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <inheritdoc cref="ProcessTask"/>
public interface IProcessTask : IRevertable, IArgumentHost, IOrderable, IDisableable
{
    /// <inheritdoc cref="IArgumentHost.GetAllArguments"/>
    IEnumerable<ProcessTaskArgument> ProcessTaskArguments { get; }

    /// <summary>
    /// Either the C# Type name of a data load component (e.g. an IAttatcher, IDataProvider) or the path to an sql file or exe file (depending on <see cref="ProcessTaskType"/>)
    /// </summary>
    string Path { get; }

    /// <summary>
    /// The human readable description of what the component is supposed to do (e.g. "Copy all csv files from c:/temp/landing into ForLoading")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The stage of the data load (RAW=>STAGING=>LIVE) that the task should be run at.  This can restrict which operations are allowed e.g. you can't run attatchers PostLoad
    /// </summary>
    LoadStage LoadStage { get; }

    /// <inheritdoc cref="DataLoad.ProcessTaskType"/>
    ProcessTaskType ProcessTaskType { get; }

    /// <summary>
    /// Deprecated property
    /// </summary>
    [Obsolete(
        "Since you can't change which Catalogues are loaded by a LoadMetadata at runtime, this property is now obsolete")]
    int? RelatesSolelyToCatalogue_ID { get; }

    /// <summary>
    /// A serialised JSON object that stores arbitrary configuration for the process task
    /// </summary>
#nullable enable
    string? SerialisableConfiguration { get; }


    int LoadMetadataVersion { get; }
}