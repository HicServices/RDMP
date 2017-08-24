using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary;
using DataLoadEngine.Attachers;
using DataLoadEngine.DataFlowPipeline;
using DataLoadEngine.DataFlowPipeline.Destinations;
using DataLoadEngine.DataFlowPipeline.Sources;
using DataLoadEngine.Job;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Attachers
{
    [Description(
    "Populates a data table by reading data out of a remote SQLServer.  The details (username/password) of the account used must be stored in ConfigurationDetails.xml in the Data directory of the HICProjectDirectory.  The Server, Database and Table arguments are specified as regular arguments within the LoadMetadataArguments as normal"
    )]
    public class RemoteSqlServerTableAttacher : RemoteTableAttacher
    {
        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {

        }

        override protected DbConnectionStringBuilder GetConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_remoteUsername))
                return new SqlConnectionStringBuilder()
                {
                    DataSource = RemoteServer,
                    InitialCatalog = RemoteDatabaseName,
                    IntegratedSecurity = true
                };

            return new SqlConnectionStringBuilder()
            {
                DataSource = RemoteServer,
                InitialCatalog = RemoteDatabaseName,
                UserID = _remoteUsername,
                Password = _remotePassword
            };
        }

        protected override DbConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString().ConnectionString);
        }

    }
}
