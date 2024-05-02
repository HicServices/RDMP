// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

/// <summary>
///     Renames DataTables in the pipeline so that they do not collide with any tables at the destination database.  This
///     is done by appending V1,V2,V3 etc to the table
/// </summary>
public class TableVersionNamer : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<DiscoveredDatabase>
{
    private string[] _tableNamesAtDestination;

    [DemandsInitialization("Suffix pattern, $v is the version, default is _V$v as in MyTable_V1, MyTable_V2 etc",
        DemandType.Unspecified, "_V$v")]
    public string Suffix { get; set; }

    [DemandsInitialization("The maximum number of tables to allow, defaults to 1000 so MyTable_V1 up to MyTable_V1000",
        DemandType.Unspecified, 1000)]
    public int MaximumNumberOfVersionsAllowed { get; set; }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        toProcess.TableName = GetVersionedTableName(toProcess.TableName);
        return toProcess;
    }

    private string GetVersionedTableName(string tableName)
    {
        for (var i = 1; i <= MaximumNumberOfVersionsAllowed; i++)
        {
            var candidate = tableName + Suffix.Replace("$v", i.ToString());
            if (!_tableNamesAtDestination.Any(t => t.Equals(candidate, StringComparison.CurrentCultureIgnoreCase)))
                return candidate;
        }

        throw new Exception(
            $"Unable to find unique table name after {MaximumNumberOfVersionsAllowed} TableNames were tried");
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        if (MaximumNumberOfVersionsAllowed == 0)
            notifier.OnCheckPerformed(
                new CheckEventArgs("MaximumNumberOfVersionsAllowed cannot be 0", CheckResult.Fail));
    }

    public void PreInitialize(DiscoveredDatabase value, IDataLoadEventListener listener)
    {
        _tableNamesAtDestination = value.DiscoverTables(true).Select(t => t.GetRuntimeName()).ToArray();
    }
}