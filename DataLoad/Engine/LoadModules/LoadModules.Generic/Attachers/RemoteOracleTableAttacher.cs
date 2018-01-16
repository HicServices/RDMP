using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using DataLoadEngine.Attachers;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Attachers
{
    /// <summary>
    /// RemoteTableAttacher implementation for Oracle databases
    /// </summary>
    [Description(
@"Populates a data table by reading data out of a remote Oracle Server.  The details (username/password) of the account used must be stored in ConfigurationDetails.xml in the Data directory of the HICProjectDirectory.  The Server, Database and Table arguments are specified as regular arguments within the LoadMetadataArguments as normal

Paramaters could look something like this:
RemoteServer = '192.168.56.101:1521/TRAININGDB';
RemoteDatabaseName = 'TRAININGDB';
RemoteTableName = 'US_CUSTOMERS_STRUCTURED';

"
)]
    public class RemoteOracleTableAttacher : RemoteTableAttacher
    {
        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
            
        }

        protected override DbConnectionStringBuilder GetConnectionString()
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();

            builder.DataSource = RemoteServer;
            
            
            if(!string.IsNullOrWhiteSpace(_remotePassword))
            {
                builder.UserID = _remoteUsername;
                builder.Password = _remotePassword;
            }
            else
                builder.UserID = "/";

            return builder;
        }

        protected override DbConnection GetConnection()
        {
            return new OracleConnection(GetConnectionString().ConnectionString);
        }
    }
}
