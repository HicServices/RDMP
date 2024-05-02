// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
///     Similar to <see cref="ExecuteSqlFileRuntimeTask" /> but doesn't require the Sql to be stored on disk.  Instead the
///     SQL is stored
///     in the property <see cref="Sql" /> (i.e. in the RMDP platform database).
/// </summary>
public class ExecuteSqlMutilation : IPluginMutilateDataTables
{
    private DiscoveredDatabase _db;
    private LoadStage _loadStage;

    [DemandsInitialization("Run the following SQL when this component is run in the DLE", DemandType = DemandType.SQL,
        Mandatory = true)]
    public string Sql { get; set; }

    public void Check(ICheckNotifier notifier)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _db = dbInfo;
        _loadStage = loadStage;
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var sql = new ExecuteSqlInDleStage(job, _loadStage);
        return sql.Execute(Sql, _db);
    }
}