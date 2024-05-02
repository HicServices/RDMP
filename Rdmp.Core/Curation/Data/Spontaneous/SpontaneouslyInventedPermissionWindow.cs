// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     Spontaneous (non database persisted) version of PermissionWindow.  Use this class when you want to define a runtime
///     only (in memory) window of execution for
///     caching / loading etc.  SpontaneouslyInventedPermissionWindow are never locked.
/// </summary>
public class SpontaneouslyInventedPermissionWindow : SpontaneousObject, IPermissionWindow
{
    private readonly ICacheProgress _cp;

    public SpontaneouslyInventedPermissionWindow(ICacheProgress cp) : this()
    {
        _cp = cp;
        PermissionWindowPeriods = new List<PermissionWindowPeriod>();
    }

    public SpontaneouslyInventedPermissionWindow(ICacheProgress cp, List<PermissionWindowPeriod> windows) : this()
    {
        _cp = cp;
        PermissionWindowPeriods = windows;
    }

    private SpontaneouslyInventedPermissionWindow() : base(new MemoryRepository())
    {
        RequiresSynchronousAccess = true;
        Name = "Spontaneous Permission Window";
    }


    public static void RefreshLockPropertiesFromDatabase()
    {
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool RequiresSynchronousAccess { get; set; }

    public List<PermissionWindowPeriod> PermissionWindowPeriods { get; private set; }

    public bool WithinPermissionWindow()
    {
        //if no periods then it's in the window
        return PermissionWindowPeriods?.Any() != true ||
               PermissionWindowPeriods.Any(w => w.Contains(DateTime.UtcNow, true));
    }

    public IEnumerable<ICacheProgress> CacheProgresses
    {
        get { return new[] { _cp }; }
    }

    public void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods)
    {
        PermissionWindowPeriods = windowPeriods;
    }
}