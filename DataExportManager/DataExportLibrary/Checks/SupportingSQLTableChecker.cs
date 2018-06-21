using System;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    /// <summary>
    /// Checks that a given <see cref="SupportingSQLTable"/> is reachable by the current user and that the <see cref="SupportingSQLTable.SQL"/> can be executed and returns data.
    /// </summary>
    public class SupportingSQLTableChecker:ICheckable
    {
        private readonly SupportingSQLTable _table;

        public SupportingSQLTableChecker(SupportingSQLTable table)
        {
            _table = table;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Found SupportingSQLTable " + _table + " about to check it", CheckResult.Success));

                var supportingSQLServer = _table.GetServer();

                notifier.OnCheckPerformed(supportingSQLServer.Exists()
                    ? new CheckEventArgs("Server " + supportingSQLServer + " exists", CheckResult.Success)
                    : new CheckEventArgs("Server " + supportingSQLServer + " does not exist", CheckResult.Fail));

                using (var con = _table.GetServer().GetConnection())
                {
                    con.Open();

                    notifier.OnCheckPerformed(new CheckEventArgs("About to check Extraction SQL:" + _table.SQL, CheckResult.Success));

                    var reader = supportingSQLServer.GetCommand(_table.SQL, con).ExecuteReader();
                    if (reader.Read())
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "SupportingSQLTable table fetched successfully and at least 1 data row was read ",
                                CheckResult.Success));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("No data was successfully read from SupportingSQLTable " + _table, CheckResult.Fail));
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Checking of SupportingSQLTable " + _table + " failed with Exception", CheckResult.Fail, e));
            }

        }
    }
}