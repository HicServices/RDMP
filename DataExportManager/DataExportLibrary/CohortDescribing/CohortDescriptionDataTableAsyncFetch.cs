using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.DataAccess;

namespace DataExportLibrary.CohortDescribing
{
    /// <summary>
    /// Async class for fetching the number of unique patients / custom tables in every cohort (ExtractableCohort) in a cohort database (ExternalCohortTable)
    /// </summary>
    public class CohortDescriptionDataTableAsyncFetch
    {
        public ExternalCohortTable Source { get; private set; }
        public DataTable DataTable { get; private set; }
        public DataTable CustomDataTable { get; private set; }
        public Task Task { get; private set; }


        public event Action Finished;

        public CohortDescriptionDataTableAsyncFetch(ExternalCohortTable source)
        {
            Source = source;
            DataTable = new DataTable();
            CustomDataTable = new DataTable();
        }


        public void Begin()
        {
            Task = new Task(() =>
            {
                var server = DataAccessPortal.GetInstance().ExpectDatabase(Source, DataAccessContext.DataExport).Server;
                using (var con = server.GetConnection())
                {
                    con.Open();
                    var cmd = server.GetCommand(Source.GetCountsDataTableSql(), con);
                    cmd.CommandTimeout = 120; //give it up to 2 minutes
                    server.GetDataAdapter(cmd).Fill(DataTable);


                    var cmd2 = server.GetCommand(Source.GetCustomTableSql(), con);
                    cmd2.CommandTimeout = 120; //give it up to 2 minutes
                    server.GetDataAdapter(cmd2).Fill(CustomDataTable);
                }
                
            });

            Task.ContinueWith(s =>
            {
                if (Finished != null)
                    Finished();
            });

            Task.Start();
        }

    }
}
