// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using Diagnostics;
using Rdmp.Core.CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace Tests.Common
{
    /// <summary>
    /// Prepares a test dataset environment configured according to test config settings. Intended for use at test fixture level, i.e. shared by multiple tests
    /// </summary>
    public class UserAcceptanceTestEnvironmentHelper
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly SqlConnectionStringBuilder _scratchServerConnectionString;
        private readonly SqlConnectionStringBuilder _catalogueConnectionString;
        private readonly SqlConnectionStringBuilder _loggingConnectionString;
        private readonly SqlConnectionStringBuilder _anoConnectionString;
        private readonly SqlConnectionStringBuilder _identifierDumpConnectionString;

        private bool _isSetUp;
        private TestDirectoryHelper _testDirectoryHelper;

        private UserAcceptanceTestEnvironment _testEnvironment;
        public UserAcceptanceTestEnvironment TestEnvironment
        {
            get
            {
                if (!_isSetUp)
                    throw new Exception("The helper has not been set up yet, call SetUp first");
                return _testEnvironment;
            }
        }
        
        public UserAcceptanceTestEnvironmentHelper(string scratchServerConnectionString, string catalogueConnectionString, string loggingConnectionString, string anoConnectionString, string identifierDumpConnectionString, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
            _scratchServerConnectionString  = new SqlConnectionStringBuilder(scratchServerConnectionString);
            _catalogueConnectionString      = new SqlConnectionStringBuilder(catalogueConnectionString);
            _loggingConnectionString        = new SqlConnectionStringBuilder(loggingConnectionString);
            _anoConnectionString            = new SqlConnectionStringBuilder(anoConnectionString);
            _identifierDumpConnectionString = new SqlConnectionStringBuilder(identifierDumpConnectionString);
        }

        public void SetUp()
        {
            // ensure we have a clean catalogue (more needed than just this)
            var catalogueDbName = _catalogueConnectionString.InitialCatalog;

            if (string.IsNullOrWhiteSpace(catalogueDbName))
                throw new Exception("The catalogue database name is not set, make sure you are passing the correct connection string");
            
            // Create catalogue
            _testDirectoryHelper = new TestDirectoryHelper(GetType());
            _testDirectoryHelper.SetUp();

            var datasetRoot = _testDirectoryHelper.Directory.CreateSubdirectory("TestDatasetEnvironment");
            
            // ANO
            _testEnvironment = new UserAcceptanceTestEnvironment(_scratchServerConnectionString, datasetRoot.FullName,
                _loggingConnectionString, "Internal", _anoConnectionString, _identifierDumpConnectionString, _repositoryLocator);
            _testEnvironment.Check(new AcceptAllCheckNotifier());

            _isSetUp = true;
        }

        public void TearDown()
        {
            _testEnvironment.DestroyEnvironment();

            // other
            _testDirectoryHelper.TearDown();

        }
    }
}