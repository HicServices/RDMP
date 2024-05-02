// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data.Dashboarding;

/// <summary>
///     Describes a named collection of windows helpful for achieving a given task (usually data summarisation).  This
///     class is the root object and has name (e.g. Dave's Dashboard).  It then
///     has a collection of DashboardControls which are IDashboardableControl instances that the user has configured on his
///     Dashboard via DashboardLayoutUI.  This can include plugins. Not only
///     does this class provide persistence for useful layouts of controls between application executions but it allows
///     users to share their dashboards with one another.
/// </summary>
public class DashboardLayout : DatabaseEntity, INamed
{
    #region Database Properties

    private string _name;
    private DateTime _created;
    private string _username;

    /// <inheritdoc />
    [Unique]
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     The time the new dashboard was created
    /// </summary>
    public DateTime Created
    {
        get => _created;
        set => SetField(ref _created, value);
    }

    /// <summary>
    ///     The user who created the dashboard
    /// </summary>
    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    ///     Returns all controls that should be rendered on the given dashboard
    /// </summary>
    [NoMappingToDatabase]
    public DashboardControl[] Controls => Repository.GetAllObjectsWithParent<DashboardControl>(this);

    #endregion

    public DashboardLayout()
    {
    }

    internal DashboardLayout(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"].ToString();
        Created = Convert.ToDateTime(r["Created"]);
        Username = r["Username"].ToString();
    }

    /// <summary>
    ///     Creates a new empty dashboard with the given name ready for controls to be added by the user
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public DashboardLayout(ICatalogueRepository repository, string name)
    {
        Repository = repository;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Username", Environment.UserName },
            { "Name", name }
        });
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}