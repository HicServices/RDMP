using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Diagnostics;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class DiagnosticsTests:DatabaseTests, ICheckNotifier
    {
        [Test]
        public void TestSetupTestDatasetEnvironment()
        {
            string whereIsTheBinDirectory = Assembly.GetExecutingAssembly().Location;
            
            Console.WriteLine("I think the bin directory is in:" + Path.GetDirectoryName(whereIsTheBinDirectory));


            var testEnvironment = new UserAcceptanceTestEnvironment(
                (SqlConnectionStringBuilder) DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder,
                Path.GetDirectoryName(whereIsTheBinDirectory),
                UnitTestLoggingConnectionString,
                "Internal",
                null,
                null, 
                RepositoryLocator) {SilentRunning = true};

            testEnvironment.Check(this);

            testEnvironment.DestroyEnvironment();
        }

        

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if (args.Result == CheckResult.Fail && args.Ex != null)
                throw args.Ex;
            
            if(args.Result == CheckResult.Fail)
                throw new Exception("Setup failed with message :" + args.Message);

            //apply any suggested fixes
            return true;
        }
    }

}
