// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using FAnsi;
using FAnsi.Discovery.ConnectionStringDefaults;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes a specific key/value pair that should always be used (unless overriden by an API requirement) in
///     connection strings to servers of the given <see cref="DatabaseType" />
///     by RDMP.  For example you could specify Encrypt = true to force all connections made to go through SSL (requires
///     certificates / certificate validation etc).  Be careful when creating
///     these as they apply to all users of the system and can make servers unreachable if a syntactically valid but
///     unresolvable connection string is created.
///     <para>
///         Checks will ensure that the keyword is a valid connection string keyword for the given
///         <see cref="DatabaseType" /> and thus you will not get syntactically illegal connection strings
///     </para>
/// </summary>
public class ConnectionStringKeyword : DatabaseEntity, INamed, ICheckable
{
    #region Database Properties

    private DatabaseType _databaseType;
    private string _name;
    private string _value;

    #endregion

    /// <summary>
    ///     The DBMS (Oracle / MySql etc) which this keyword should be used when connecting to
    /// </summary>
    public DatabaseType DatabaseType
    {
        get => _databaseType;
        set => SetField(ref _databaseType, value);
    }

    /// <summary>
    ///     The name of the keyword.  Must be a valid connection string key for the <see cref="DatabaseType" /> e.g.
    ///     IntegratedSecurity
    /// </summary>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     The value to write into the connection string for the keyword e.g.  sspi
    /// </summary>
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public ConnectionStringKeyword()
    {
    }

    /// <summary>
    ///     Defines a new keyword that should be set on all connections to databases of <see cref="DatabaseType" /> when making
    ///     new connections
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="databaseType"></param>
    /// <param name="keyword"></param>
    /// <param name="value"></param>
    public ConnectionStringKeyword(ICatalogueRepository repository, DatabaseType databaseType, string keyword,
        string value)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "DatabaseType", databaseType.ToString() },
            { "Name", keyword },
            { "Value", value }
        });

        if (ID == 0 || Repository != repository)
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    internal ConnectionStringKeyword(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        DatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), r["DatabaseType"].ToString());
        Name = r["Name"].ToString();
        Value = r["Value"] as string;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Checks that the keyword is valid syntax for the <see cref="DatabaseType" /> and can be set on a
    ///     <see cref="DbConnectionStringBuilder" />
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        try
        {
            var accumulator = new ConnectionStringKeywordAccumulator(DatabaseType);
            accumulator.AddOrUpdateKeyword(Name, Value, ConnectionStringKeywordPriority.SystemDefaultLow);
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Integrity of keyword is ok according to ConnectionStringKeywordAccumulator", CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));
        }
    }
}