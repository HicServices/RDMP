// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;

/// <summary>
/// Extraction source which creates a PrimaryKey on the DataTable being extracted.  This is based on <see cref="IColumn.IsPrimaryKey"/> of the
/// columns extracted and is not guaranteed to actually be unique (depending on how you have configured the flags).
/// 
/// <para>The primary use case for this is when extracting to database where you want to have meaningful primary keys</para>
/// </summary>
public class ExecutePkSynthesizerDatasetExtractionSource : ExecuteDatasetExtractionSource
{
    private const string SYNTH_PK_COLUMN = "SynthesizedPk";
    private bool _synthesizePkCol;

    public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
    {
        // let's look for primary keys in the Extraction Information
        var cataloguePrimaryKeys = GetCatalogueItemPrimaryKeys();

        // if there are some they will be marked in the "GetChunk".
        // If there are none, then we need to synth a new column here.
        if (!cataloguePrimaryKeys.Any())
        {
            var primaryKeys = GetColumnInfoPrimaryKeys().ToArray();

            if (primaryKeys.Any())
            {
                string newSql;
                if (primaryKeys.Length > 1) // no need to do anything if there is only one.
                    newSql = $"CONCAT({string.Join(",'_',", primaryKeys.Select(apk => apk.ToString()))})";
                else
                    newSql = primaryKeys.Single().Name;

                var syntaxHelper = Request.Catalogue.GetQuerySyntaxHelper();

                Request.QueryBuilder.AddColumn(
                    new SpontaneouslyInventedColumn(new MemoryRepository(), SYNTH_PK_COLUMN,
                        syntaxHelper.HowDoWeAchieveMd5(newSql))
                    {
                        HashOnDataRelease = true,
                        IsPrimaryKey = true,
                        Order = -1
                    });
                _synthesizePkCol = true;
            }
        }

        return Request.QueryBuilder.SQL;
    }

    private IEnumerable<ITableInfo> GetProperTables()
    {
        if (Request.QueryBuilder.SQLOutOfDate)
            Request.QueryBuilder.RegenerateSQL();

        return Request.QueryBuilder.TablesUsedInQuery.Where(ti => !ti.IsLookupTable());
    }

    public override DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        var chunk = base.GetChunk(listener, cancellationToken);
        if (GlobalsRequest != null)
            return chunk;

        if (chunk == null)
            return null;

        var catalogueItemPkColumns = GetCatalogueItemPrimaryKeys().Select(c => c.GetRuntimeName()).ToArray();

        if (catalogueItemPkColumns.Any())
            chunk.PrimaryKey = chunk.Columns.Cast<DataColumn>().Where(c =>
                catalogueItemPkColumns.Contains(c.ColumnName, StringComparer.CurrentCultureIgnoreCase)).ToArray();
        else if (_synthesizePkCol)
            chunk.PrimaryKey = new[] { chunk.Columns[SYNTH_PK_COLUMN] };

        return chunk;
    }

    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);
        if (Request == null ||
            Request == ExtractDatasetCommand
                .EmptyCommand) // it is the globals, and there is no PK involved in there... although there should be...
            return;

        var cataloguePrimaryKeys = GetCatalogueItemPrimaryKeys().ToArray();
        if (!cataloguePrimaryKeys.Any())
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"PKSynthesizer:No CatalogueItems marked IsPrimaryKey in '{Request.SelectedDataSets}'",
                CheckResult.Warning));

            var columnInfoPrimaryKeys = GetColumnInfoPrimaryKeys().ToArray();

            if (columnInfoPrimaryKeys.Any())
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"PKSynthesizer:Found ColumnInfo(s) marked IsPrimaryKey in '{Request.SelectedDataSets}'{string.Join(",", columnInfoPrimaryKeys.Select(c => c.Name))}",
                    CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"PKSynthesizer:No ColumnInfo marked IsPrimaryKey in '{Request.SelectedDataSets}'",
                    CheckResult.Fail));
        }
        else
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"PKSynthesizer:Found CatalogueItem(s) marked IsPrimaryKey in '{Request.SelectedDataSets}'{string.Join(",", cataloguePrimaryKeys.Select(c => c.GetRuntimeName()))}",
                CheckResult.Success));
        }
    }

    private IEnumerable<IColumn> GetCatalogueItemPrimaryKeys()
    {
        foreach (var column in Request.ColumnsToExtract.Union(Request.ReleaseIdentifierSubstitutions))
            switch (column)
            {
                case ReleaseIdentifierSubstitution ri when ri.IsPrimaryKey || ri.OriginalDatasetColumn.IsPrimaryKey:
                    yield return ri;

                    break;
                case ExtractableColumn { IsPrimaryKey: true } ec:
                    yield return ec;

                    break;
            }
    }

    private IEnumerable<ColumnInfo> GetColumnInfoPrimaryKeys()
    {
        return GetProperTables().SelectMany(static t => t.ColumnInfos).Where(static column => column.IsPrimaryKey);
    }
}