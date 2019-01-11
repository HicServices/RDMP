using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAnsi.Discovery;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace HIC.Logging.Tests.Integration
{
    class DataLoadTaskHelper
    {
        private readonly DiscoveredServer _loggingServer;
        private readonly Stack<string> _sqlToCleanUp = new Stack<string>();

        public DataLoadTaskHelper(DiscoveredServer loggingServer)
        {
            _loggingServer = loggingServer;
        }

        public void SetUp()
        {
            var checker = new LoggingDatabaseChecker(_loggingServer);
            checker.Check(new AcceptAllCheckNotifier());
        }

        public void CreateDataLoadTask(string taskName)
        {
            using (var con =_loggingServer.GetConnection())
            {
                con.Open();

                var datasetName = "Test_" + taskName;
                var datasetCmd = _loggingServer.GetCommand("INSERT INTO DataSet (dataSetID) VALUES ('" + datasetName + "')", con);
                datasetCmd.ExecuteNonQuery();
                _sqlToCleanUp.Push("DELETE FROM DataSet WHERE dataSetID = '" + datasetName + "'");

                var taskCmd =
                    _loggingServer.GetCommand(
                        "INSERT INTO DataLoadTask VALUES (100, '" + taskName + "', '" + taskName + "', GETDATE(), '" + datasetName + "', 1, 1, '" + datasetName + "')",
                        con);

                taskCmd.ExecuteNonQuery();
                _sqlToCleanUp.Push("DELETE FROM DataLoadTask WHERE dataSetID = '" + datasetName + "'");
            }
        }

        public void TearDown()
        {
            using (var con = _loggingServer.GetConnection())
            {
                con.Open();

                while (_sqlToCleanUp.Any())
                    _loggingServer.GetCommand(_sqlToCleanUp.Pop(), con).ExecuteNonQuery();
            }

        }
    }
}
