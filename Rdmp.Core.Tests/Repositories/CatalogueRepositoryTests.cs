using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using FAnsi.Implementation;
using NUnit.Framework;
using Rdmp.Core.Repositories;
using Rdmp.Core.Startup;

namespace Rdmp.Core.Tests.Repositories
{
    class CatalogueRepositoryTests
    {
        /// <summary>
        /// Tests that when a <see cref="CatalogueRepository"/> fails connection testing that the password is not exposed but that
        /// the rest of the connection string is exposed (including any optional settings like connection timeout)
        /// </summary>
        [Test]
        public void TestConnection_NoServer_DoNotShowPassword()
        {
            ImplementationManager.Load<FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation>();

            var repo = new CatalogueRepository(new SqlConnectionStringBuilder()
            {
                DataSource = "NonExistant11",
                UserID = "fish",
                Password = "omg",
                ConnectTimeout = 2
            });

            var msg = Assert.Throws<Exception>(()=>repo.TestConnection());

            StringAssert.StartsWith("Testing connection failed",msg.Message);
            StringAssert.DoesNotContain("omg",msg.Message);
            StringAssert.Contains("****",msg.Message);
            StringAssert.Contains("Timeout",msg.Message);
            StringAssert.Contains("2",msg.Message);
        }

        [Test]
        public void TestConnection_NoServer_IntegratedSecurity()
        {
            ImplementationManager.Load<FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation>();

            if(EnvironmentInfo.IsLinux)
                Assert.Inconclusive("Linux doesn't really support IntegratedSecurity and in fact can bomb just setting it on a builder");

            var repo = new CatalogueRepository(new SqlConnectionStringBuilder()
            {
                DataSource = "NonExistant11",
                IntegratedSecurity = true,
                ConnectTimeout = 2
            });

            var msg = Assert.Throws<Exception>(()=>repo.TestConnection());

            StringAssert.StartsWith("Testing connection failed",msg.Message);
            StringAssert.Contains("Integrated Security=",msg.Message);
            StringAssert.Contains("Timeout",msg.Message);
            StringAssert.Contains("2",msg.Message);
        }

    }
}
