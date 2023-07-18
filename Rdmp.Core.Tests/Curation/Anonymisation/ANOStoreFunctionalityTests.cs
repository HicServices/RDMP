// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.Curation.Anonymisation;

public class ANOStoreFunctionalityTests:TestsRequiringFullAnonymisationSuite
{
    [Test]
    public void CanAccessANODatabase_Directly()
    {
        var server = ANOStore_Database.Server;
        using var con = server.GetConnection();
        con.Open();

        var cmd = server.GetCommand("Select version from RoundhousE.Version", con);
        var version = new Version(cmd.ExecuteScalar().ToString());

        Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

        con.Close();
    }

    [Test]
    public void CanAccessANODatabase_ViaExternalServerPointer()
    {
        using var connection = DataAccessPortal.ExpectServer(ANOStore_ExternalDatabaseServer, DataAccessContext.DataLoad).GetConnection();
        connection.Open();

        using (var cmd =
               DatabaseCommandHelper.GetCommand("Select version from RoundhousE.Version", connection))
        {
            var version = new Version(cmd.ExecuteScalar().ToString());
            Assert.GreaterOrEqual(version, new Version("0.0.0.0"));
        }
                
        connection.Close();
    }

    [Test]
    public void CanAccessIdentifierDumpDatabase_Directly()
    {
        using var con = IdentifierDump_Database.Server.GetConnection();
        con.Open();

        var cmd = IdentifierDump_Database.Server.GetCommand("Select version from RoundhousE.Version", con);
        var version = new Version(cmd.ExecuteScalar().ToString());

        Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

        con.Close();
    }

    [Test]
    public void CanAccessIdentifierDumpDatabase_ViaExternalServerPointer()
    {
        using var connection = DataAccessPortal.ExpectServer(IdentifierDump_ExternalDatabaseServer, DataAccessContext.DataLoad).GetConnection();
        connection.Open();

        using (var cmd = DatabaseCommandHelper.GetCommand("Select version from RoundhousE.Version", connection))
        {
            var version = new Version(cmd.ExecuteScalar().ToString());
            Assert.GreaterOrEqual(version, new Version("0.0.0.0"));
        }
                

        connection.Close();
    }
}