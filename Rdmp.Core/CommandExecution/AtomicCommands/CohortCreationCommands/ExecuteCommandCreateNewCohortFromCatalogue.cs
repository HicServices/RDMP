// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

/// <summary>
///     Generates and runs an SQL query to fetch all private identifiers contained in a dataset and commits them as a new
///     cohort using the specified <see cref="Pipeline" />.  Note that this command will query an entire table, use
///     <see cref="ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration" /> if you want to generate a
///     proper query (e.g. joining multiple tables or only fetching a subset of the table)
/// </summary>
public class ExecuteCommandCreateNewCohortFromCatalogue : CohortCreationCommandExecution
{
    private ExtractionInformation _extractionIdentifierColumn;


    public ExecuteCommandCreateNewCohortFromCatalogue(IBasicActivateItems activator,
        ExtractionInformation extractionInformation) : this(activator)
    {
        if (!extractionInformation.IsExtractionIdentifier)
            SetImpossible("Column is not marked IsExtractionIdentifier");

        OverrideCommandName = "Create New Cohort From Column...";

        SetExtractionIdentifierColumn(extractionInformation);
    }


    public ExecuteCommandCreateNewCohortFromCatalogue(IBasicActivateItems activator, Catalogue catalogue) :
        this(activator)
    {
        if (catalogue != null)
            SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(catalogue));
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewCohortFromCatalogue(IBasicActivateItems activator,
        [DemandsInitialization(
            "Either a Catalogue with a single IsExtractionIdentifier column or a specific ExtractionInformation to query")]
        IMapsDirectlyToDatabaseTable toQuery,
        [DemandsInitialization(Desc_ExternalCohortTableParameter)]
        ExternalCohortTable ect,
        [DemandsInitialization(Desc_CohortNameParameter)]
        string cohortName,
        [DemandsInitialization(Desc_ProjectParameter)]
        Project project,
        [DemandsInitialization(
            "Pipeline for executing the query, performing any required transforms on the output list and allocating release identifiers")]
        IPipeline pipeline) : base(activator, ect, cohortName, project, pipeline)
    {
        UseTripleDotSuffix = true;

        if (toQuery != null)
        {
            if (toQuery is Catalogue c)
                SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(c));
            else if (toQuery is ExtractionInformation ei)
                SetExtractionIdentifierColumn(ei);
            else
                throw new ArgumentException(
                    $"{nameof(toQuery)} must be a Catalogue or an ExtractionInformation but it was a {toQuery.GetType().Name}",
                    nameof(toQuery));
        }
    }

    public override string GetCommandHelp()
    {
        return "Creates a cohort using ALL of the patient identifiers in the referenced dataset";
    }

    public ExecuteCommandCreateNewCohortFromCatalogue(IBasicActivateItems activator)
        : this(activator, null, null, null, null, null)
    {
    }

    public ExecuteCommandCreateNewCohortFromCatalogue(IBasicActivateItems activator,
        ExternalCohortTable externalCohortTable) : this(activator)
    {
        ExternalCohortTable = externalCohortTable;
    }

    public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        switch (target)
        {
            case Catalogue cata:
                SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(cata));
                break;
            case ExtractionInformation ei:
                SetExtractionIdentifierColumn(ei);
                break;
        }

        return base.SetTarget(target);
    }

    private ExtractionInformation GetExtractionInformationFromCatalogue(ICatalogue catalogue)
    {
        var eis = catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

        if (eis.Count(ei => ei.IsExtractionIdentifier) != 1)
        {
            SetImpossible("Catalogue must have a single IsExtractionIdentifier column");
            return null;
        }

        return eis.Single(e => e.IsExtractionIdentifier);
    }

    private void SetExtractionIdentifierColumn(ExtractionInformation extractionInformation)
    {
        //if they are trying to set the identifier column to something that isn't marked IsExtractionIdentifier
        if (_extractionIdentifierColumn != null && !extractionInformation.IsExtractionIdentifier)
            SetImpossible("Column is not marked IsExtractionIdentifier");

        _extractionIdentifierColumn = extractionInformation;
    }

    public override void Execute()
    {
        if (_extractionIdentifierColumn == null)
        {
            var cata = (ICatalogue)BasicActivator.SelectOne("Select Catalogue to create cohort from",
                BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>());

            if (cata == null)
                return;
            SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(cata));
        }

        base.Execute();

        var request =
            GetCohortCreationRequest(ExtractableCohortAuditLogBuilder.GetDescription(_extractionIdentifierColumn));

        //user choose to cancel the cohort creation request dialogue
        if (request == null)
            return;

        request.ExtractionIdentifierColumn = _extractionIdentifierColumn;
        var configureAndExecute = GetConfigureAndExecuteControl(request,
            $"Import column {_extractionIdentifierColumn} as cohort and commit results", _extractionIdentifierColumn);

        configureAndExecute.Run(BasicActivator.RepositoryLocator, null, null, null);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);
    }
}