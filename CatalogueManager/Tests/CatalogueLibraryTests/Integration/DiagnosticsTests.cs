// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Diagnostics;
using Diagnostics.TestData.Exercises;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
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
                RepositoryLocator);

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

        [Test]
        public void TestGaussian()
        {
            var t = new CarotidArteryScanReportExerciseTestData();
            
            double[] a = new double[10000];

            a = a.Select(e => t.GetGaussian()).OrderBy(e => e).ToArray();

            Console.WriteLine(string.Join(Environment.NewLine, a));
        }
        [Test]
        public void TestGaussianRange()
        {
            var t = new CarotidArteryScanReportExerciseTestData();
            
            double[] a = new double[1000];

            a = a.Select(e => t.GetGaussian(10,20)).OrderBy(e => e).ToArray();

            Console.WriteLine(string.Join(Environment.NewLine, a));
        }
    }

}
