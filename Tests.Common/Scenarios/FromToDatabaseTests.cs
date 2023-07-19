// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;

namespace Tests.Common.Scenarios;

/// <summary>
/// Tests which require two test databases and handles moving data from one to the other
/// </summary>
public class FromToDatabaseTests:DatabaseTests
{
    private readonly string _suffix;
    protected DiscoveredDatabase From;
    protected DiscoveredDatabase To;

    /// <summary>
    /// The runtime name of <see cref="To"/>.  This is the same as calling <see cref="DiscoveredDatabase.GetRuntimeName()"/>
    /// </summary>
    protected string DatabaseName => To.GetRuntimeName();

    public FromToDatabaseTests(string suffix = "_STAGING")
    {
        _suffix = suffix;
    }

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        SetupFromTo(DatabaseType.MicrosoftSQLServer);
    }

    /// <summary>
    /// Sets up the databases <see cref="From"/> and <see cref="To"/> on the test database server of the given
    /// <paramref name="dbType"/>.  This method is automatically called with <see cref="DatabaseType.MicrosoftSQLServer"/>
    /// in <see cref="OneTimeSetUp()"/> (nunit automatically fires it).
    /// </summary>
    /// <param name="dbType"></param>
    protected void SetupFromTo(DatabaseType dbType)
    {
        To = GetCleanedServer(dbType);
        From = To.Server.ExpectDatabase(To.GetRuntimeName() + _suffix);

        // ensure the test staging and live databases are empty
        if(!From.Exists())
            From.Create();
        else
            DeleteTables(From);
    }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
    }
}