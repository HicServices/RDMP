// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;

/// <summary>
///     Checker for validating the anonymisation configuration of a TableInfo.  This includes iterating all columns which
///     have ANOTables configured (See ANOTable)
///     and checking that the database has the correct columns / datatypes etc).  Also checks the IdentifierDumper.
/// </summary>
public class ANOTableInfoSynchronizer
{
    private readonly ITableInfo _tableToSync;

    public ANOTableInfoSynchronizer(ITableInfo tableToSync)
    {
        _tableToSync = tableToSync;
    }

    public void Synchronize(ICheckNotifier notifier)
    {
        var dumper = new IdentifierDumper(_tableToSync);
        dumper.Check(notifier);

        CheckForDuplicateANOVsRegularNames();

        var columnInfosWithANOTransforms = _tableToSync.ColumnInfos.Where(c => c.ANOTable_ID != null).ToArray();

        if (!columnInfosWithANOTransforms.Any())
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "There are no ANOTables configured for this table so skipping ANOTable checking",
                    CheckResult.Success));

        foreach (var columnInfoWithANOTransform in columnInfosWithANOTransforms)
        {
            var anoTable = columnInfoWithANOTransform.ANOTable;
            anoTable.Check(ThrowImmediatelyCheckNotifier.Quiet);

            if (!anoTable.GetRuntimeDataType(LoadStage.PostLoad).Equals(columnInfoWithANOTransform.Data_type))
                throw new ANOConfigurationException(
                    $"Mismatch between anoTable.GetRuntimeDataType(LoadStage.PostLoad) = {anoTable.GetRuntimeDataType(LoadStage.PostLoad)} and column {columnInfoWithANOTransform} datatype = {columnInfoWithANOTransform.Data_type}");

            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"ANOTable {anoTable} has shared compatible datatype {columnInfoWithANOTransform.Data_type} with ColumnInfo {columnInfoWithANOTransform}",
                    CheckResult.Success));
        }
    }

    private void CheckForDuplicateANOVsRegularNames()
    {
        //make sure he doesn't have 2 columns called MyCol and ANOMyCol in the same table as this will break RAW creation and is symptomatic of a botched anonymisation configuration change
        var colNames = _tableToSync.ColumnInfos.Select(c => c.GetRuntimeName()).ToArray();
        var duplicates = colNames.Where(c => colNames.Any(c2 => c2.Equals(ANOTable.ANOPrefix + c))).ToArray();

        if (duplicates.Any())
            throw new ANOConfigurationException(
                $"The following columns exist both in their identifiable state and ANO state in TableInfo {_tableToSync} (this is not allowed).  The offending column(s) are:{string.Join(",", duplicates.Select(s => $"'{s}' & '{ANOTable.ANOPrefix}{s}'"))}");
    }
}