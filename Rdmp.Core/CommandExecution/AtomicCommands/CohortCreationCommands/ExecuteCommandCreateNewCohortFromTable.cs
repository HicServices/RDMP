// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

/// <summary>
///     Generates and runs an SQL query to fetch all private identifiers contained in a table
///     and commits them as a new cohort using the specified <see cref="Pipeline" />.  Note that
///     this command will query an entire table, use
///     <see cref="ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration" />
///     if you want to generate a proper query (e.g. joining multiple tables or only fetching a subset of the table)
/// </summary>
public class ExecuteCommandCreateNewCohortFromTable : CohortCreationCommandExecution
{
    /// <summary>
    ///     Prompts user at runtime to pick a table and column which are then imported as a new cohort
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="externalCohortTable"></param>
    public ExecuteCommandCreateNewCohortFromTable(IBasicActivateItems activator,
        ExternalCohortTable externalCohortTable) : base(activator)
    {
        UseTripleDotSuffix = true;
        ExternalCohortTable = externalCohortTable;
    }

    public override string GetCommandHelp()
    {
        return "Creates a cohort using ALL of the patient identifiers in the referenced table";
    }


    public override void Execute()
    {
        var tbl = BasicActivator.SelectTable(false, "Pick a table to import cohorts from");

        if (tbl == null)
            // user cancelled selecting a table
            return;

        if (!BasicActivator.SelectObject(new DialogArgs
            {
                EntryLabel = "Patient Identifier Column",
                TaskDescription =
                    $"Select which column in the table '{tbl.GetFullyQualifiedName()}' contains the patient identifiers which you want to import",
                AllowAutoSelect = true
            }, tbl.DiscoverColumns(), out var col))
            // user cancelled selecting a column
            return;

        base.Execute();

        var request = GetCohortCreationRequest(ExtractableCohortAuditLogBuilder.GetDescription(col));

        //user choose to cancel the cohort creation request dialogue
        if (request == null)
            return;

        var m = new MemoryCatalogueRepository();
        var fakeCatalogue = new Catalogue(m, tbl.GetFullyQualifiedName());
        var fakeCatalogueItem = new CatalogueItem(m, fakeCatalogue, col.GetRuntimeName());
        var fakeTableInfo = new TableInfo(m, tbl.GetFullyQualifiedName())
        {
            DatabaseType = tbl.Database.Server.DatabaseType,
            Database = tbl.Database.GetRuntimeName(),
            Server = tbl.Database.Server.Name
        };
        var fakeColumnInfo = new ColumnInfo(m, col.GetFullyQualifiedName(), col.DataType.ToString(), fakeTableInfo);
        var fakeExtractionInformation = new ExtractionInformation(m, fakeCatalogueItem, fakeColumnInfo,
            col.GetFullyQualifiedName())
        {
            IsExtractionIdentifier = true
        };

        request.ExtractionIdentifierColumn = fakeExtractionInformation;
        var configureAndExecute = GetConfigureAndExecuteControl(request,
            $"Import column {col.GetFullyQualifiedName()} as cohort and commit results", fakeExtractionInformation);

        configureAndExecute.Run(BasicActivator.RepositoryLocator, null, null, null);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Import);
    }
}