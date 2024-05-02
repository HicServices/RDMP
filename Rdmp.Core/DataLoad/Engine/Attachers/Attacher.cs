// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Attachers;

/// <summary>
///     A Class which will run during Data Load Engine execution and result in the creation or population of a RAW
///     database, the database may or not require
///     to already exist (e.g. MDFAttacher would expect it not to exist but AnySeparatorFileAttacher would require the
///     tables/databases already exist).
/// </summary>
public abstract class Attacher : IAttacher
{
    public const string Culture_DemandDescription =
        "Culture to use for bulk insert operations (determines date formats etc)";

    public const string ExplicitDateTimeFormat_DemandDescription =
        "Optional - explicit format for all date columns e.g. yyyy-MM-dd. See https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings";

    private CultureInfo _culture;

    [DemandsInitialization(Culture_DemandDescription)]
    public virtual CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    [DemandsInitialization(ExplicitDateTimeFormat_DemandDescription)]
    public virtual string ExplicitDateTimeFormat { get; set; }

    protected DiscoveredDatabase _dbInfo;

    public abstract ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken);

    public ILoadDirectory LoadDirectory { get; set; }

    public bool RequestsExternalDatabaseCreation { get; }

    public virtual void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
        LoadDirectory = directory;
        _dbInfo = dbInfo;
    }

    protected Attacher(bool requestsExternalDatabaseCreation)
    {
        RequestsExternalDatabaseCreation = requestsExternalDatabaseCreation;
    }

    public abstract void Check(ICheckNotifier notifier);


    public abstract void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener);
}