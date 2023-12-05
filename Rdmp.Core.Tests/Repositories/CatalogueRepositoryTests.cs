// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using FAnsi.Implementation;
using NUnit.Framework;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;

namespace Rdmp.Core.Tests.Repositories;

internal class CatalogueRepositoryTests
{
    /// <summary>
    /// Tests that when a <see cref="CatalogueRepository"/> fails connection testing that the password is not exposed but that
    /// the rest of the connection string is exposed (including any optional settings like connection timeout)
    /// </summary>
    [Test]
    public void TestConnection_NoServer_DoNotShowPassword()
    {
        ImplementationManager.Load<FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation>();

        var repo = new CatalogueRepository(new SqlConnectionStringBuilder
        {
            DataSource = "NonExistant11",
            UserID = "fish",
            Password = "omg",
            ConnectTimeout = 2
        });

        var msg = Assert.Throws<Exception>(() => repo.TestConnection());

        Assert.That(msg.Message, Does.StartWith("Testing connection failed"));
        Assert.That(msg.Message, Does.Not.Contain("omg"));
        Assert.That(msg.Message, Does.Contain("****"));
        Assert.That(msg.Message, Does.Contain("Timeout"));
        Assert.That(msg.Message, Does.Contain("2"));
    }

    [Test]
    public void TestConnection_NoServer_IntegratedSecurity()
    {
        ImplementationManager.Load<FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation>();

        if (EnvironmentInfo.IsLinux)
            Assert.Inconclusive(
                "Linux doesn't really support IntegratedSecurity and in fact can bomb just setting it on a builder");

        var repo = new CatalogueRepository(new SqlConnectionStringBuilder
        {
            DataSource = "NonExistant11",
            IntegratedSecurity = true,
            ConnectTimeout = 2
        });

        var msg = Assert.Throws<Exception>(() => repo.TestConnection());

        Assert.That(msg.Message, Does.StartWith("Testing connection failed"));
        Assert.That(msg.Message, Does.Contain("Integrated Security="));
        Assert.That(msg.Message, Does.Contain("Timeout"));
        Assert.That(msg.Message, Does.Contain("2"));
    }
}