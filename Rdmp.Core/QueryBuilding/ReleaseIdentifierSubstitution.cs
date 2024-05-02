// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text.RegularExpressions;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Records how (via SQL) replace the private patient identifier column (e.g. CHI) with the release identifier (e.g.
///     swap [biochemistry]..[chi] for
///     [cohort]..[ReleaseId]).  Also includes the Join SQL string for linking the cohort table (which contains the
///     ReleaseId e.g. [cohort]) with the dataset
///     table (e.g. [biochemistry]).
///     <para>
///         This class is an IColumn and is designed to be added as a new Column to a QueryBuilder as normal (See
///         ExtractionQueryBuilder)
///     </para>
/// </summary>
public class ReleaseIdentifierSubstitution : SpontaneousObject, IColumn
{
    public string JoinSQL { get; private set; }

    /// <summary>
    ///     The identifiable column which is being substituted on
    /// </summary>
    public IColumn OriginalDatasetColumn;

    private readonly IQuerySyntaxHelper _querySyntaxHelper;

    [Sql] public string SelectSQL { get; set; }

    public string Alias { get; }

    //all these are hard coded to null or false really
    public ColumnInfo ColumnInfo => OriginalDatasetColumn.ColumnInfo;

    public int Order
    {
        get => OriginalDatasetColumn.Order;
        set { }
    }

    public bool HashOnDataRelease => false;
    public bool IsExtractionIdentifier => OriginalDatasetColumn.IsExtractionIdentifier;
    public bool IsPrimaryKey => OriginalDatasetColumn.IsPrimaryKey;

    public ReleaseIdentifierSubstitution(MemoryRepository repo, IColumn extractionIdentifierToSubFor,
        IExtractableCohort extractableCohort, bool isPartOfMultiCHISubstitution,
        IQuerySyntaxHelper querySyntaxHelper) : base(repo)
    {
        if (!extractionIdentifierToSubFor.IsExtractionIdentifier)
            throw new Exception(
                $"Column {extractionIdentifierToSubFor} is not marked IsExtractionIdentifier so cannot be substituted for a ReleaseIdentifier");

        OriginalDatasetColumn = extractionIdentifierToSubFor;
        _querySyntaxHelper = querySyntaxHelper;
        if (OriginalDatasetColumn.ColumnInfo == null)
            throw new Exception(
                $"The column {OriginalDatasetColumn.GetRuntimeName()} references a ColumnInfo that has been deleted");

        var syntaxHelper = extractableCohort.GetQuerySyntaxHelper();

        //the externally referenced Cohort table
        var privateCol = extractableCohort.ExternalCohortTable.DiscoverPrivateIdentifier();

        var collateStatement = "";

        //the release identifier join might require collation

        //if the private has a collation
        if (!string.IsNullOrWhiteSpace(privateCol.Collation))
        {
            var cohortCollation = privateCol.Collation;
            var otherTableCollation = OriginalDatasetColumn.ColumnInfo.Collation;


            //only collate if the server types match and if the collations differ
            if (privateCol.Table.Database.Server.DatabaseType ==
                OriginalDatasetColumn.ColumnInfo.TableInfo.DatabaseType)
                if (!string.IsNullOrWhiteSpace(otherTableCollation) &&
                    !string.Equals(cohortCollation, otherTableCollation))
                    collateStatement = $" collate {cohortCollation}";
        }


        if (!isPartOfMultiCHISubstitution)
        {
            SelectSQL = extractableCohort.GetReleaseIdentifier();
            Alias = syntaxHelper.GetRuntimeName(SelectSQL);
        }
        else
        {
            SelectSQL =
                $"(SELECT DISTINCT {extractableCohort.GetReleaseIdentifier()} FROM {privateCol.Table.GetFullyQualifiedName()} WHERE {extractableCohort.WhereSQL()} AND {privateCol.GetFullyQualifiedName()}={OriginalDatasetColumn.SelectSQL}{collateStatement})";

            if (!string.IsNullOrWhiteSpace(OriginalDatasetColumn.Alias))
            {
                var toReplace = extractableCohort.GetPrivateIdentifier(true);
                var toReplaceWith = extractableCohort.GetReleaseIdentifier(true);

                //take the same name as the underlying column
                Alias = OriginalDatasetColumn.Alias;

                //but replace all instances of CHI with PROCHI (or Barcode, or whatever)
                if (!Alias.Contains(toReplace) || Regex.Matches(Alias, Regex.Escape(toReplace)).Count > 1)
                    throw new Exception(
                        $"Failed to resolve multiple extraction identifiers in dataset.  Either mark a single column as the IsExtractionIdentifier for this extraction or ensure all columns are of compatible type and have the text \"{toReplace}\" appearing once (and only once in its name)");

                Alias = Alias.Replace(toReplace, toReplaceWith);
            }
            else
            {
                throw new Exception(
                    $"In cases where you have multiple columns marked IsExtractionIdentifier, they must all have Aliases, the column {OriginalDatasetColumn.SelectSQL} does not have one");
            }
        }

        JoinSQL = $"{OriginalDatasetColumn.SelectSQL}={privateCol.GetFullyQualifiedName()}{collateStatement}";
    }

    public string GetRuntimeName()
    {
        return string.IsNullOrWhiteSpace(Alias) ? _querySyntaxHelper.GetRuntimeName(SelectSQL) : Alias;
    }

    public void Check(ICheckNotifier notifier)
    {
        new ColumnSyntaxChecker(this).Check(notifier);
    }
}