// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Threading.Tasks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.CohortDescribing;

/// <summary>
///     Async class for fetching the number of unique patients / custom tables in every cohort (ExtractableCohort) in a
///     cohort database (ExternalCohortTable)
/// </summary>
public class CohortDescriptionDataTableAsyncFetch
{
    public ExternalCohortTable Source { get; }
    public DataTable DataTable { get; }
    public Task Task { get; private set; }


    public event Action Finished;

    public CohortDescriptionDataTableAsyncFetch(ExternalCohortTable source)
    {
        Source = source;
        DataTable = new DataTable();
        DataTable.BeginLoadData();
    }


    public void Begin()
    {
        Task = new Task(() =>
        {
            var server = DataAccessPortal.ExpectDatabase(Source, DataAccessContext.DataExport).Server;
            using var con = server.GetConnection();
            con.Open();
            using var cmd = server.GetCommand(Source.GetCountsDataTableSql(), con);
            cmd.CommandTimeout = 120; //give it up to 2 minutes
            server.GetDataAdapter(cmd).Fill(DataTable);
            DataTable.EndLoadData();
        });

        Task.ContinueWith(s => { Finished?.Invoke(); });

        Task.Start();
    }
}