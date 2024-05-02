// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Dashboarding;

/// <summary>
///     Describes a specific object used by a DashboardControl.  For example if you create a pie chart of issues on a
///     specific catalogue on your DashboardLayout then there will be a
///     DashboardControl for the pie chart and a DashboardObjectUse pointing at that specific Catalogue.  These refernces
///     do not stop objects being deleted.  References can also be
///     cross database (e.g. pointing at objects in a DataExport database like Project etc).
/// </summary>
public class DashboardObjectUse : ReferenceOtherObjectDatabaseEntity
{
    #region Database Properties

    private int _dashboardControlID;

    /// <summary>
    ///     The <see cref="DashboardControl" /> for which the class records object usage for
    /// </summary>
    public int DashboardControl_ID
    {
        get => _dashboardControlID;
        set => SetField(ref _dashboardControlID, value);
    }

    #endregion

    public DashboardObjectUse()
    {
    }

    internal DashboardObjectUse(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        DashboardControl_ID = Convert.ToInt32(r["DashboardControl_ID"]);
    }

    /// <summary>
    ///     Records the fact that the given <see cref="DashboardControl" /> targets the given object (and hopefully displays
    ///     information about it)
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parentControl"></param>
    /// <param name="objectToSave"></param>
    public DashboardObjectUse(ICatalogueRepository repository, DashboardControl parentControl,
        IMapsDirectlyToDatabaseTable objectToSave)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", objectToSave.ID },
            { "ReferencedObjectType", objectToSave.GetType().Name },
            { "ReferencedObjectRepositoryType", objectToSave.Repository.GetType().Name },
            { "DashboardControl_ID", parentControl.ID }
        });
    }
}