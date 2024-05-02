// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandExportLoggedDataToCsv : BasicCommandExecution
{
    private readonly LogViewerFilter _filter;
    private readonly ExternalDatabaseServer[] _loggingServers;

    [UseWithObjectConstructor]
    public ExecuteCommandExportLoggedDataToCsv(IBasicActivateItems activator, LoggingTables table, int idIfAny)
        : this(activator, new LogViewerFilter(table, idIfAny <= 0 ? null : idIfAny))
    {
    }

    public ExecuteCommandExportLoggedDataToCsv(IBasicActivateItems activator, LogViewerFilter filter) : base(activator)
    {
        _filter = filter ?? new LogViewerFilter(LoggingTables.DataLoadTask);
        _loggingServers =
            BasicActivator.RepositoryLocator.CatalogueRepository.GetAllDatabases<LoggingDatabasePatcher>();

        if (!_loggingServers.Any())
            SetImpossible("There are no logging servers");
    }

    public override string GetCommandName()
    {
        return "Export to CSV";
    }

    public override void Execute()
    {
        var db = SelectOne(_loggingServers, null, true);
        var server = db.Discover(DataAccessContext.Logging).Server;

        if (db != null)
        {
            using var con = server.GetConnection();
            con.Open();

            var sql = string.Format(@"SELECT * FROM (
SELECT [dataLoadRunID]
      ,eventType
      ,[description]
      ,[source]
      ,[time]
      ,[ID]
  FROM {0}
  {2}
UNION
SELECT [dataLoadRunID]
      ,'OnError'
      ,[description]
      ,[source]
      ,[time]
      ,[ID]
  FROM {1}
  {2}
 ) as x
order by time ASC", LoggingTables.ProgressLog, LoggingTables.FatalError, _filter.GetWhereSql());

            var output = BasicActivator.SelectFile("Output CSV file");

            var extract = new ExtractTableVerbatim(server, sql, output.Name, output.Directory, ",",
                CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);

            extract.DoExtraction();
        }
    }
}