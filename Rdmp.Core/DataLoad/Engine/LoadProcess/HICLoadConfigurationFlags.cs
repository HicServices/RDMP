// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Engine.LoadProcess;

/// <summary>
///     Options for controlling which sections of a Data Load are skipped/executed (e.g. for user debugging purposes you
///     might want to stop a load after
///     populating RAW if you think there is a problem with the load configuration).
/// </summary>
public class HICLoadConfigurationFlags
{
    public bool ArchiveData { get; set; }
    public bool DoLoadToStaging { get; set; }
    public bool DoMigrateFromStagingToLive { get; set; }

    public HICLoadConfigurationFlags()
    {
        ArchiveData = true;
        DoLoadToStaging = true;
        DoMigrateFromStagingToLive = true;
    }
}